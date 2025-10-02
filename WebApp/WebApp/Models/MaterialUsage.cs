namespace WebApp.Models
{
    public class MaterialUsage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public virtual Material Material { get; set; }

        public int? WorkJournalEntryId { get; set; } // Связь с записью в журнале работ

        [ForeignKey(nameof(WorkJournalEntryId))]
        public virtual WorkJournalEntry WorkJournalEntry { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityUsed { get; set; }
    }
}
