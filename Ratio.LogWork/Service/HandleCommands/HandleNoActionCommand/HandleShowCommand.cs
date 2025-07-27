using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands.HandleNoActionCommand
{
    public class HandleShowCommand : HandleNoActionCommandBase
    {
        public HandleShowCommand(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Show tasks.
        /// Eg: show 123,234,555
        /// </summary>
        /// <param name="workLogRequest"></param>
        /// <returns></returns>
        public override async Task Handle(WorkLogRequest workLogRequest)
        {
            if (workLogRequest == null || string.IsNullOrWhiteSpace(workLogRequest.TaskID))
            {
                DisplayError(workLogRequest);
                return;
            }

            if (workLogRequest.CommandType != RatioCommandType.Task
                && workLogRequest.Action != WorkLogType.Other
                && !workLogRequest.Command.Equals(WorkLogHelper.SHOW, StringComparison.OrdinalIgnoreCase)) return;

            var commandParts = workLogRequest.FullCommand.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var ticketPart = commandParts.Length > 1 ? commandParts[1] : string.Empty;

            var tickets = ticketPart.Trim().Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            List<WorkLog> displayItems = new List<WorkLog>();
            var workLogRepository = _unitOfWork.GetRepository<WorkLog>();

            if (tickets.Length == 1)
            {
                // Show 123
                var ticketId = tickets[0];
                if (string.IsNullOrWhiteSpace(ticketId))
                {
                    DisplayError(workLogRequest, "Task ID cannot empty.");
                    return;
                }

                var workLogs = await workLogRepository.FindAsync(x => x.TaskID == ticketId);
                if (workLogs == null || !workLogs.Any())
                {
                    DisplayError(workLogRequest);
                    return;
                }

                displayItems.AddRange(workLogs);
                Display(displayItems, workLogRequest);
            }
            else if (tickets.Length > 1)
            {
                // show 123,453,345,656                
                var workLogs = await workLogRepository
                    .GetAll()
                    .Where(x => !string.IsNullOrWhiteSpace(x.TaskID) && tickets.Contains(x.TaskID))
                    .ToListAsync();

                if (workLogs == null || !workLogs.Any())
                {
                    DisplayError(workLogRequest);
                    return;
                }

                displayItems.AddRange(workLogs);
                Display(displayItems, workLogRequest);
            }

            return;
        }

        private void DisplayError(WorkLogRequest? workLogRequest, string errorMessage = "")
        {
            string err = string.IsNullOrWhiteSpace(errorMessage) ? "Task not found" : errorMessage;

            if (workLogRequest == null)
            {
                Console.WriteLine(err);
                KeepDisplayResult();
                return;
            }

            _logger.LogInformation("Task not found. Command {requestCommand}", workLogRequest.FullCommand);
            Console.WriteLine($"{err}. Command {workLogRequest.TaskID}");

            KeepDisplayResult();
            return;
        }

        private void Display(List<WorkLog> workLogs, WorkLogRequest workLogRequest)
        {
            if (!workLogs.Any())
            {
                _logger.LogInformation("Task not found. Command {requestCommand}", workLogRequest.FullCommand);
                Console.WriteLine($"Task not found. Command {workLogRequest.TaskID}");

                KeepDisplayResult();
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var item in workLogs.OrderBy(x => x.TaskID).ThenBy(x => x.CreatedDate))
            {
                sb.AppendLine($"[{item.Status}][{WorkLogHelper.GetWorkLogIdDisplay(item)}]{item.Name.SanitizeName()}");
                sb.AppendLine($"⏳ Time: {item.WorkingHour.GetTimeDisplay()}");
                sb.AppendLine($"🏃 Start: {item.CreatedDate:yyyy-MM-dd HH:mm}");
                string currentStatus = item.Status == WorkLogStatus.Done ? "✅ Finish" : $"⚒️ {item.Status.ToString()}";
                sb.AppendLine($"{currentStatus}: {item.EndDate:yyyy-MM-dd HH:mm}");
                sb.AppendLine("---------------------------------------------------------");
            }

            Console.Write(sb.ToString());

            KeepDisplayResult();
        }
    }
}
