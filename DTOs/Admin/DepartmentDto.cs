namespace APM.API.DTOs.Admin
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserCount { get; set; }
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}