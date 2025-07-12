using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands
{
    public class HandleStartCommand : IHandleCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();

        public HandleStartCommand(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(WorkLogRequest workLogRequest)
        {            
            var workLog = WorkLogHelper.MapWorkLog(workLogRequest);
            var logHistories = await GetHistoriesLogs(workLog);

            // Start add to DB in a transaction
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        await _unitOfWork.GetRepository<WorkLog>().AddAsync(workLog);
                        await _unitOfWork.GetRepository<WorkLogHistory>().AddRangeAsync(logHistories);
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
                await _unitOfWork.RollbackAsync();

                _logger.LogError(ex, "Error adding work log and history entry for request: {RequestCommand}", workLogRequest.FullCommand);
                return;
            }
        }

        private async Task<IEnumerable<WorkLogHistory>> GetHistoriesLogs(WorkLog newWorkLog)
        {
            var results = new List<WorkLogHistory>();

            // Make sure only 1 task active at a time.
            // Get current active work
            var currentActive = await _unitOfWork.GetRepository<WorkLog>()
                .GetAll()
                .FirstOrDefaultAsync(w =>
                    w.Status == WorkLogStatus.Active && w.WorkingProjectId == newWorkLog.WorkingProjectId);

            if (currentActive != null)
            {
                // Pause current active
                results.Add(new WorkLogHistory
                {
                    Action = WorkLogHistoryAction.Pause,
                    CreatedDate = DateTime.UtcNow,
                    WorkLogId = currentActive.Id,
                    WorkLogEntity = currentActive
                });
            }

            // Start new work
            results.Add(new WorkLogHistory
            {
                Action = WorkLogHistoryAction.Start,
                CreatedDate = DateTime.UtcNow,
                WorkLogId = newWorkLog.Id,
                WorkLogEntity = newWorkLog
            });

            return results;
        }
    }
}
