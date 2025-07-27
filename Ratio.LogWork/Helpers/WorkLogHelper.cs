using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Models;
using Ratio.LogWork.Service;

namespace Ratio.LogWork.Helpers
{
    public static class WorkLogHelper
    {
        // Command constant
        public const string START = "start";
        public const string PAUSE = "pause";
        public const string CONTINUE = "continue";
        public const string DONE = "done";
        public const string CANCEL = "cancel";
        public const string EVENT = "event";
        public const string WC = "wc";
        public const string LUNCH = "lunch";
        public const string DRINK = "drink";
        public const string HAPPY_HOUR = "happy hour";
        public const string HOME = "home";
        public const string OFF = "off";
        public const string MEETING = "meeting";
        public const string TEST = "test";
        public const string PR = "pr";
        public const string SUPPORT = "support";
        public const string REPORT = "report";
        public const string SHOW = "show";
        public const string EDIT = "edit";

        //
        public static readonly List<string> _listCommandAcceptable =
        new List<string> {
            START, PAUSE, CONTINUE, DONE, CANCEL, EVENT,
            WC, LUNCH, DRINK, HAPPY_HOUR, HOME, OFF,
            MEETING, TEST, PR, SUPPORT,
            REPORT, SHOW,
            EDIT
        };

        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();
        
        private static readonly Dictionary<string, WorkLogHistoryAction> _commandToHistoryAction = new(StringComparer.OrdinalIgnoreCase)
        {
            { START, WorkLogHistoryAction.Start },
            { PAUSE, WorkLogHistoryAction.Pause },
            { CONTINUE, WorkLogHistoryAction.Continue },
            { DONE, WorkLogHistoryAction.Done },
            { CANCEL, WorkLogHistoryAction.Cancel },
            { TEST, WorkLogHistoryAction.Start },
            { PR, WorkLogHistoryAction.Start },
            { SUPPORT, WorkLogHistoryAction.Start },
            { MEETING, WorkLogHistoryAction.Start },
            { EVENT, WorkLogHistoryAction.Start },
            { WC, WorkLogHistoryAction.Start },
            { LUNCH, WorkLogHistoryAction.Start },
            { DRINK, WorkLogHistoryAction.Start },
            { HAPPY_HOUR, WorkLogHistoryAction.Start },
            { HOME, WorkLogHistoryAction.Start },
            { OFF, WorkLogHistoryAction.Start }
            // all other cases should return NoAction
        };
        public static WorkLogHistoryAction GetHistoryActionByCommand(string command)
        {
            // Normal Command: start, pause, continue, done, cancel: those ticket no need to do, event: company event
            // Command no deadline: wc, lunch, drink, happy hour, home:go home
            // ====> should seed worklog those common command.(start and pause)

            // report command: report => won't log into history

            // off: for on leave - will have different logic

            // Command no deadline-work: meeting, test, pr, support

            // History action: start, pause, continue, done
            if (string.IsNullOrWhiteSpace(command))
                return WorkLogHistoryAction.NoAction;

            if (_commandToHistoryAction.TryGetValue(command.Trim(), out var action))
                return action;

            return WorkLogHistoryAction.NoAction;
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

        public static double CalculateWorkingHour(List<WorkLogHistory> workLogHistories)
        {           
            if (!workLogHistories.Any()) return 0;

            workLogHistories = workLogHistories.OrderBy(x=>x.CreatedDate).ToList();

            double totalMinutes = 0;
            DateTime? startTime = null;
            foreach (var item in workLogHistories)
            {
                switch (item.Action)
                {
                    case WorkLogHistoryAction.Start:
                    case WorkLogHistoryAction.Continue:
                        // Begin timing
                        startTime = item.CreatedDate;
                        break;

                    case WorkLogHistoryAction.Pause:
                    case WorkLogHistoryAction.Done:
                    case WorkLogHistoryAction.Cancel:
                        // End timing and accumulate
                        if (startTime.HasValue)
                        {
                            totalMinutes += (item.CreatedDate - startTime.Value).TotalMinutes;
                            startTime = null;
                        }
                        break;

                    case WorkLogHistoryAction.NoAction:
                    default:
                        // No timing change for these actions
                        break;
                }
            }

            return totalMinutes / 60;            
        }

        private static readonly Dictionary<string, WorkLogType> _commandToWorkLogType = new(StringComparer.OrdinalIgnoreCase)
        {
            { START, WorkLogType.Task },
            { PAUSE, WorkLogType.Task },
            { CONTINUE, WorkLogType.Task },
            { DONE, WorkLogType.Task },
            { CANCEL, WorkLogType.Task },
            { TEST, WorkLogType.Task },
            { PR, WorkLogType.Task },
            { EVENT, WorkLogType.Event },
            { LUNCH, WorkLogType.Lunch },
            { HOME, WorkLogType.GoHome },
            { OFF, WorkLogType.OnLeave },
            { MEETING, WorkLogType.Meeting },
            { SUPPORT, WorkLogType.Support },
            { WC, WorkLogType.Other },
            { DRINK, WorkLogType.Other },
            { HAPPY_HOUR, WorkLogType.Other },
            { REPORT, WorkLogType.Report },
            { SHOW, WorkLogType.Report },
        };
        public static WorkLogType GetWorkActionByCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return WorkLogType.Other;

            if (_commandToWorkLogType.TryGetValue(command.Trim(), out var type))
                return type;

            return WorkLogType.Other;
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
            var parts = new List<string> { action.ToString(), ticketId.Trim() };

            var displayName = StringHelper.ToCapitalize(name ?? string.Empty).Trim();

            if (!string.IsNullOrEmpty(displayName))
                parts.Add(displayName);

            return string.Join("-", parts);
        }

        public static string GetTimeDisplay(this double hour)
        {
            if (hour < 0)
                return "Invalid";

            int totalSeconds = (int)Math.Round(hour * 3600);

            int years = totalSeconds / (365 * 24 * 3600);
            totalSeconds %= (365 * 24 * 3600);

            int months = totalSeconds / (30 * 24 * 3600);
            totalSeconds %= (30 * 24 * 3600);

            int days = totalSeconds / (24 * 3600);
            totalSeconds %= (24 * 3600);

            int hours = totalSeconds / 3600;
            totalSeconds %= 3600;

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            var parts = new List<string>();
            if (years > 0) parts.Add($"{years}y");
            if (months > 0) parts.Add($"{months}mo");
            if (days > 0) parts.Add($"{days}d");
            if (hours > 0) parts.Add($"{hours}h");
            if (minutes > 0) parts.Add($"{minutes}m");
            if (seconds > 0 || parts.Count == 0) parts.Add($"{seconds}s");

            return string.Join(" ", parts);
        }

        public static string GetWorkLogIdDisplay(WorkLog workLog)
        {
            const string WORK_LOG_PREFIX_CODE = "R";
            if (workLog == null) return string.Empty;

            return $"{WORK_LOG_PREFIX_CODE}{workLog.Id}";
        }
    }
}
