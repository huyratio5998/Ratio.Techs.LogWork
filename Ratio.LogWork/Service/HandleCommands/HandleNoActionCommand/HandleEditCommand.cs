using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Helpers;
using Ratio.LogWork.Models;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service.HandleCommands.HandleNoActionCommand
{
    public class HandleEditCommand : HandleNoActionCommandBase
    {
        public HandleEditCommand(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Edit comment allow update latest transaction only or create new task in specific time.
        /// Update latest: edit 123
        /// In 123 detail: update start 
        /// </summary>
        /// <param name="workLogRequest"></param>
        /// <returns></returns>
        public override async Task Handle(WorkLogRequest workLogRequest)
        {
            if (workLogRequest == null) return;

            if (workLogRequest.CommandType != RatioCommandType.Task
                && workLogRequest.Action != WorkLogType.Other
                && !workLogRequest.Command.Equals(WorkLogHelper.EDIT, StringComparison.OrdinalIgnoreCase)) return;

            _logger.LogInformation("Handle no action command {requestCommand}", workLogRequest.FullCommand);
            return;
        }
    }
}
