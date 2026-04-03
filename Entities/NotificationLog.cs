namespace APM.API.Entities
{
    public class NotificationLog
    {
        public int Id { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;    // Reminder, Escalation_N1, N2, N3
        public bool IsSent { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Lié à quelle action (optionnel)
        public int? ActionItemId { get; set; }
        public ActionItem? ActionItem { get; set; }
    }
}