using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands
{
    public class HandleCancelCommand : IHandleCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();

        public HandleCancelCommand(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(WorkLogRequest workLogRequest)
        {
            // Get current active task
            WorkLog? taskNeedCancel = null;
            if (!string.IsNullOrWhiteSpace(workLogRequest.TaskID))
            {
                // Find by taskId
                var tasksInProject = await _unitOfWork.GetRepository<WorkLog>()
                                .GetAll()
                                .Where(x => x.WorkingProjectId == workLogRequest.WorkingProjectId)
                                .ToListAsync();

                taskNeedCancel = tasksInProject.OrderBy(x=>x.CreatedDate).LastOrDefault(x => x.TaskID == workLogRequest.TaskID) == null
                    ? tasksInProject.OrderBy(x=>x.CreatedDate).LastOrDefault(x => x.TaskID.Contains(workLogRequest.TaskID))
                    : tasksInProject.OrderBy(x=>x.CreatedDate).LastOrDefault(x => x.TaskID == workLogRequest.TaskID);
            }

            if (taskNeedCancel == null && !string.IsNullOrWhiteSpace(workLogRequest.Name))
            {
                // Still not found => find by name
                taskNeedCancel = await _unitOfWork.GetRepository<WorkLog>()
                                .GetAll()
                                .OrderBy(x=>x.CreatedDate).LastOrDefaultAsync(x => x.WorkingProjectId == workLogRequest.WorkingProjectId
                                    && (x.Name.ToLower().Equals(workLogRequest.Name.ToLower())
                                    || x.Name.ToLower().Contains(workLogRequest.Name.ToLower())));
            }

            if (taskNeedCancel == null)
            {
                _logger.LogError("Incorrect commands. Not found task to cancel");
                return;
            }

            // Add history logs
            var logHistories = GetHistoriesLogs(taskNeedCancel);

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
                        var workLogHistories = await _unitOfWork.GetRepository<WorkLogHistory>().FindAsync(x => x.WorkLogId == taskNeedCancel.Id);
                        var allHistories = workLogHistories.Concat(logHistories).ToList();

                        taskNeedCancel.Status = WorkLogStatus.Cancelled;
                        taskNeedCancel.EndDate = DateTime.UtcNow;
                        taskNeedCancel.WorkingHour = WorkLogHelper.CalculateWorkingHour(allHistories);

                        await _unitOfWork.GetRepository<WorkLog>().UpdateAsync(taskNeedCancel);

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
                _logger.LogError(ex, "Error cancel work log and add history entry for request: {RequestCommand}", workLogRequest.FullCommand);                
                return;
            }
        }

        private IEnumerable<WorkLogHistory> GetHistoriesLogs(WorkLog taskNeedCancel)
        {
            var results = new List<WorkLogHistory>();

            if (taskNeedCancel == null) return results;

            // Finish current active
            results.Add(new WorkLogHistory
            {
                Action = WorkLogHistoryAction.Cancel,
                CreatedDate = DateTime.UtcNow,
                WorkLogId = taskNeedCancel.Id,
                WorkLogEntity = taskNeedCancel
            });

            return results;
        }
    }
}
