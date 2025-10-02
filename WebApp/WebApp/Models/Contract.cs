namespace WebApp.Models
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; }

        [Required]
        [MaxLength(200)]
        public string ContractorCompany { get; set; }

        [Required]
        [MaxLength(200)]
        public string ClientCompany { get; set; }

        [Required]
        [MaxLength(100)]
        public string Number { get; set; }

        [Required]
        public DateTime SignDate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        // Навигационные свойства
        public virtual ICollection<ContractWorkType> ContractWorkTypes { get; set; }
    }

    public enum ContractStatus
    {
        Active,
        Suspended,
        Terminated,
        Completed
    }

}
