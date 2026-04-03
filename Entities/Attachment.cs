namespace APM.API.Entities
{
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long FileSize { get; set; }
        public int Version { get; set; } = 1;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Qui a uploadé
        public int UploadedById { get; set; }
        public User UploadedBy { get; set; } = null!;

        // Sur quelle action
        public int ActionItemId { get; set; }
        public ActionItem ActionItem { get; set; } = null!;
    }
}