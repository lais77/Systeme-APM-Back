namespace APM.API.DTOs.Admin
{
    public class DepartmentDto
    {
        public int id { get; set; }
        public string nom { get; set; } = string.Empty;
        public string? description { get; set; }
        public int nombreUtilisateurs { get; set; }
    }

    public class CreateDepartmentDto
    {
        public string nom { get; set; } = string.Empty;
        public string? description { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string nom { get; set; } = string.Empty;
        public string? description { get; set; }
    }
}