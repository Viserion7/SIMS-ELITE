//inspiration: https://medium.com/@aschultzme/introduction-to-entity-framework-building-a-simple-crud-api-in-net-b22eb2efbe6e
using Microsoft.AspNetCore.Mvc;

namespace sims_identity.Controllers;

using Microsoft.Extensions.Logging;

using sims_identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Policy = "isAdmin")]
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
    [EndpointDescription("Weist einem Benutzer ein Level zu.\n\nEinschränkung:\n- Nur Administratoren")]
    public async Task<ActionResult<User>> LevelUserZuweisen(int id, int levelid)
    {
        var user = await _context.User
            .Include(u => u.Levels)
            .FirstOrDefaultAsync(u => u.id == id);

        var level = await _context.Level
            .FirstOrDefaultAsync(l => l.id == levelid);

        if (user == null || level == null)
        {
            return NotFound();
        }

        if (!user.Levels.Any(l => l.id == levelid))
        {
            user.Levels.Add(level);
            await _context.SaveChangesAsync();
        }

        return Ok(user);
    }

    [HttpDelete("user/{id}/level/{levelid}")]
    [EndpointDescription("Entfernt ein Level von einem Benutzer.\n\nEinschränkung:\n- Nur Administratoren")]
    public async Task<ActionResult<User>> LevelVonUserLoeschen(int id, int levelid)
    {
        var user = await _context.User
            .Include(u => u.Levels)
            .FirstOrDefaultAsync(u => u.id == id);

        if (user == null)
        {
            return NotFound();
        }

        var levelToRemove = user.Levels.FirstOrDefault(l => l.id == levelid);
        if (levelToRemove != null)
        {
            user.Levels.Remove(levelToRemove);
            await _context.SaveChangesAsync();
        }

        return Ok(user);
    }





}