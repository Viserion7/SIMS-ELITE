//inspiration: https://medium.com/@aschultzme/introduction-to-entity-framework-building-a-simple-crud-api-in-net-b22eb2efbe6e
using Microsoft.AspNetCore.Mvc;
using sims_identity.Dtos;

namespace sims_identity.Controllers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using sims_identity.Data;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;


[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Policy = "isAdmin")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(ApplicationDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }


    [HttpPost]
    [EndpointDescription("Legt einen neuen Benutzer an.")]
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
    [EndpointDescription("Gibt die Daten eines Benutzers zurück. Wenn kein Benutzer mit dieser ID existiert, wird 404 zurückgegeben.")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
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
    [EndpointDescription("Gibt alle Benutzer zurück.")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.User.ToArrayAsync();
    }


    [HttpPut("{id}")]
    [EndpointDescription("Ändert die Daten eines Benutzers.")]
    public async Task<ActionResult> UpdateUser(int id, UpdateUserDto updatedUser)
    {


        var userEntity = await _context.User.FindAsync(id);
        if (userEntity == null)
            return NotFound();

        userEntity.email = updatedUser.email;
        userEntity.password_hash = BCrypt.HashPassword(updatedUser.password);
        userEntity.is_deleted = updatedUser.is_deleted;

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
    [EndpointDescription("Markiert einen Benutzer als gelöscht.")]
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
    [EndpointDescription("Gibt einen Benutzer mit seinen Levels und Kategorien zurück.")]
    public async Task<ActionResult<User>> GetDetails(int id)
    {
        var user = await _context.User
    .Include(user => user.Levels)
        .ThenInclude(level => level.Categorys)
    .FirstOrDefaultAsync(user => user.id == id);

        if (user == null)
            return NotFound();

        return user;
    }

}