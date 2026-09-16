//inspiration: https://medium.com/@aschultzme/introduction-to-entity-framework-building-a-simple-crud-api-in-net-b22eb2efbe6e
using Microsoft.AspNetCore.Mvc;

namespace sims_identity.Controllers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using sims_identity.Data;


[Route("api/v1/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }






    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        _context.User.Add(user);

        await _context.SaveChangesAsync();

        return Created();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.User.FindAsync(id);

        if (user == null)
            return NotFound();

        return user;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.User.ToArrayAsync();
    }


    // PUT: api/Users/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, User updatedUser)
    {

        var userEntity = await _context.User.FindAsync(id);

        userEntity.email = updatedUser.email;
        userEntity.password_hash = updatedUser.password_hash;
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

}