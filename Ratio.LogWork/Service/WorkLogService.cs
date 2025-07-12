using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;
using System.Collections.Concurrent;

namespace Ratio.LogWork.Service
{
    public class WorkLogService : IWorkLogService
    {                               
        private readonly IUnitOfWork _unitOfWork;

        private static readonly ILogger<WorkLogService> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();
        
        public WorkLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }   
        
        public async Task ExecuteCommand(string request, WorkingProject workingProject)
        {                        
            RatioCommand? requestCommand = WorkLogHelper.GetWorkingRequest(request, workingProject.Id);

            if (request == null || requestCommand == null)
            {
                logger.LogError("Invalid request format: {RequestCommand}", request);
                return;
            }            

            if (requestCommand.CommandType == RatioCommandType.Task)
            {
                var taskRequest = requestCommand as WorkLogRequest;

                if (taskRequest == null || taskRequest.WorkingProjectId == null)
                {
                    logger.LogError("Invalid task request format: {RequestCommand}", request);
                    return;
                }

                var workLog = await HandleTaskCommandAsync(taskRequest);

                if (workLog)
                {
                    Console.WriteLine($"Command '{request}' processed successfully.");
                }
            }
        }

        private async Task<bool> HandleTaskCommandAsync(WorkLogRequest request)
        {
            if (request == null)
            {
                logger.LogError("Request is null.");
                return false;
            }

            // Create work log and history entry
            var workLog = new WorkLog
            {
                TaskID = request.TaskID,
                Name = request.Name,
                Command = request.Command,
                FullCommand = request.FullCommand,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                WorkingHour = 0,
                WorkType = request.Action,
                Status = WorkLogStatus.Active,
                WorkingProjectId = (int)request.WorkingProjectId
            };

            var logHistories = await BuildHistoriesRecords(request.Command, workLog);

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
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();

                logger.LogError(ex, "Error adding work log and history entry for request: {RequestCommand}", request.FullCommand);
                return false;
            }
        }        

        private async Task<IEnumerable<WorkLogHistory>> BuildHistoriesRecords(string command, WorkLog newWorkLog)
        {
            var results = new List<WorkLogHistory>();
            var historyAction = WorkLogHelper.GetHistoryActionByCommand(command);

            // get current active
            var currentActive = (await _unitOfWork.GetRepository<WorkLog>()
                .FindAsync(w => w.Status == WorkLogStatus.Active &&
                                w.WorkingProjectId == newWorkLog.WorkingProjectId))
                .FirstOrDefault();

            if (currentActive != null)
            {
                // move current active to pause
                results.Add(new WorkLogHistory
                {
                    Action = WorkLogHistoryAction.Pause,
                    CreatedDate = DateTime.UtcNow,
                    WorkLogEntity = currentActive
                });
            }

            if (historyAction == WorkLogHistoryAction.Start)
            {
                results.Add(new WorkLogHistory
                {
                    Action = historyAction,
                    CreatedDate = DateTime.UtcNow,
                    WorkLogEntity = newWorkLog
                });
            }

            return results;
        }        

        private async Task<WorkLog> CreateWorkLog(WorkLogRequest request)
        {
            // Look for an existing work log with the same TaskID
            var workLogRepository = _unitOfWork.GetRepository<WorkLog>();
            var existingWorkLog = (await workLogRepository.FindAsync(w =>
                w.TaskID == request.TaskID && w.WorkingProjectId == request.WorkingProjectId))
                .FirstOrDefault();

            if (existingWorkLog != null)
                return existingWorkLog;

            // Create new work log if not found
            var workLog = new WorkLog
            {
                TaskID = request.TaskID,
                Name = request.Name,
                Command = request.Command,
                FullCommand = request.FullCommand,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                WorkingHour = 0,
                WorkType = request.Action,
                Status = WorkLogStatus.Active,
                WorkingProjectId = (int)request.WorkingProjectId
            };

            await workLogRepository.AddAsync(workLog);
            return workLog;
        }
        
        // not implement
        public Task<WorkReportResponse> GetReport(DateTime reportDate)
        {
            throw new NotImplementedException();
        }

        public Task<WorkReportResponse> GetReports(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }
        
    }
}
