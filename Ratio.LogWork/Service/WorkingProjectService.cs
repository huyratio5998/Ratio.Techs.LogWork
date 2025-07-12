using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Service
{
    public class WorkingProjectService : IWorkingProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly ILogger<WorkLogService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkLogService>();

        public WorkingProjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ActiveProject(WorkingProject currentActiveProject, string newProjectName)
        {
            if (currentActiveProject == null || string.IsNullOrWhiteSpace(newProjectName)) return;

            if (currentActiveProject.Name.Equals(newProjectName, StringComparison.OrdinalIgnoreCase)) return;

            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        var workingProjectRepository = _unitOfWork.GetRepository<WorkingProject>();

                        // Disable current active project
                        currentActiveProject.ProjectStatus = WorkingProjectStatus.None;
                        await workingProjectRepository.UpdateAsync(currentActiveProject);

                        var newProject = workingProjectRepository
                        .GetAll()
                        .FirstOrDefault(x => x.Name.Equals(newProjectName, StringComparison.OrdinalIgnoreCase));

                        // Active new project: Add or update
                        if (newProject == null)
                        {
                            await workingProjectRepository.AddAsync(
                                new WorkingProject
                                {
                                    Name = newProjectName,
                                    ProjectStatus = WorkingProjectStatus.Active,
                                });
                        }
                        else
                        {
                            newProject.ProjectStatus = WorkingProjectStatus.Active;
                            await workingProjectRepository.UpdateAsync(newProject);
                        }

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitAsync();
                    }
                    catch
                    {
                        await _unitOfWork.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();

                _logger.LogError(ex, "Error when active projet: {0}", newProjectName);
                return;
            }
        }
    }
}
