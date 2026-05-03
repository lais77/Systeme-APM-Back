namespace APM.API.DTOs.Stats
{
    public class StatsByPilotDto
    {
        public int pilotId { get; set; }
        public string pilotName { get; set; } = "";
        public int totalPlans { get; set; }
        public int actionsEnRetard { get; set; }
        public int actionsCloturees { get; set; }
    }
}
