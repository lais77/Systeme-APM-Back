namespace APM.API.DTOs.Stats
{
    public class StatsByDeptDto
    {
        public int departmentId { get; set; }
        public string departmentName { get; set; } = "";
        public int totalPlans { get; set; }
        public int totalActions { get; set; }
        public int actionsEnRetard { get; set; }
        public int actionsCloturees { get; set; }
    }
}
