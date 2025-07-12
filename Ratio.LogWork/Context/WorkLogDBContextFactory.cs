using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Ratio.LogWork.Context
{
    public class WorkLogDBContextFactory : IDesignTimeDbContextFactory<WorkLogDBContext>
    {
        public WorkLogDBContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<WorkLogDBContext>();
            var connectionString = configuration.GetConnectionString("Ratio-LogWork");
            optionsBuilder.UseSqlServer(connectionString);

            return new WorkLogDBContext(optionsBuilder.Options);
        }
    }
}
