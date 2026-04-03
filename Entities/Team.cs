namespace APM.API.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Une équipe appartient à un département
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // Une équipe a plusieurs membres
        public ICollection<User> Members { get; set; } = new List<User>();
    }
}