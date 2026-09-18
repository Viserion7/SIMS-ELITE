namespace sims_incidentManager.DTOs
{
    public class EscalateRequestDto
    {
        public Guid IncidentId { get; set; }
        public string Message { get; set; } = "";

    }
}
