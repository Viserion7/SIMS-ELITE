namespace sims_aggregator.DTOs
{
    public class CreateIncidentDto
    {

        public Guid id { get; set; }
        public DateTime created_at { get; set; }
        public string source_format { get; set; } = "";
        public string type { get; set; } = "";
        public string name { get; set; } = "";
        public string desc { get; set; } = "";

    }
}
