using Microsoft.AspNetCore.Mvc;

namespace sims_incidentManager.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EskalationController
    {
        private readonly ILogger<EskalationController> logger;
        private readonly HttpClient httpClient;

        public EskalationController(ILogger<EskalationController> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
        }
    }
}
