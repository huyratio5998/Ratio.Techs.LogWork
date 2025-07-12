using Ratio.LogWork.Models;

namespace Ratio.LogWork.Service.HandleCommands
{
    public interface IHandleCommands
    {
        Task Handle(WorkLogRequest workLogRequest);
    }
}
