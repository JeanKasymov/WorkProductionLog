namespace WebApp.LLM
{
    public class DocumentAnalysisResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RelatedEntityId { get; set; } // ID связанной сущности (например, MaterialDelivery или WorkJournalEntry)

        [Required]
        [MaxLength(100)]
        public string RelatedEntityType { get; set; } // Тип сущности, например "MaterialDelivery"

        [Required]
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        // Результаты анализа можно хранить в формате JSON
        public string AnalysisResult { get; set; } // JSON с результатами, которые возвращает LLM

        // Статус анализа
        public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;

        public string ErrorMessage { get; set; }
    }

    public enum AnalysisStatus
    {
        Pending,
        Completed,
        Failed
    }
}
