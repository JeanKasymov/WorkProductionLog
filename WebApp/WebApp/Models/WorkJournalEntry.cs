namespace WebApp.Models
{
    public class WorkJournalEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; }

        [Required]
        public int WorkTypeId { get; set; }

        [ForeignKey(nameof(WorkTypeId))]
        public virtual WorkType WorkType { get; set; }

        [Required]
        public int ContractorUserId { get; set; } // ID пользователя-подрядчика (связано с пользователем в системе)

        [Required]
        public DateTime Date { get; set; }

        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CompletedVolume { get; set; }

        // Геолокация
        public double? GeoLatitude { get; set; }
        public double? GeoLongitude { get; set; }
        public double? GeoAccuracy { get; set; }
        public string GeoAddress { get; set; } // Адрес, полученный по координатам

        [Required]
        public WorkJournalStatus Status { get; set; } = WorkJournalStatus.Draft;

        // Навигационные свойства
        public virtual ICollection<WorkJournalPhoto> Photos { get; set; }
    }

    public enum WorkJournalStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected
    }
}
