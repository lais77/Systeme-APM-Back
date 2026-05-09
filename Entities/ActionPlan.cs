namespace APM.API.Entities
{
    public class ActionPlan
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Objective { get; set; }
        public string Priority { get; set; } = "Medium";    // Critical, High, Medium, Low
        public string Status { get; set; } = "Draft";       // Draft, Validated, InProgress, Closed
        public string Type { get; set; } = "Mono"; // Mono ou Multi
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public double ProgressPercentage { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        // Le pilote qui a créé ce plan
        public int PilotId { get; set; }
        public User Pilot { get; set; } = null!;

        // Le processus concerné
        public int ProcessId { get; set; }
        public Process Process { get; set; } = null!;

        // Le département concerné
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        // Un plan contient plusieurs actions
        public ICollection<ActionItem> Actions { get; set; } = new List<ActionItem>();

        // Pour le type "Multi", on peut avoir plusieurs co-pilotes
        public ICollection<User> CoPilots { get; set; } = new List<User>();
    }
}