
namespace sims_identity;

using Scalar.AspNetCore;
using Microsoft.Extensions.Logging;

using sims_identity.Extensions;
using sims_identity.Data;

using sims_identity.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        // Source - https://stackoverflow.com/a/79785013
        // Posted by Kevin Argueta, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-09-17, License - CC BY-SA 4.0

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        builder.Services.AddScoped<AuthService>();
        builder.Logging.AddConsole();
        builder.AddOtel("sims-identity");
        builder.AddEf();
        builder.AddJwt();
        builder.AddAuthorizationPolicies();

        var app = builder.Build();

        //ai Magie 2h selber Probiert ich verstehe es nicht :(
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            Seeder LokalSeeder = new Seeder(context);
            await LokalSeeder.AddCategories();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(options =>
            {
                options.AddPreferredSecuritySchemes(["Bearer"])
                    .AddHttpAuthentication("Bearer", bearer =>
                    {
                        bearer.Token = string.Empty;
                    });
            });
            app.MapOpenApi();
        }


        app.UseHttpsRedirection();

        //Mit Policies vor Authorisazion beabrietet werden!
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
