using Microsoft.AspNetCore.Mvc;

namespace sims_identity.Controllers;

using Microsoft.Extensions.Logging;
using sims_identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class LevelsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LevelsController> _logger;

    public LevelsController(ApplicationDbContext context, ILogger<LevelsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [EndpointDescription("Gibt alle verfügbaren Level zurück.\n\nEinschränkung:\n- Jeder angemeldete Benutzer")]
    public async Task<ActionResult<List<Level>>> GetAllLevels()
    {
        return await _context.Level.ToListAsync();
    }
}
