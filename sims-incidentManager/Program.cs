
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

namespace sims_incidentManager;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // setup openTelemetry
        var serviceName_openTelemetry = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "sims-incidentManager";
        var endpoint_opentelemetry = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";
        var key_opentelemetry = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT__KEY");

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



        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddHttpClient(); // Http Client injecten
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

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
