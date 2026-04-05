namespace APM.API.DTOs.Actions
{
    public class ActionDto
    {
        public int Id { get; set; }
        public string Theme { get; set; } = string.Empty;
        public string? AnomalyDescription { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Criticity { get; set; } = string.Empty;
        public string? Cause { get; set; }
        public string Status { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; }
        public DateTime Deadline { get; set; }
        public string? RealizationMethod { get; set; }
        public DateTime? RealizationDate { get; set; }
        public string? VerificationMethod { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string? Effectiveness { get; set; }
        public string? EffectivenessComment { get; set; }
        public int? StarRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ActionPlanId { get; set; }
        public int ResponsibleId { get; set; }
        public string ResponsibleName { get; set; } = string.Empty;
        public int? ParentActionId { get; set; }
    }

    public class CreateActionDto
    {
        public string Theme { get; set; } = string.Empty;
        public string? AnomalyDescription { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
        public string Type { get; set; } = "Corrective";
        public string Criticity { get; set; } = "Medium";
        public string? Cause { get; set; }
        public DateTime Deadline { get; set; }
        public int ResponsibleId { get; set; }
    }

    public class UpdateActionDto
    {
        public string? Theme { get; set; }
        public string? ActionDescription { get; set; }
        public string? Type { get; set; }
        public string? Criticity { get; set; }
        public string? Cause { get; set; }
        public DateTime? Deadline { get; set; }
        public int? ResponsibleId { get; set; }
        public double? ProgressPercentage { get; set; }
    }

    public class SubmitActionDto
    {
        public string RealizationMethod { get; set; } = string.Empty;
        public DateTime RealizationDate { get; set; }
    }

    public class ValidateActionDto
    {
        public bool IsApproved { get; set; }
        public string? Comment { get; set; }
    }

    public class EvaluateActionDto
    {
        public string Effectiveness { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public int? StarRating { get; set; }
        public CreateActionDto? ReplacementAction { get; set; }
    }
}