namespace APM.API.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;     // Login, CreatePlan, etc.
        public string? EntityType { get; set; }                 // User, ActionPlan, etc.
        public int? EntityId { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Qui a fait cette action
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}