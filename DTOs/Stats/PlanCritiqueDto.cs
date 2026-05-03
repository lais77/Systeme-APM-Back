namespace APM.API.DTOs.Stats
{
    public class PlanCritiqueDto
    {
        public int id { get; set; }
        public string title { get; set; } = "";
        public string status { get; set; } = "";
        public string priority { get; set; } = "";
        public string pilotName { get; set; } = "";
        public string departmentName { get; set; } = "";
        public int actionsEnRetard { get; set; }
        public int totalActions { get; set; }
        public DateTime dateEcheance { get; set; }
    }
}
