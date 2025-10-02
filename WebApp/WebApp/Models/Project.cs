using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WebApp.Models
{

    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public ProjectStatus Status { get; set; } = ProjectStatus.Planned;

        // Навигационные свойства
        public virtual ICollection<Contract> Contracts { get; set; }
        public virtual ICollection<WorkJournalEntry> WorkJournalEntries { get; set; }
        public virtual ICollection<DailyReport> DailyReports { get; set; }
        public virtual ICollection<ConstructionSchedule> ConstructionSchedules { get; set; }
        public virtual ICollection<MaterialDelivery> MaterialDeliveries { get; set; }
        public virtual ICollection<MaterialUsage> MaterialUsages { get; set; }
    }

    public enum ProjectStatus
    {
        Planned,
        InProgress,
        Suspended,
        Completed,
        Cancelled
    }
}
