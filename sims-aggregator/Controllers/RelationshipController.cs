using Microsoft.AspNetCore.Mvc;
using sims_aggregator.Data;
using sims_aggregator.Models;
using Microsoft.EntityFrameworkCore;


namespace sims_aggregator.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RelationshipController : ControllerBase
    {
        private readonly dbContext context;
        private readonly ILogger<Relationship> logger;
        public RelationshipController(dbContext context, ILogger<Relationship> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        [HttpPost]
        [EndpointDescription("Upload Relationship")]
        public async Task<ActionResult> AddRelationship(Relationship arg_relationship)
        {
            var from_Incident = await context.Incidents.FindAsync(arg_relationship.idFrom);
            var to_Incident = await context.Incidents.FindAsync(arg_relationship.idTo);

            if (from_Incident == null || to_Incident == null)
                return NotFound("Incident was not found.");

            if (from_Incident == to_Incident)
                return Conflict("from and to Incident may not be the same");

            if (await context.Relationships.FindAsync(arg_relationship.id) != null)
                return Conflict("This relationship with this ID already exists");

            this.context.Relationships.Add(arg_relationship);
            await this.context.SaveChangesAsync();

            return Created();
        }

        [HttpGet("{id}")]
        [EndpointDescription("Get Relationship by ID")]
        public async Task<ActionResult> GetRelationship(Guid id)
        {
            var relationship = await this.context.Relationships.FindAsync(id);

            if (relationship == null)
                return NotFound("relationship by this ID has not been found");

            return Ok(relationship);
        }

        [HttpGet("{page}/{count}")]
        [EndpointDescription("Get all Relationship data")]
        public async Task<ActionResult<Relationship[]>> GetRelationships(int page, int count)
        {
            if (page < 0 || count <= 0)
                return BadRequest(">:( must be >= 0 and count must be > 0.");

            var relationships = await this.context.Relationships
                .OrderByDescending(i => i.idFrom) // Bei Paging IMMER sortieren, sonst springen die Daten
                .Skip(page * count)             // Überspringe die vorherigen Seiten
                .Take(count)                    // Nimm nur die gewünschte Anzahl
                .ToListAsync();

            return Ok(relationships);
        }

    }
}