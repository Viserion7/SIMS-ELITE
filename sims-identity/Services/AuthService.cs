using sims_identity.Data;
using sims_identity.Dtos;

using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace sims_identity.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<TokenResponse?> LoginAsync(CreateUserDto request)
    {
        var user = await _context.User
            .FirstOrDefaultAsync(u => u.email == request.email);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(request.password, user.password_hash))
        {
            return null; // Invalid credentials
        }

        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.id.ToString())
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenExpirationMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(User user)
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.id,
            Expires = DateTime.UtcNow.AddDays(
                double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!))
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<TokenResponse?> RefreshTokenAsync(string refreshToken)
{
    var token = await _context.RefreshTokens
        .FirstOrDefaultAsync(t => t.Token == refreshToken);

    if (token == null || token.IsRevoked || token.Expires < DateTime.UtcNow)
    {
        return null; // Invalid or expired token
    }

    var user = await _context.User.FindAsync(token.UserId);
    if (user == null)
    {
        return null; // User not found
    }

    // Generate new access token
    var newAccessToken = GenerateAccessToken(user);

    // Generate new refresh token and revoke the old one
    token.IsRevoked = true;
    var newRefreshToken = await GenerateRefreshTokenAsync(user);

    await _context.SaveChangesAsync();

    return new TokenResponse
    {
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken.Token
    };
}
public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
{
    var token = await _context.RefreshTokens
        .FirstOrDefaultAsync(t => t.Token == refreshToken);

    if (token == null || token.IsRevoked)
    {
        return false;
    }

    token.IsRevoked = true;
    await _context.SaveChangesAsync();
    return true;
}
}