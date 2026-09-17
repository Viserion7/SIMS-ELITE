//inspiration: https://medium.com/@aschultzme/introduction-to-entity-framework-building-a-simple-crud-api-in-net-b22eb2efbe6e
using Microsoft.AspNetCore.Mvc;
using sims_identity.Dtos;

namespace sims_identity.Controllers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using sims_identity.Data;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using sims_identity.Services;


[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserController> _logger;

    private readonly UserService _userService;

    public UserController(ApplicationDbContext context, ILogger<UserController> logger, UserService userService)
    {
        _context = context;
        _logger = logger;
        _userService = userService;
    }


    [HttpPost]
    [Authorize(Policy = "isAdmin")]
    [EndpointDescription("Legt einen neuen Benutzer an.\n\nEinschränkung:\n- Nur Administratoren")]
    public async Task<ActionResult<CreateUserDto>> CreateUser(CreateUserDto incommingUser)
    {

        if (_context.User.Any(u => u.email == incommingUser.email))
        {
            return BadRequest();
        }

        User user = new User
        {
            email = incommingUser.email,
            password_hash = BCrypt.HashPassword(incommingUser.password),
            is_deleted = false
        };


        _context.User.Add(user);

        await _context.SaveChangesAsync();

        return Created();
    }

    [HttpGet("{id}")]
    [EndpointDescription("Gibt die Daten eines Benutzers zurück. Wenn kein Benutzer mit dieser ID existiert, wird 404 zurückgegeben.\n\nEinschränkung:\n- Eigene Daten: erlaubt\n- Andere Benutzer: nur Administratoren")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        if (!await IsUserAllowed(id))
        {
            return Forbid();
        }




        var user = await _context.User.
            Where(user => user.id == id)
            .Select(user => new UserDto
            {
                id = user.id,
                email = user.email,
                is_deleted = user.is_deleted
            }).FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return user;
    }

    [HttpGet]
    [Authorize(Policy = "isAdmin")]
    [EndpointDescription("Gibt alle Benutzer zurück.\n\nEinschränkung:\n- Nur Administratoren")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.User.ToArrayAsync();
    }


    [HttpPut("{id}")]
    [Authorize(Policy = "isAdmin")]
    [EndpointDescription("Ändert die Daten eines Benutzers.\n\nEinschränkung:\n- Nur Administratoren")]
    public async Task<ActionResult> UpdateUser(int id, UpdateUserDto updatedUser)
    {


        var userEntity = await _context.User.FindAsync(id);
        if (userEntity == null)
            return NotFound();

        userEntity.email = updatedUser.email;
        userEntity.password_hash = BCrypt.HashPassword(updatedUser.password);
        userEntity.is_deleted = updatedUser.is_deleted;
        userEntity.is_Admin = updatedUser.is_Admin;

        _context.Entry(userEntity).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            return BadRequest();
        }

        return NoContent();
    }


    [HttpDelete("{id}")]
    [Authorize(Policy = "isAdmin")]
    [EndpointDescription("Markiert einen Benutzer als gelöscht.\n\nEinschränkung:\n- Nur Administratoren")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.User.FindAsync(id);
        if (user == null)
            return NotFound();
        user.is_deleted = true;

        try
        {
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return BadRequest();
        }


    }
    [HttpGet("{id}/details")]
    [EndpointDescription("Gibt einen Benutzer mit seinen Levels und Kategorien zurück.\n\nEinschränkung:\n- Eigene Daten: erlaubt\n- Andere Benutzer: nur Administratoren")]
    public async Task<ActionResult<User>> GetDetails(int id)
    {
        if (!await IsUserAllowed(id))
        {
            return Forbid();
        }


        var user = await _context.User
    .Include(user => user.Levels)
        .ThenInclude(level => level.Categorys)
    .FirstOrDefaultAsync(user => user.id == id);

        if (user == null)
            return NotFound();

        return user;
    }

    private async Task<bool> IsUserAllowed(int requestedUserId)
    {
        var userIdClaim = Int32.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);
        var allowed = await _userService.CheckuserID(userIdClaim, requestedUserId);
        if (!allowed)
        {
            return false;
        }
        return true;
    }


}