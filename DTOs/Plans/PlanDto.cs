using APM.API.DTOs.Actions;

namespace APM.API.DTOs.Plans
{
    public class PlanDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Objective { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public double ProgressPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int PilotId { get; set; }
        public string PilotName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int TotalActions { get; set; }
        public List<ActionDto>? Actions { get; set; }
        public List<UserSummaryDto>? CoPilots { get; set; }
    }

    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
    }

    public class CreatePlanDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Objective { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Type { get; set; } = "Mono";
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int ProcessId { get; set; }
        public int? DepartmentId { get; set; }
        public List<int>? CoPilotIds { get; set; }
    }

    public class UpdatePlanDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Objective { get; set; }
        public string? Priority { get; set; }
        public string? Type { get; set; }
        public DateTime? DueDate { get; set; }
    }
}