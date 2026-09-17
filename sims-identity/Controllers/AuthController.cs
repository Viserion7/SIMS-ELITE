//inspiration: https://medium.com/@aschultzme/introduction-to-entity-framework-building-a-simple-crud-api-in-net-b22eb2efbe6e
using Microsoft.AspNetCore.Mvc;
using sims_identity.Dtos;

namespace sims_identity.Controllers;

using Microsoft.Extensions.Logging;
using sims_identity.Data;
using sims_identity.Services;
using Microsoft.AspNetCore.Authorization;


[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    private readonly AuthService _authService;

    public AuthController(ApplicationDbContext context, ILogger<AuthController> logger, AuthService authService)
    {
        _context = context;
        _logger = logger;
        _authService = authService;
    }


    [HttpPost("login")]
    [EndpointDescription("Meldet einen Benutzer an.")]
    public async Task<ActionResult<User>> LoginUser(CreateUserDto user)
    {
        var response = await _authService.LoginAsync(user);
        if (response == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        return Ok(response);
    }



    [HttpGet("me")]
    [Authorize]
    [EndpointDescription("Gibt den aktuell angemeldeten Benutzer zurück.")]
    public async Task<ActionResult<UserAuthorized>> GetMe()
    {
        var userId = Int32.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);
        var useremail = User.FindFirst("name")?.Value;

        UserAuthorized user = new UserAuthorized
        {
            id = userId,
            email = useremail,
            authenticated = true

        };

        return Ok(user);
    }

    [HttpPost("refresh")]
    [EndpointDescription("Erstellt ein neues Zugriffstoken.")]
    public async Task<ActionResult<TokenResponse>> RefreshMe([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (response == null)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        return Ok(response);
    }



    [HttpPost("logout")]
    [Authorize]
    [EndpointDescription("Revoken den RefreshToken")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var success = await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        if (!success)
        {
            return BadRequest("Invalid or already revoked refresh token.");
        }

        return Ok("Refresh token revoked.");
    }

}