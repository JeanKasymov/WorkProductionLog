namespace WebApp.Models
{
    public class DailyReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; }

        [Required]
        public DateTime ReportDate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal GeneralProgress { get; set; } // Общий прогресс в процентах

        public string WorkDetails { get; set; } // Детали выполненных работ

        public string Issues { get; set; } // Проблемы

        public string NextDayPlan { get; set; } // План на следующий день
    }
}
