using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands
{
    public class HandlePauseCommand : IHandleCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();

        public HandlePauseCommand(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(WorkLogRequest workLogRequest)
        {
            // Get task need pause
            WorkLog? taskNeedPause = null;
            if (!string.IsNullOrWhiteSpace(workLogRequest.TaskID))
            {
                // Find by taskId
                var tasksInProject = await _unitOfWork.GetRepository<WorkLog>()
                                .GetAll()
                                .Where(x => x.WorkingProjectId == workLogRequest.WorkingProjectId
                                            && x.Status == WorkLogStatus.Active)
                                .ToListAsync();

                taskNeedPause = tasksInProject.OrderBy(x=>x.CreatedDate).LastOrDefault(x => x.TaskID == workLogRequest.TaskID) == null
                    ? tasksInProject.OrderBy(x=>x.CreatedDate).LastOrDefault(x => x.TaskID.Contains(workLogRequest.TaskID))
                    : tasksInProject.OrderBy(x=>x.CreatedDate).LastOrDefault(x => x.TaskID == workLogRequest.TaskID);
            }

            if (taskNeedPause == null && !string.IsNullOrWhiteSpace(workLogRequest.Name))
            {
                // Still not found => find by name
                taskNeedPause = await _unitOfWork.GetRepository<WorkLog>()
                                .GetAll()
                                .OrderBy(x=>x.CreatedDate).LastOrDefaultAsync(x => x.WorkingProjectId == workLogRequest.WorkingProjectId
                                    && x.Status == WorkLogStatus.Active
                                    && (x.Name.ToLower().Equals(workLogRequest.Name.ToLower())
                                    || x.Name.ToLower().Contains(workLogRequest.Name.ToLower())));
            }

            if (taskNeedPause == null)
            {
                // Pause current active workLog
                taskNeedPause = await _unitOfWork.GetRepository<WorkLog>()
                    .GetAll()
                    .OrderBy(x=>x.CreatedDate).LastOrDefaultAsync(x => x.Status == WorkLogStatus.Active && x.WorkingProjectId == workLogRequest.WorkingProjectId);

                if (taskNeedPause == null)
                {
                    _logger.LogError("Incorrect commands. Not found task to pause");
                    return;
                }
            }

            // Add history logs
            var logHistories = GetHistoriesLogs(taskNeedPause);

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
                        var workLogHistories = await _unitOfWork.GetRepository<WorkLogHistory>().FindAsync(x => x.WorkLogId == taskNeedPause.Id);

                        taskNeedPause.Status = WorkLogStatus.Paused;
                        taskNeedPause.WorkingHour = WorkLogHelper.CalculateWorkingHour(workLogHistories.ToList());

                        await _unitOfWork.GetRepository<WorkLog>().UpdateAsync(taskNeedPause);

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

        private IEnumerable<WorkLogHistory> GetHistoriesLogs(WorkLog taskNeedPause)
        {
            var results = new List<WorkLogHistory>();

            if (taskNeedPause == null) return results;

            // Pause current active
            results.Add(new WorkLogHistory
            {
                Action = WorkLogHistoryAction.Pause,
                CreatedDate = DateTime.UtcNow,
                WorkLogId = taskNeedPause.Id,
                WorkLogEntity = taskNeedPause
            });

            return results;
        }
    }
}
