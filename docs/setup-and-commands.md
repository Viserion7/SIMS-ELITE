# Setup and Commands

## .NET Project Setup

dotnet new webapi -n sims -f net10.0 --use-program-main -controllers
dotnet sln sims.slnx add sims/sims.csproj

## Setup OpenTelemetry in .NET 10

1. In Program dependencies hinzufügen:

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
```

2. In Projekt die OpenTelemetry Konfiguration hinzufügen, siehe https://signoz.io/blog/opentelemetry-dotnet-logs/
3. die appSettings json anpassen, siehe https://signoz.io/blog/opentelemetry-dotnet-logs/
