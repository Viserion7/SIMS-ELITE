using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Nodes;

namespace sims_incidentManager.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EskalationController : ControllerBase
    {
        private readonly ILogger<EskalationController> logger;
        private readonly HttpClient httpClient;

        public EskalationController(ILogger<EskalationController> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
        }

        [HttpGet("{id}/{msg}")]
        [EndpointDescription("Escalate Incident to make an Admin look at it or delegate it")]

        public async Task<ActionResult> Escalate(Guid incident_id, string msg)
        {

            return Created();
        }


    }
}
