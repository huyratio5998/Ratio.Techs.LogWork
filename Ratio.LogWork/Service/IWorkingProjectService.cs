using Ratio.LogWork.Entity;

namespace Ratio.LogWork.Service
{
    public interface IWorkingProjectService
    {
        Task ActiveProject(WorkingProject currentActiveProject, string newProjectName);
    }
}
