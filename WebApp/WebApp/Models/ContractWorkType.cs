namespace WebApp.Models
{
    public class ContractWorkType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public virtual Contract Contract { get; set; }

        [Required]
        public int WorkTypeId { get; set; }

        [ForeignKey(nameof(WorkTypeId))]
        public virtual WorkType WorkType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PlannedVolume { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }
    }
}
