using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sims_aggregator.Data;
using sims_aggregator.Models;

namespace sims_aggregator.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class IncidentsController: ControllerBase
    {
        private readonly IncidentContext context;
        private readonly ILogger<IncidentsController> logger;

        public IncidentsController(IncidentContext context, ILogger<IncidentsController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        [HttpGet("{id}")]
        [EndpointDescription("Get Incident data")]
        public async Task<ActionResult<Incident>> GetIncident(Guid id)
        {

            var incident = await this.context.Incidents.FindAsync(id);

            if (incident == null)
            {
                this.logger.LogWarning("Incident with ID {IncidentId} not found.", id);
                return NotFound("Incident was not found.");
            }

            return Ok(incident);
        }
    }
}
