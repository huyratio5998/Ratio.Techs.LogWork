using Ratio.LogWork.Entity;
using Ratio.LogWork.Models;

namespace Ratio.LogWork.Service
{
    public interface IWorkLogService
    {
        QueryRequest GetQueryRequest(string command);
        Task ExecuteCommand(string request, WorkingProject workingProject);        
    }
}
