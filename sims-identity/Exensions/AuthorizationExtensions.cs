namespace sims_identity.Extensions;

using Microsoft.AspNetCore.Authorization;
using sims_identity.Authorization;



public static class AuthorizationExtensions
{
    public static void AddAuthorizationPolicies(this WebApplicationBuilder builder)
    {
        //https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0&tabs=swagger-ui
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("isAdmin", policy =>
                policy.Requirements.Add(new AdminRequirement()));

        builder.Services.AddScoped<IAuthorizationHandler, AdminHandler>();
        //Wenn ASP.NET einen IAuthorizationHandler braucht, verwende dafür AdminHandler.

    }
}