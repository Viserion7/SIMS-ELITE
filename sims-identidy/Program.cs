
namespace sims_identidy;

using Scalar.AspNetCore;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        string serviceName = "sims-identidy";
        IConfigurationSection otlpEndpoint = builder.Configuration.GetSection("OTEL_EXPORTER_OTLP_ENDPOINT");
        string? endpointUrl = otlpEndpoint["url"];
        string? endpointKey = otlpEndpoint["key"];

        builder.Logging.AddConsole();
        if (!string.IsNullOrEmpty(endpointUrl))
        {
            //tracing
            builder.Services.AddOpenTelemetry()
                .WithTracing(tpb =>
                {
                    tpb.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddOtlpExporter(otlp =>
                       {
                           otlp.Endpoint = new Uri(endpointUrl);
                           if (!string.IsNullOrEmpty(endpointKey))
                           {
                               otlp.Headers = "signoz-ingestion-key=" + endpointKey;
                           }
                       });
                });



            //logging
            builder.Logging.AddOpenTelemetry(opt =>
            {
                opt.IncludeFormattedMessage = true;
                opt.IncludeScopes = true;
                opt.ParseStateValues = true;
                opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
                opt.AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = new Uri(endpointUrl);
                    if (!string.IsNullOrEmpty(endpointKey))
                    {
                        otlp.Headers = "signoz-ingestion-key=" + endpointKey;
                    }

                });
            });

        }

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
