namespace APM.API.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;      // Admin, Manager, Responsable, Auditeur
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // Pour reset mot de passe
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        // Chef hiérarchique
        public int? ManagerId { get; set; }
        public User? Manager { get; set; }

        // Département et équipe
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public int? TeamId { get; set; }
        public Team? Team { get; set; }

        // Relations
        public ICollection<User> Subordinates { get; set; } = new List<User>();
        public ICollection<ActionPlan> ManagedPlans { get; set; } = new List<ActionPlan>();
        public ICollection<ActionItem> AssignedActions { get; set; } = new List<ActionItem>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}