//inspiration: https://medium.com/@aschultzme/introduction-to-entity-framework-building-a-simple-crud-api-in-net-b22eb2efbe6e
using Microsoft.AspNetCore.Mvc;

namespace sims_identity.Controllers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using sims_identity.Data;


[Route("api/v1/[controller]")]
[ApiController]
public class AssignmentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssignmentController> _logger;

    public AssignmentController(ApplicationDbContext context, ILogger<AssignmentController> logger)
    {
        _context = context;
        _logger = logger;
    }


    [HttpPost("user/{id}/level")]
    [EndpointDescription("Weist einem Benutzer ein Level zu.")]
    public async Task<ActionResult<User>> LevelUserZuweisen(int id)
    {
        return NoContent(); // to do
    }

    [HttpDelete("user/{id}/level/{levelid}")]
    [EndpointDescription("Entfernt ein Level von einem Benutzer.")]
    public async Task<ActionResult<User>> LevelVonUserLoeschen(int id, int levelid)
    {
        return NoContent(); // to do
    }





}