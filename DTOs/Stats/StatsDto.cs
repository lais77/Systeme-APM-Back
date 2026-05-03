namespace APM.API.DTOs.Stats
{
    public class GlobalStatsDto
    {
        public int totalPlans { get; set; }
        public int totalActions { get; set; }
        public int actionsEnCours { get; set; }
        public int actionsEnRetard { get; set; }
        public int actionsCloturees { get; set; }
        public double tauxRealisation { get; set; }
        public double tauxCloture { get; set; }
        public double tauxEfficacite { get; set; }
    }



    public class MonthlyStatsDto
    {
        public int month { get; set; }
        public int year { get; set; }
        public int actionsCloturees { get; set; }
        public int actionsEnRetard { get; set; }
    }
}