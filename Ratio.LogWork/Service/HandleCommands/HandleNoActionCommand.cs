using Microsoft.Extensions.Logging;
using Ratio.LogWork.Models;

namespace Ratio.LogWork.Service.HandleCommands
{
    public class HandleNoActionCommand : IHandleCommands
    {
        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();

        public async Task Handle(WorkLogRequest workLogRequest)
        {
            _logger.LogInformation("Handle no action command {requestCommand}", workLogRequest.FullCommand);
            return;
        }
    }
}
