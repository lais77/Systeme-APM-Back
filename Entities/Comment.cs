namespace APM.API.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Qui a écrit ce commentaire
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;

        // Sur quelle action
        public int ActionItemId { get; set; }
        public ActionItem ActionItem { get; set; } = null!;
    }
}