using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Models;
using Ratio.LogWork.Service;

namespace Ratio.LogWork.Helpers
{
    public static class WorkLogHelper
    {
        public static readonly List<string> _listCommandAcceptable =
            new List<string> {
                "start", "pause", "continue", "done", "cancel", "event",
                "wc","lunch", "drink", "happy hour", "home", "off",
                "meeting", "test", "pr", "support"
            };

        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();

        public static WorkLogHistoryAction GetHistoryActionByCommand(string command)
        {
            // Normal Command: start, pause, continue, done, cancel: those ticket no need to do, event: company event
            // Command no deadline: wc, lunch, drink, happy hour, home:go home
            // ====> should seed worklog those common command.(start and pause)

            // report command: report => won't log into history

            // off: for on leave - will have different logic

            // Command no deadline-work: meeting, test, pr, support

            // History action: start, pause, continue, done
            command = command.ToLowerInvariant();
            return command switch
            {
                "start" => WorkLogHistoryAction.Start,
                "test" => WorkLogHistoryAction.Start,
                "pr" => WorkLogHistoryAction.Start,
                "support" => WorkLogHistoryAction.Start,
                "pause" => WorkLogHistoryAction.Pause,
                "continue" => WorkLogHistoryAction.Continue,
                "done" => WorkLogHistoryAction.Done,
                "cancel" => WorkLogHistoryAction.Cancel,

                "event" => WorkLogHistoryAction.Start,
                "wc" => WorkLogHistoryAction.Start,
                "lunch" => WorkLogHistoryAction.Start,
                "drink" => WorkLogHistoryAction.Start,
                "happy hour" => WorkLogHistoryAction.Start,
                "home" => WorkLogHistoryAction.Start,
                "off" => WorkLogHistoryAction.Start,
                "meeting" => WorkLogHistoryAction.Start,

                _ => WorkLogHistoryAction.NoAction
            };
        }

        public static WorkLog MapWorkLog(WorkLogRequest workLogRequest)
        {
            return new WorkLog
            {
                TaskID = workLogRequest.TaskID,
                Name = workLogRequest.Name,
                Command = workLogRequest.Command,
                FullCommand = workLogRequest.FullCommand,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                WorkingHour = 0,
                WorkType = workLogRequest.Action,
                Status = WorkLogStatus.Active,
                WorkingProjectId = (int)workLogRequest.WorkingProjectId
            };
        }

        public static WorkLogType GetWorkActionByCommand(string command)
        {
            return command.ToLowerInvariant() switch
            {
                "start" => WorkLogType.Task,
                "pause" => WorkLogType.Task,
                "continue" => WorkLogType.Task,
                "done" => WorkLogType.Task,
                "cancel" => WorkLogType.Task,
                "test" => WorkLogType.Task,
                "pr" => WorkLogType.Task,
                "event" => WorkLogType.Event,
                "lunch" => WorkLogType.Lunch,
                "home" => WorkLogType.GoHome,
                "off" => WorkLogType.OnLeave,
                "meeting" => WorkLogType.Meeting,
                "support" => WorkLogType.Support,
                "wc" => WorkLogType.Other,
                "drink" => WorkLogType.Other,
                "happy hour" => WorkLogType.Other,
                _ => WorkLogType.Other
            };
        }        

        public static WorkLogRequest? BuildTaskRequest(QueryRequest queryRequest, int activeProjectId)
        {
            if (!_listCommandAcceptable.Contains(queryRequest.Command, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogError("Invalid command: {Command}", queryRequest.Command);
                return null;
            }

            var workAction = GetWorkActionByCommand(queryRequest.Command);            
            WorkLogRequest request = new WorkLogRequest
            {
                WorkingProjectId = activeProjectId,
                CommandType = RatioCommandType.Task,
                Command = queryRequest.Command,
                Action = workAction,
                FullCommand = queryRequest.RawQuery,
                TaskID = queryRequest.TicketId,
                Name = GetWorkDisplayName(workAction, queryRequest.TicketId, queryRequest.Description),
            };

            return request;
        }

        private static string GetWorkDisplayName(WorkLogType action, string taskId, string name)
        {
            var ticketId = string.IsNullOrEmpty(taskId) ? "{ticketId}" : taskId;
            return $"{action.ToString()} {ticketId}-{StringHelper.ToCapitalize(name)}";
        }
    }
}
