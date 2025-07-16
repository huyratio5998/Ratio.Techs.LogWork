using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands
{
    public class HandleContinueCommand : IHandleCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();

        public HandleContinueCommand(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(WorkLogRequest workLogRequest)
        {
            // Get task need continue
            WorkLog? taskNeedContinue = null;
            if (!string.IsNullOrWhiteSpace(workLogRequest.TaskID))
            {
                // Find by taskId
                var tasksInProject = await _unitOfWork.GetRepository<WorkLog>()
                                .GetAll()
                                .Where(x => x.WorkingProjectId == workLogRequest.WorkingProjectId)
                                .ToListAsync();

                taskNeedContinue = tasksInProject.OrderBy(x => x.CreatedDate).LastOrDefault(x => x.TaskID == workLogRequest.TaskID) == null
                    ? tasksInProject.OrderBy(x => x.CreatedDate).LastOrDefault(x => x.TaskID.Contains(workLogRequest.TaskID))
                    : tasksInProject.OrderBy(x => x.CreatedDate).LastOrDefault(x => x.TaskID == workLogRequest.TaskID);
            }

            if (taskNeedContinue == null && !string.IsNullOrWhiteSpace(workLogRequest.Name))
            {
                // Still not found => find by name
                taskNeedContinue = await _unitOfWork.GetRepository<WorkLog>()
                                .GetAll()
                                .OrderBy(x => x.CreatedDate).LastOrDefaultAsync(x =>
                                    x.WorkingProjectId == workLogRequest.WorkingProjectId
                                    && (x.Name.ToLower().Equals(workLogRequest.Name.ToLower())
                                        || x.Name.ToLower().Contains(workLogRequest.Name.ToLower())
                                       )
                                 );
            }

            if (taskNeedContinue == null)
            {
                // Continue latest workLog paused
                taskNeedContinue = await _unitOfWork.GetRepository<WorkLog>()
                    .GetAll()
                    .Where(x => x.WorkingProjectId == workLogRequest.WorkingProjectId
                                && (x.Status == WorkLogStatus.Paused || x.Status == WorkLogStatus.Active)
                          )
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefaultAsync();

                if (taskNeedContinue == null)
                {
                    _logger.LogError("Incorrect commands. Not found task to continue");
                    return;
                }
                else if (taskNeedContinue.Status == WorkLogStatus.Active)
                {
                    // because continue active task => no need to do anything.
                    _logger.LogInformation("Handled comand. Do nothing");
                }
            }

            // Get current active
            var currentActiveTask = await _unitOfWork.GetRepository<WorkLog>()
                .GetAll()
                .FirstOrDefaultAsync(w =>
                    w.Status == WorkLogStatus.Active && w.WorkingProjectId == workLogRequest.WorkingProjectId);

            // Add history logs
            var logHistories = GetHistoriesLogs(taskNeedContinue, currentActiveTask);

            // Start add to DB in a transaction
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        await _unitOfWork.GetRepository<WorkLogHistory>().AddRangeAsync(logHistories);

                        // Pause if continues another task.
                        if (currentActiveTask != null && currentActiveTask.Id != taskNeedContinue.Id)
                        {
                            var workLogHistories = await _unitOfWork.GetRepository<WorkLogHistory>().FindAsync(x => x.WorkLogId == currentActiveTask.Id);
                            var allHistories = workLogHistories.Concat(logHistories).ToList();

                            currentActiveTask.Status = WorkLogStatus.Paused;
                            currentActiveTask.WorkingHour = WorkLogHelper.CalculateWorkingHour(allHistories);
                            await _unitOfWork.GetRepository<WorkLog>().UpdateAsync(currentActiveTask);
                        }

                        // Update status only
                        taskNeedContinue.Status = WorkLogStatus.Active;
                        await _unitOfWork.GetRepository<WorkLog>().UpdateAsync(taskNeedContinue);

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitAsync();
                    }
                    catch
                    {
                        await _unitOfWork.RollbackAsync();
                        throw;
                    }
                });
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error continue work log and add history for request: {RequestCommand}", workLogRequest.FullCommand);
                return;
            }
        }

        private IEnumerable<WorkLogHistory> GetHistoriesLogs(WorkLog taskNeedContinue, WorkLog? currentActiveTask)
        {
            var results = new List<WorkLogHistory>();

            if (taskNeedContinue == null) return results;

            if (currentActiveTask != null && currentActiveTask.Id != taskNeedContinue.Id)
            {
                // Pause current active
                results.Add(new WorkLogHistory
                {
                    Action = WorkLogHistoryAction.Pause,
                    CreatedDate = DateTime.UtcNow,
                    WorkLogId = currentActiveTask.Id,
                    WorkLogEntity = currentActiveTask
                });
            }

            // Continue task
            results.Add(new WorkLogHistory
            {
                Action = WorkLogHistoryAction.Continue,
                CreatedDate = DateTime.UtcNow,
                WorkLogId = taskNeedContinue.Id,
                WorkLogEntity = taskNeedContinue
            });

            return results;
        }
    }
}
