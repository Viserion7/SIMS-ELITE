using System.ComponentModel.DataAnnotations;

namespace sims_aggregator.Models
{
    public class Incident
    {
        public Incident()
        {
        }
        public Guid id { get; set; }
        public Guid eid { get; set; } // extra entity ID ,which user it created
        public DateTime created_at { get; set; } // received from TAXII
        public DateTime db_created_at { get; set; } // when written inside db
        public bool is_deleted { get; set; }
        public Guid deleted_by { get; set; }
        public string source_format { get; set; } = ""; // taxii or raw json or what...

        // -- now all the TAXII specific values
        public string type { get; set; } = "";
        public string name { get; set; } = "";
        public string desc { get; set; } = "";
    }
}