namespace WebApp.Models
{
    public class ConstructionSchedule
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
        public DateTime PlannedStartDate { get; set; }

        [Required]
        public DateTime PlannedEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        // Для управления зависимостями задач (например, ID предыдущей задачи)
        public int? PredecessorId { get; set; }

        [ForeignKey(nameof(PredecessorId))]
        public virtual ConstructionSchedule Predecessor { get; set; }
    }
}
