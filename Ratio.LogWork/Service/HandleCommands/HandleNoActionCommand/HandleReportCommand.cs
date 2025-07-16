using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands.HandleNoActionCommand
{
    public class HandleReportCommand : HandleNoActionCommandBase
    {
        public HandleReportCommand(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public override async Task Handle(WorkLogRequest workLogRequest)
        {
            if (workLogRequest == null) return;

            if (workLogRequest.CommandType != RatioCommandType.Task
                && workLogRequest.Action != WorkLogType.Other
                && !workLogRequest.Command.Equals(WorkLogHelper.REPORT, StringComparison.OrdinalIgnoreCase)) return;

            _logger.LogInformation("Handle no action command {requestCommand}", workLogRequest.FullCommand);
            return;
        }
    }
}
