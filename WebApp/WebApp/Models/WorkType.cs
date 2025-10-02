namespace WebApp.Models
{
    public class WorkType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Code { get; set; }

        public string Description { get; set; }

        // Навигационные свойства
        public virtual ICollection<ContractWorkType> ContractWorkTypes { get; set; }
        public virtual ICollection<WorkJournalEntry> WorkJournalEntries { get; set; }
        public virtual ICollection<ConstructionSchedule> ConstructionSchedules { get; set; }
    }
}
