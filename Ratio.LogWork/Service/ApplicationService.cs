using Microsoft.EntityFrameworkCore;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Repository;
using Ratio.LogWork.Service.SimpleAutoComplete;

namespace Ratio.LogWork.Service
{
    public class ApplicationService : IApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkLogService _workLogService;
        private readonly IReportService _reportService;

        public ApplicationService(IUnitOfWork unitOfWork, IWorkLogService workLogService, IReportService reportService)
        {
            _unitOfWork = unitOfWork;
            _workLogService = workLogService;
            _reportService = reportService;
        }

        public async Task Run()
        {
            var activeProject = await _unitOfWork.WorkingProjects.GetActiveProjectAsync();

            if (activeProject == null) await HandleActiveProject(activeProject);

            while (true)
            {
                await InitDisplay(activeProject);

                ReadLine.HistoryEnabled = true;
                ReadLine.AutoCompletionHandler = new LogWorkAutoCompleteHandler(new[]
                {
                    "test", "pr", "support", "meeting", "start", "pause", "continue", "cancel", "done",
                    "wc", "lunch", "drink", "happy hour", "event", "off", "home"
                });                
                var command = ReadLine.Read();

                // Validate command
                if (string.IsNullOrWhiteSpace(command))
                {
                    Console.WriteLine("Please enter a valid command.");
                    continue;
                }

                if (command.Equals("0") || command.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                if (command.Equals("clear", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Clear();
                    continue;
                }

                // handle command
                await _workLogService.ExecuteCommand(command, activeProject);
            }
        }

        private async Task HandleActiveProject(WorkingProject activeProject)
        {
            while (true)
            {
                Console.WriteLine("No active project found. Please set an active project before running the application.");

                var requestCommand = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(requestCommand))
                {
                    Console.WriteLine("Please enter a valid command.");
                    continue;
                }

                // active project
                var query = _workLogService.GetQueryRequest(requestCommand);
                if (!query.Command.Equals("project", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(query.ProjectName))
                {
                    Console.WriteLine("Please enter a valid command.");
                    continue;
                }

                var existedProject = await _unitOfWork.WorkingProjects
                    .GetAll()
                    .FirstOrDefaultAsync(x => x.Name.Equals(query.ProjectName, StringComparison.OrdinalIgnoreCase));

                if (existedProject == null)
                {
                    await _unitOfWork.WorkingProjects.AddAsync(new WorkingProject
                    {
                        Name = query.ProjectName,
                        ProjectStatus = WorkingProjectStatus.Active,
                    });
                    await _unitOfWork.CommitAsync();
                }
                else
                {
                    existedProject.ProjectStatus = WorkingProjectStatus.Active;

                    await _unitOfWork.WorkingProjects.UpdateAsync(existedProject);
                    await _unitOfWork.CommitAsync();
                }

                activeProject = await _unitOfWork.WorkingProjects.GetActiveProjectAsync();

                if (activeProject == null)
                {
                    Console.WriteLine("Error when Active project. Please try again.");
                }
                else break;
            }
        }

        private async Task InitDisplay(WorkingProject? activeProject)
        {
            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine($"RATIO-TECHS:WORK-LOGS {DateTime.Now.ToString("D")}");
            Console.WriteLine();

            if (activeProject == null)
            {
                Console.WriteLine("No project is active");
                return;
            }

            List<WorkLog>? displayTopRecentTasks = new List<WorkLog>();
            int maximumTaskDisplay = 5;
            displayTopRecentTasks = await _unitOfWork.GetRepository<WorkLog>()
                .GetAll()
                .Where(x => x.Status == WorkLogStatus.Active)
                .OrderByDescending(x => x.CreatedDate)
                .Take(maximumTaskDisplay)
                .ToListAsync();

            if (!displayTopRecentTasks.Any() || displayTopRecentTasks.Count < maximumTaskDisplay)
            {
                var remainSlot = maximumTaskDisplay - displayTopRecentTasks.Count;
                displayTopRecentTasks.AddRange(
                    await _unitOfWork.GetRepository<WorkLog>()
                    .GetAll()
                    .Where(x => x.Status == WorkLogStatus.Paused)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(remainSlot)
                    .ToListAsync()
                );
            }

            displayTopRecentTasks = displayTopRecentTasks.OrderBy(x => x.CreatedDate).ToList();

            Console.WriteLine($"---------------------- ACTIVE-PROJECT:[{activeProject.Name.ToUpperInvariant()}]----------------------");
            Console.WriteLine();

            if (displayTopRecentTasks.Any())
            {
                Console.WriteLine("Recent tasks:");
                foreach (var item in displayTopRecentTasks)
                {
                    Console.WriteLine($"[{item.Status.ToString().ToUpperInvariant()}]{item.Name}]");
                }
            }
            else
            {
                Console.WriteLine("No task need to do");
            }

            Console.WriteLine();
            Console.WriteLine("🖥️ Task-Action: Test, PR, Support, Meeting, Start, Pause, Continue, Cancel, Done");
            Console.WriteLine("💼 Others-Action: WC, Lunch, Drink, Happy hour, Event, Off, Home:go home");
            Console.WriteLine("⌨️ Your commands \"{Action} {Task} {Description}\":");
        }
    }
}
