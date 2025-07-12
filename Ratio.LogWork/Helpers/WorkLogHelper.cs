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
                "pause" => WorkLogHistoryAction.Pause,
                "continue" => WorkLogHistoryAction.Continue,
                "done" => WorkLogHistoryAction.Done,
                "cancel" => WorkLogHistoryAction.Cancel,
                "event" => WorkLogHistoryAction.Start,

                "wc" => WorkLogHistoryAction.Continue,
                "lunch" => WorkLogHistoryAction.Continue,
                "drink" => WorkLogHistoryAction.Continue,
                "happy hour" => WorkLogHistoryAction.Continue,
                "home" => WorkLogHistoryAction.Continue,
                "off" => WorkLogHistoryAction.Continue,

                "meeting" => WorkLogHistoryAction.Continue,
                "test" => WorkLogHistoryAction.Continue,
                "pr" => WorkLogHistoryAction.Continue,
                "support" => WorkLogHistoryAction.Continue,

                _ => WorkLogHistoryAction.NoAction
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

        public static RatioCommand? GetWorkingRequest(string requestCommand, int activeProjectId)
        {
            var projectCommands = new List<string> { "active project", "start project", "switch project" };

            if (string.IsNullOrWhiteSpace(requestCommand))
            {
                _logger.LogError("Request command is null or empty.");
                return null;
            }

            var command = requestCommand.Split(" ")[0];

            if (projectCommands.Any(x => requestCommand.StartsWith(x)))
            {
                // Handle project commands                
                return new RatioCommand
                {
                    CommandType = RatioCommandType.Project,
                    Command = command,
                    FullCommand = requestCommand
                };
            }

            var firstSpaceIndex = requestCommand.IndexOf(' ');
            var data = (firstSpaceIndex >= 0 && firstSpaceIndex < requestCommand.Length - 1)
                ? requestCommand.Substring(firstSpaceIndex + 1)
                : string.Empty;

            return BuildTaskRequest(command, data, activeProjectId);
        }

        public static WorkLogRequest? BuildTaskRequest(string command, string data, int activeProjectId)
        {
            if (!_listCommandAcceptable.Contains(command, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogError("Invalid command: {Command}", command);
                return null;
            }

            var workAction = WorkLogHelper.GetWorkActionByCommand(command);
            var taskId = data.Split("-")[0] ?? string.Empty;
            WorkLogRequest request = new WorkLogRequest
            {
                CommandType = RatioCommandType.Task,
                Command = command,
                FullCommand = $"{command} {data}",
                Action = workAction,
                TaskID = taskId,
                Name = GetWorkDisplayName(workAction, taskId, data.Split("-").Length > 1 ? data.Split("-")[1] : string.Empty),
                WorkingProjectId = activeProjectId
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
