namespace APM.API.DTOs.Admin
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int MemberCount { get; set; }
    }

    public class CreateTeamDto
    {
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
    }

    public class UpdateTeamDto
    {
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
    }
}