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
    [EndpointDescription("Meldet einen Benutzer an.")]
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
    [EndpointDescription("Gibt den aktuell angemeldeten Benutzer zurück.")]
    public async Task<ActionResult<TokenDto>> GetMe(TokenDto token)
    {
        return token; // To Do Implement me
    }

    [HttpPost("refresh")]
    [EndpointDescription("Erstellt ein neues Zugriffstoken.")]
    public async Task<ActionResult<TokenDto>> RefreshMe(TokenDto token)
    {
        return token; // To Do Implement me
    }

    [HttpPost("logout")]
    [EndpointDescription("Meldet den Benutzer ab.")]
    public async Task<ActionResult<TokenDto>> LogoutMe(TokenDto token)
    {
        return token; // To Do Implement me
    }


}