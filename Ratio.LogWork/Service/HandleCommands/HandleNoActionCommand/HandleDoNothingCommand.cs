using Microsoft.Extensions.Logging;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands.HandleNoActionCommand
{
    public class HandleDoNothingCommand : HandleNoActionCommandBase
    {
        public HandleDoNothingCommand(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public override async Task Handle(WorkLogRequest workLogRequest)
        {
            if (workLogRequest == null) return;

            _logger.LogInformation("Handle no action command {requestCommand}", workLogRequest.FullCommand);
            return;
        }
    }
}
