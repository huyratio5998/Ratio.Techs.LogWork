using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ratio.LogWork.Entity;

namespace Ratio.LogWork.Context
{
    public class WorkLogDBContext : DbContext
    {
        public DbSet<WorkingProject> WorkingProjects { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }
        public DbSet<WorkLogHistory> WorkLogHistories { get; set; }

        public WorkLogDBContext(DbContextOptions<WorkLogDBContext> options) : base(options)
        {
        }       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure WorkLog entity
            modelBuilder.Entity<WorkLog>()
                .HasOne(w => w.Project)
                .WithMany(p => p.WorkLogs)
                .HasForeignKey(w => w.WorkingProjectId);

            // Configure WorkLogHistory entity
            modelBuilder.Entity<WorkLogHistory>()
                .HasOne(h => h.WorkLogEntity)
                .WithMany(w => w.WorkLogHistories)
                .HasForeignKey(h => h.WorkLogId);

            // Seed WorkingProject data
            SeedWorkingProjects(modelBuilder);
        }

        private void SeedWorkingProjects(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkingProject>().HasData(
                new WorkingProject
                {
                    Id = 1,
                    Name = "PCL",
                    ProjectStatus = WorkingProjectStatus.Active,
                    CreatedDate = new DateTime(2025, 01, 01)
                }                
            );
        }

        /// <summary>
        /// Ensures the database is properly seeded with initial data, but only once
        /// </summary>
        public async Task EnsureSeedDataAsync()
        {
            // Apply migrations instead of creating the database directly
            await Database.MigrateAsync();

            // Check if the project with Id=1 already exists
            var projectExists = await WorkingProjects.AnyAsync(p => p.Id == 1);

            // Only add seed data if the specific record doesn't exist
            if (!projectExists)
            {
                WorkingProjects.Add(new WorkingProject
                {
                    Id = 1,
                    Name = "PCL",
                    ProjectStatus = WorkingProjectStatus.Active,
                    CreatedDate = new DateTime(2025, 01, 01)
                });

                await SaveChangesAsync();
            }
        }
    }
}
