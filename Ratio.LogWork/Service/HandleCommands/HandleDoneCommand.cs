using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands
{
    public class HandleDoneCommand : IHandleCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();

        public HandleDoneCommand(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(WorkLogRequest workLogRequest)
        {
            // Get current active task
            WorkLog? taskToFinish = null;
            if (!string.IsNullOrWhiteSpace(workLogRequest.TaskID))
            {
                // Find by taskId
                var tasksInProject = await _unitOfWork.GetRepository<WorkLog>()
                                .GetAll()
                                .Where(x => x.WorkingProjectId == workLogRequest.WorkingProjectId
                                            && x.Status == WorkLogStatus.Active)
                                .ToListAsync();

                taskToFinish = tasksInProject.OrderBy(x=>x.CreatedDate).LastOrDefault(x => x.TaskID == workLogRequest.TaskID) == null
                    ? tasksInProject.OrderBy(x=>x.CreatedDate).LastOrDefault(x => x.TaskID.Contains(workLogRequest.TaskID))
                    : tasksInProject.OrderBy(x=>x.CreatedDate).LastOrDefault(x => x.TaskID == workLogRequest.TaskID);
            }

            if (taskToFinish == null && !string.IsNullOrWhiteSpace(workLogRequest.Name))
            {
                // Still not found => find by name
                taskToFinish = await _unitOfWork.GetRepository<WorkLog>()
                                .GetAll()
                                .OrderBy(x=>x.CreatedDate).LastOrDefaultAsync(x => x.WorkingProjectId == workLogRequest.WorkingProjectId
                                    && x.Status == WorkLogStatus.Active
                                    && (x.Name.ToLower().Equals(workLogRequest.Name.ToLower())
                                    || x.Name.ToLower().Contains(workLogRequest.Name.ToLower())));
            }

            if (taskToFinish == null)
            {
                // Get latest active task
                taskToFinish = await _unitOfWork.GetRepository<WorkLog>()
                    .GetAll()
                    .Where(x => x.Status == WorkLogStatus.Active && x.WorkingProjectId == workLogRequest.WorkingProjectId)
                    .OrderByDescending(x=>x.CreatedDate)
                    .FirstOrDefaultAsync();

                if (taskToFinish == null)
                {
                    _logger.LogError("Incorrect commands. Not found task to finish");
                    return;
                }
            }

            // Add history logs
            var logHistories = GetHistoriesLogs(taskToFinish);

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

                        // Update workLog: status and time:
                        var workLogHistories = await _unitOfWork.GetRepository<WorkLogHistory>().FindAsync(x => x.WorkLogId == taskToFinish.Id);
                        var allHistories = workLogHistories.Concat(logHistories).ToList();

                        taskToFinish.Status = WorkLogStatus.Done;
                        taskToFinish.EndDate = DateTime.UtcNow;
                        taskToFinish.WorkingHour = WorkLogHelper.CalculateWorkingHour(allHistories);

                        await _unitOfWork.GetRepository<WorkLog>().UpdateAsync(taskToFinish);

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
                _logger.LogError(ex, "Error pause work log and add history entry for request: {RequestCommand}", workLogRequest.FullCommand);                               
                return;
            }
        }

        private IEnumerable<WorkLogHistory> GetHistoriesLogs(WorkLog taskToFinish)
        {
            var results = new List<WorkLogHistory>();

            if (taskToFinish == null) return results;

            // Finish current active
            results.Add(new WorkLogHistory
            {
                Action = WorkLogHistoryAction.Done,
                CreatedDate = DateTime.UtcNow,
                WorkLogId = taskToFinish.Id,
                WorkLogEntity = taskToFinish
            });

            return results;
        }
    }
}
