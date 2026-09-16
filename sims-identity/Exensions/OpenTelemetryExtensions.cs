namespace sims_identity.Extensions;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
public static class OtelExtensions
{
    public static void AddOtel(this WebApplicationBuilder builder, string serviceName)
    {
        IConfigurationSection otlpEndpoint = builder.Configuration.GetSection("OTEL_EXPORTER_OTLP_ENDPOINT");
        string? endpointUrl = otlpEndpoint["url"];
        string? endpointKey = otlpEndpoint["key"];


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

    }
}