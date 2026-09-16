//inspiration: https://medium.com/@aschultzme/introduction-to-entity-framework-building-a-simple-crud-api-in-net-b22eb2efbe6e
using Microsoft.AspNetCore.Mvc;
using sims_identity.Dtos;

namespace sims_identity.Controllers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using sims_identity.Data;


[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }


    [HttpPost("login")]
    public async Task<ActionResult<User>> LoginUser(CreateUserDto user)
    {
        User? bestehendesItem = _context.User
                .FirstOrDefault(inDbSchonExestierend => inDbSchonExestierend.email == user.email);
        if (bestehendesItem == null)
        {
            return NotFound();
        }
        if (bestehendesItem.password_hash != user.password) // to do noch pw hash einbauen
        {
            return Unauthorized();
        }

        return Ok(new
        {
            message = "Login erfolgreich",
            userId = bestehendesItem.id,
            email = bestehendesItem.email,
            token = "eyJh..."
        });
    }



    [HttpGet("me")]
    public async Task<ActionResult<TokenDto>> GetMe(TokenDto token)
    {
        return token; // To Do Implement me
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenDto>> RefreshMe(TokenDto token)
    {
        return token; // To Do Implement me
    }

    [HttpPost("logout")]
    public async Task<ActionResult<TokenDto>> LogoutMe(TokenDto token)
    {
        return token; // To Do Implement me
    }


}