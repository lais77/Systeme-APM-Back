namespace APM.API.DTOs.Admin
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? DepartmentName { get; set; }
        public string? TeamName { get; set; }
        public string? ManagerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public int? TeamId { get; set; }
        public int? ManagerId { get; set; }
    }

    public class UpdateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public int? TeamId { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; }
    }
}