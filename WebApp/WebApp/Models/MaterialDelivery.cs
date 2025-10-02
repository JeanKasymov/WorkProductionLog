namespace WebApp.Models
{
    public class MaterialDelivery
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

        [Required]
        public DateTime DeliveryDate { get; set; }

        [MaxLength(200)]
        public string Supplier { get; set; }

        [MaxLength(100)]
        public string BatchNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        // Пути к файлам сертификатов (можно хранить как JSON или в отдельной таблице, но для простоты - строка с разделителем)
        public string QualityDocuments { get; set; } // Например, список путей через разделитель
    }
}
