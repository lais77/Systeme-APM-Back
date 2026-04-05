namespace APM.API.DTOs.Attachments
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public int ActionItemId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public int UploadedById { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
    }
}