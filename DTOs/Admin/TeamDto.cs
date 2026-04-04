namespace APM.API.DTOs.Admin
{
    public class TeamDto
    {
        public int id { get; set; }
        public string nom { get; set; } = string.Empty;
        public int departementId { get; set; }
        public string? departement { get; set; }
        public int nombreMembres { get; set; }
    }

    public class CreateTeamDto
    {
        public string nom { get; set; } = string.Empty;
        public int departementId { get; set; }
    }

    public class UpdateTeamDto
    {
        public string nom { get; set; } = string.Empty;
        public int departementId { get; set; }
    }
}