using Microsoft.EntityFrameworkCore;
using WebApp.LLM;
using WebApp.Models;
namespace WebApp
{

    namespace ConstructionJournal.Data
    {
        public class ConstructionJournalDbContext : DbContext
        {
            public ConstructionJournalDbContext(DbContextOptions<ConstructionJournalDbContext> options)
                : base(options)
            {
            }

            // Основные сущности
            public DbSet<Project> Projects { get; set; }
            public DbSet<Contract> Contracts { get; set; }
            public DbSet<WorkType> WorkTypes { get; set; }
            public DbSet<ContractWorkType> ContractWorkTypes { get; set; }

            // Журнал работ
            public DbSet<WorkJournalEntry> WorkJournalEntries { get; set; }
            public DbSet<WorkJournalPhoto> WorkJournalPhotos { get; set; }

            // Отчетность и график
            public DbSet<DailyReport> DailyReports { get; set; }
            public DbSet<ConstructionSchedule> ConstructionSchedules { get; set; }

            // Материалы
            public DbSet<Material> Materials { get; set; }
            public DbSet<MaterialDelivery> MaterialDeliveries { get; set; }
            public DbSet<MaterialUsage> MaterialUsages { get; set; }
            public DbSet<MaterialRequirement> MaterialRequirements { get; set; }

            // Анализ LLM
            public DbSet<DocumentAnalysisResult> DocumentAnalysisResults { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Уникальные индексы
                modelBuilder.Entity<ContractWorkType>()
                    .HasIndex(cwt => new { cwt.ContractId, cwt.WorkTypeId })
                    .IsUnique();

                modelBuilder.Entity<DailyReport>()
                    .HasIndex(dr => new { dr.ProjectId, dr.ReportDate })
                    .IsUnique();

                // Конфигурации отношений
                modelBuilder.Entity<WorkJournalEntry>()
                    .HasOne(w => w.Project)
                    .WithMany(p => p.WorkJournalEntries)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<MaterialDelivery>()
                    .HasOne(m => m.Project)
                    .WithMany(p => p.MaterialDeliveries)
                    .OnDelete(DeleteBehavior.Restrict);

                // Конвертеры для JSON полей (для PostgreSQL)
                modelBuilder.Entity<MaterialDelivery>()
                    .Property(m => m.QualityDocuments)
                    .HasColumnType("jsonb");

                modelBuilder.Entity<DocumentAnalysisResult>()
                    .Property(d => d.RequestData)
                    .HasColumnType("jsonb");

                modelBuilder.Entity<DocumentAnalysisResult>()
                    .Property(d => d.ResponseData)
                    .HasColumnType("jsonb");

                modelBuilder.Entity<DocumentAnalysisResult>()
                    .Property(d => d.AnalysisResult)
                    .HasColumnType("jsonb");

                modelBuilder.Entity<DocumentAnalysisResult>()
                    .Property(d => d.NonCompliances)
                    .HasColumnType("jsonb");
            }
        }
    }
}
