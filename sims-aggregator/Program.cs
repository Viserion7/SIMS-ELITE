
namespace sims_aggregator;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using sims_aggregator.Data;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // setup openTelemetry
        var serviceName_openTelemetry = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "sims-aggregator";
        var endpoint_opentelemetry = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";
        var key_opentelemetry = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        builder.Services.AddOpenTelemetry() // setup OpenTelemetry tracing
        .WithTracing(tpb =>
        {
            tpb.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName_openTelemetry)) // Service name used in dashboard
               .AddAspNetCoreInstrumentation() // Auto-instrument ASP.NET Core middleware
               .AddHttpClientInstrumentation() // Auto-instrument HTTP client requests
               .AddOtlpExporter(otlp =>
               {
                   otlp.Endpoint = new Uri(endpoint_opentelemetry); // Replace with SigNoz OTLP endpoint
                   otlp.Headers = $"signoz-ingestion-key={key_opentelemetry}";
               });
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(); // see Logs also in the Console
        builder.Logging.AddOpenTelemetry(opt => // setup OpenTelemetry logging
        {
            opt.IncludeFormattedMessage = true; // Include the formatted log message
            opt.IncludeScopes = true; // Include scope information
            opt.ParseStateValues = true; // Enable structured log parsing
            opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName_openTelemetry));
            opt.AddOtlpExporter(otlp =>
            {
                otlp.Endpoint = new Uri(endpoint_opentelemetry);
                otlp.Headers = $"signoz-ingestion-key={key_opentelemetry}";
            });
        });

        // HTTP aufsetzen
        builder.Services.AddDbContext<dbContext>(); // damit in Controller auf DB Context zugreifen können
        // Add services to the container.
        builder.Services.AddControllers(); // In Controller/ gibt es Klassen das sind unsere ControllerKlassen
        builder.Services.AddHttpClient(); // Http Client injecten
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi(); // API Configs erstellen

        var app = builder.Build(); // Web app erstellen

        // Erstellt automatisch alle fehlenden Tabellen in Postgres!
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<dbContext>();
            db.Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) // wenn in Entwiklungsumgebung aktiviere Scalar helper
        {
            app.MapScalarApiReference();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection(); // http:// (302 fwd req)-> https://

        app.UseAuthorization(); // middleware -> checkt ob user passt

        app.MapControllers(); // aktiviert routing

        app.Run(); // startet
    }
}
