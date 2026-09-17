using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sims_aggregator.Data;
using sims_aggregator.DTOs;
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
        [EndpointDescription("Get Incident data of an ID")]
        public async Task<ActionResult<Incident>> GetIncident(Guid id)
        {

            var incident = await this.context.Incidents.FindAsync(id);

            if (incident == null || incident.is_deleted == true)
            {
                this.logger.LogWarning("Incident with ID {IncidentId} not found.", id);
                return NotFound("Incident was not found.");
            }

            return Ok(incident);
        }

        [HttpGet("{page}/{count}")]
        [EndpointDescription("Get all Incident data")]
        public async Task<ActionResult<Incident[]>> GetIncidents(int page,int count)
        {
            if (page < 0 || count <= 0)
                return BadRequest(">:( must be >= 0 and count must be > 0.");

            var incidents = await this.context.Incidents
                .Where(i => !i.is_deleted)      // Gelöschte nicht mitgeben!
                .OrderByDescending(i => i.db_created_at) // Bei Paging IMMER sortieren, sonst springen die Daten
                .Skip(page * count)             // Überspringe die vorherigen Seiten
                .Take(count)                    // Nimm nur die gewünschte Anzahl
                .ToListAsync();

            return Ok(incidents);
        }

        [HttpPost]
        [EndpointDescription("Upload Incident data")]
        public async Task<ActionResult> AddIncident(CreateIncidentDto arg_incident)
        {
            if (await this.context.Incidents.FindAsync(arg_incident.id) != null)
                return Conflict("Incident already exists.");

            var incident = new Incident
            {
                id = arg_incident.id,
                created_at = arg_incident.created_at,
                db_created_at = DateTime.UtcNow,
                is_deleted = false,
                deleted_by = null,
                source_format = arg_incident.source_format,
                type = arg_incident.type,
                name = arg_incident.name,
                desc = arg_incident.desc,
            };

            this.context.Incidents.Add(incident);
            await this.context.SaveChangesAsync();

            return Created();
        }

        [HttpDelete("{id}")]
        [EndpointDescription("Delete Incident data")]
        public async Task<ActionResult> DeleteIncident(Guid id)
        {
            var incident = await this.context.Incidents.FindAsync(id);

            if (incident == null || incident.is_deleted == true)
            {
                this.logger.LogWarning("Incident with ID {IncidentId} not found.", id);
                return NotFound("Incident was not found.");
            }

            incident.is_deleted = true;

            await this.context.SaveChangesAsync();

            return NoContent();
        }

    }
}
