
namespace sims_identity;

using Scalar.AspNetCore;
using Microsoft.Extensions.Logging;

using sims_identity.Extensions;
using sims_identity.Data;


public class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Logging.AddConsole();
        builder.AddOtel("sims-identity");
        builder.AddEf();

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
            app.MapScalarApiReference();
            app.MapOpenApi();
        }


        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
