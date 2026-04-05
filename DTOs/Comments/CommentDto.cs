namespace APM.API.DTOs.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int ActionItemId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCommentDto
    {
        public string Content { get; set; } = string.Empty;
    }
}