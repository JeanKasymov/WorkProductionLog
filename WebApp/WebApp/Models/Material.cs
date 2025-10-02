namespace WebApp.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; }

        // Навигационные свойства
        public virtual ICollection<MaterialDelivery> MaterialDeliveries { get; set; }
        public virtual ICollection<MaterialUsage> MaterialUsages { get; set; }
    }
}
