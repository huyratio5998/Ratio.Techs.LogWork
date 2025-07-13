using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;
using Ratio.LogWork.Service.HandleCommands;

namespace Ratio.LogWork.Service
{
    public class WorkLogService : IWorkLogService
    {                               
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkingProjectService _workingProjectService;

        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();        

        public WorkLogService(IUnitOfWork unitOfWork, IWorkingProjectService workingProjectService)
        {
            _unitOfWork = unitOfWork;
            _workingProjectService = workingProjectService;
        }

        public QueryRequest GetQueryRequest(string command)
        {
            var result = new QueryRequest() { RawQuery = command };
            var words = command.Split(' ');
            var wordNumbers = words.Length;

            if (string.IsNullOrWhiteSpace(command)) return result;

            if (wordNumbers == 1)
            {
                result.Command = words[0];
            }
            else if (wordNumbers == 2)
            {
                result.Command = words[0];
                result.TicketId = words[1];
            }
            else if (wordNumbers > 3)
            {
                result.Command = words[0];
                result.TicketId = words[1];
                result.Description = string.Join(' ', words.Skip(2));
            }

            if (result.Command.Equals("project", StringComparison.OrdinalIgnoreCase))
            {
                result.TicketId = string.Empty;
                result.Description = string.Empty;
                result.ProjectName = string.Join(' ', words.Skip(1));
            }

            return result;
        }

        /// <summary>
        /// Handle project or task
        /// </summary>
        /// <param name="request"></param>
        /// <param name="workingProject"></param>
        /// <returns></returns>
        public async Task ExecuteCommand(string request, WorkingProject workingProject)
        {
            QueryRequest queryRequest = GetQueryRequest(request);

            // Handle project
            if(queryRequest.Command.Equals("project", StringComparison.OrdinalIgnoreCase))
            {
                await _workingProjectService.ActiveProject(workingProject, queryRequest.ProjectName);
                return;
            }

            // Handle task
            RatioCommand? requestCommand = WorkLogHelper.BuildTaskRequest(queryRequest, workingProject.Id);

            if (requestCommand == null)
            {
                _logger.LogError("Invalid request format: {RequestCommand}", request);
                return;
            }            

            if (requestCommand.CommandType == RatioCommandType.Task)
            {
                var taskRequest = requestCommand as WorkLogRequest;

                if (taskRequest == null || taskRequest.WorkingProjectId == null)
                {
                    _logger.LogError("Invalid task request format: {RequestCommand}", request);
                    return;
                }

                await HandleTaskCommandAsync(taskRequest);                
            }
        }

        private async Task HandleTaskCommandAsync(WorkLogRequest request)
        {
            WorkLogHistoryAction historyAction = WorkLogHelper.GetHistoryActionByCommand(request.Command);
            IHandleCommands handleCommands = new HandleCommandFactory(_unitOfWork).Create(historyAction);

            await handleCommands.Handle(request);                
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
