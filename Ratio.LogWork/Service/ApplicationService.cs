using Ratio.LogWork.Entity;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

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

            if (activeProject == null)
            {
                var projectCommands = new List<string> { "active project", "start project", "switch project" };
                while (true)
                {
                    Console.WriteLine("No active project found. Please set an active project before running the application.");

                    var requestCommand = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(requestCommand))
                    {
                        Console.WriteLine("Please enter a valid command.");
                        continue;
                    }

                    var command = requestCommand.Split(" ")[0];

                    if (projectCommands.Any(x => requestCommand.StartsWith(x)))
                    {
                        // Handle project commands -> would implement later                
                        Console.WriteLine("Active project");
                    }
                    else
                    {
                        Console.WriteLine("Invalid command. Please enter a valid project command.");
                    }
                }

            }

            while (true)
            {
                InitDisplay(activeProject);

                var command = Console.ReadLine();

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

        private static void InitDisplay(WorkingProject? activeProject)
        {
            Console.WriteLine("Welcome to Ratio Log Work Service!");

            if (activeProject == null)
            {
                Console.WriteLine("No project is active");
            }
            else
            {
                Console.WriteLine($"Working project: {activeProject.Name.ToUpperInvariant()}");
            }
            Console.WriteLine("Command pattern: {command} {ticketId}-{descriptions}");
            Console.Write("Command: ");
        }
    }
}
