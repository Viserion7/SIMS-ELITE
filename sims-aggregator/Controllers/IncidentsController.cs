using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sims_aggregator.Data;
using sims_aggregator.DTOs;
using sims_aggregator.Models;
using System.Net;
using System.Text.Json.Nodes;

namespace sims_aggregator.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class IncidentsController: ControllerBase
    {
        private readonly dbContext context;
        private readonly ILogger<IncidentsController> logger;
        private readonly HttpClient httpClient;

        public IncidentsController(dbContext context, ILogger<IncidentsController> logger, HttpClient httpClient)
        {
            this.context = context;
            this.logger = logger;
            this.httpClient = httpClient;
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
            // 1. Prüfen, ob der Aufrufer überhaupt einen Token mitgeschickt hat
            var authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized("No Authorization header provided.");
            }

            // 2. Request an identity vorbereiten und den Bearer-Token 1:1 mitschicken
            var identityUrl = Environment.GetEnvironmentVariable("IDENTITY_URL") ?? "http://identity:8080";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{identityUrl}/api/v1/Auth/me");
            request.Headers.Add("Authorization", authHeader);

            // 3. Identity-Service anfragen
            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Could not reach identity service");
                return StatusCode(503, "Identity service unavailable.");
            }

            // 4. Statuscodes prüfen
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return Unauthorized("Invalid or expired session token.");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return Forbid();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Authentication check failed.");

            // 5. User-ID (jetzt als int) aus dem JSON von /me extrahieren
            var json = await response.Content.ReadFromJsonAsync<JsonObject>();
            int? userId = null;

            if (json != null && json.TryGetPropertyValue("id", out var idNode) && idNode != null)
            {
                if (int.TryParse(idNode.ToString(), out var parsedId))
                    userId = parsedId;
            }

            // 6. Den Vorfall in Postgres suchen und als gelöscht markieren
            var incident = await this.context.Incidents.FindAsync(id);

            if (incident == null || incident.is_deleted == true)
            {
                this.logger.LogWarning("Incident with ID {IncidentId} not found.", id);
                return NotFound("Incident was not found.");
            }

            incident.is_deleted = true;
            incident.deleted_by = userId;

            await this.context.SaveChangesAsync();

            this.logger.LogInformation("Incident {IncidentId} was soft-deleted by User {UserId}", id, userId);

            return NoContent();
        }
    }
}
