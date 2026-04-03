namespace APM.API.Entities
{
    public class ActionItem
    {
        public int Id { get; set; }
        public string Theme { get; set; } = string.Empty;
        public string? AnomalyDescription { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
        public string Type { get; set; } = "Corrective";
        public string Criticity { get; set; } = "Medium";
        public string? Cause { get; set; }
        public string Status { get; set; } = "Created";
        public double ProgressPercentage { get; set; } = 0;
        public DateTime Deadline { get; set; }

        public string? RealizationMethod { get; set; }
        public DateTime? RealizationDate { get; set; }

        public string? VerificationMethod { get; set; }
        public DateTime? VerificationDate { get; set; }

        public string? Effectiveness { get; set; }
        public string? EffectivenessComment { get; set; }
        public int? StarRating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }

        public int ActionPlanId { get; set; }
        public ActionPlan ActionPlan { get; set; } = null!;

        public int ResponsibleId { get; set; }
        public User Responsible { get; set; } = null!;

        public int? ParentActionId { get; set; }
        public ActionItem? ParentAction { get; set; }
        public ICollection<ActionItem> ChildActions { get; set; } = new List<ActionItem>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}