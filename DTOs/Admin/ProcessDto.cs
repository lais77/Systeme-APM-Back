namespace APM.API.DTOs.Admin
{
    public class ProcessDto
    {
        public int id { get; set; }
        public string nom { get; set; } = string.Empty;
        public string? description { get; set; }
        public bool actif { get; set; }
    }

    public class CreateProcessDto
    {
        public string nom { get; set; } = string.Empty;
        public string? description { get; set; }
    }

    public class UpdateProcessDto
    {
        public string nom { get; set; } = string.Empty;
        public string? description { get; set; }
        public bool actif { get; set; }
    }
}