namespace APM.API.DTOs.Stats
{
    public class GlobalStatsDto
    {
        public int TotalPlans { get; set; }
        public int TotalActions { get; set; }
        public int ActionsEnCours { get; set; }
        public int ActionsEnRetard { get; set; }
        public int ActionsCloturees { get; set; }
        public double TauxRealisation { get; set; }
        public double TauxCloture { get; set; }
        public double TauxEfficacite { get; set; }
    }

    public class StatsByDepartmentDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalPlans { get; set; }
        public int TotalActions { get; set; }
        public int ActionsCloturees { get; set; }
        public double TauxCloture { get; set; }
    }

    public class StatsByPilotDto
    {
        public string PilotName { get; set; } = string.Empty;
        public int TotalPlans { get; set; }
        public int TotalActions { get; set; }
        public double TauxCloture { get; set; }
    }

    public class MonthlyStatsDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int ActionsCloturees { get; set; }
        public int ActionsEnRetard { get; set; }
    }
}