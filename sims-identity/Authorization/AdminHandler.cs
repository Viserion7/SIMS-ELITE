namespace sims_identity.Authorization;

using Microsoft.AspNetCore.Authorization;
using sims_identity.Data;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;


public class AdminHandler : AuthorizationHandler<AdminRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminHandler> _logger;

    public AdminHandler(ApplicationDbContext context, ILogger<AdminHandler> logger)
    {
        _context = context;
        _logger = logger;
    }


    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return;

        var isAdmin = await _context.User
            .Where(user => user.id.ToString() == userId)
            .Select(user => user.is_Admin)
            .SingleOrDefaultAsync();

        if (isAdmin)
            context.Succeed(requirement);

    }

}