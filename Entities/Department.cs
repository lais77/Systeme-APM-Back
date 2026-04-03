namespace APM.API.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Un département a plusieurs utilisateurs
        public ICollection<User> Users { get; set; } = new List<User>();

        // Un département a plusieurs équipes
        public ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}