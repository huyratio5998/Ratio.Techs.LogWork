using Ratio.LogWork.Entity;

namespace Ratio.LogWork.Service
{
    public interface IWorkLogService
    {                
        Task ExecuteCommand(string request, WorkingProject workingProject);        
    }
}
