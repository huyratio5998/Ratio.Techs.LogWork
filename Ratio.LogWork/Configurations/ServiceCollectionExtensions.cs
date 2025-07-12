using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ratio.LogWork.Context;
using Ratio.LogWork.Repositories;
using Ratio.LogWork.Repository;
using Ratio.LogWork.Service;

namespace Ratio.LogWork.Configurations
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkLogServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<WorkLogDBContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Ratio-LogWork"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()));

            // Register repositories
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IWorkLogRepository, WorkLogRepository>();
            services.AddScoped<IWorkingProjectRepository, WorkingProjectRepository>();
            services.AddScoped<IWorkLogHistoryRepository, WorkLogHistoryRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IWorkLogService, WorkLogService>();

            return services;
        }
    }
}
