namespace WebApp.Models
{
    public class WorkJournalPhoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EntryId { get; set; }

        [ForeignKey(nameof(EntryId))]
        public virtual WorkJournalEntry WorkJournalEntry { get; set; }

        [Required]
        public string FilePath { get; set; } // Относительный путь к файлу в хранилище

        [Required]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    }
}
