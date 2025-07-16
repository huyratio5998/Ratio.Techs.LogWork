using Microsoft.Extensions.Logging;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands.HandleNoActionCommand
{
    public abstract class HandleNoActionCommandBase : IHandleCommands
    {
        protected static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();
        protected readonly IUnitOfWork _unitOfWork;

        protected HandleNoActionCommandBase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public abstract Task Handle(WorkLogRequest workLogRequest);        

        public void KeepDisplayResult(string continueProgramCommand = "ok")
        {
            Console.WriteLine("Write \"ok\" to continue commands: ");
            while (true)
            {
                var command = Console.ReadLine();
                
                if (command == null) continue;
                if (command.Equals(continueProgramCommand, StringComparison.OrdinalIgnoreCase)) break;

                Console.WriteLine("Write \"ok\" to continue commands: ");
            }
        }
    }
}
