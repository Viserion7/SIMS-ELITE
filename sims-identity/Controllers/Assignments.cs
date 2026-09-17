//inspiration: https://medium.com/@aschultzme/introduction-to-entity-framework-building-a-simple-crud-api-in-net-b22eb2efbe6e
using Microsoft.AspNetCore.Mvc;

namespace sims_identity.Controllers;

using Microsoft.Extensions.Logging;

using sims_identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class AssignmentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssignmentController> _logger;

    public AssignmentController(ApplicationDbContext context, ILogger<AssignmentController> logger)
    {
        _context = context;
        _logger = logger;
    }


    [HttpPost("user/{id}/level/{levelid}")]
    [EndpointDescription("Weist einem Benutzer ein Level zu.")]
    public async Task<ActionResult<User>> LevelUserZuweisen(int id, int levelid)
    {
        var user = await _context.User.
            Where(user => user.id == id).FirstOrDefaultAsync();

        var level = await _context.Level.
            Where(level => level.id == levelid).FirstOrDefaultAsync();

        if (user == null || level == null)
        {
            return NotFound();
        }

        user.Levels.Add(level);
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpDelete("user/{id}/level/{levelid}")]
    [EndpointDescription("Entfernt ein Level von einem Benutzer.")]
    public async Task<ActionResult<User>> LevelVonUserLoeschen(int id, int levelid)
    {
        var user = await _context.User.
            Where(user => user.id == id).FirstOrDefaultAsync();

        var level = await _context.Level.
            Where(level => level.id == levelid).FirstOrDefaultAsync();

        if (user == null || level == null)
        {
            return NotFound();
        }

        user.Levels.Remove(level);
        await _context.SaveChangesAsync();

        return Ok(user);
    }





}