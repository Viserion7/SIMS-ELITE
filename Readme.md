
# Usefull things
http://localhost:5066/scalar/



# To Do:
[]nur im Def Scalar anzeigen Lassen!


# Für Uns Docs (Kann man später weglöschen!)

dotnet new webapi -n sims -f net10.0 --use-program-main -controllers
dotnet sln sims.slnx add sims/sims.csproj

## Logger Usage daweil nur für mich Recherche LG Yannick
```csharp
//Zentrales Logging (ILogger) an SigNoz anbinden
// nur wenn wirklich var in appsettings.json gesetzt ist, sonst wird kein Logging an SigNoz gesendet
//standart in console ist aber eh immer an wir clearen nicht
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (!string.IsNullOrEmpty(otlpEndpoint))
{
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.SetResourceBuilder(resourceBuilder);
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter();
    });
}
```

appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://<IP_DEINES_SIGNOZ_CONTAINERS>:4317"
}
```

einbinden der libraries in datei wo man es braucht (z.B. Controller)
```csharp
using Microsoft.Extensions.Logging;

// 1. Variable für den Logger anlegen
    private readonly ILogger<SimsController> _logger;

// 2. Logger per Constructor Injection laden
    public SimsController(ILogger<SimsController> logger)
    {
        _logger = logger;
    }
```


beispiel nutzung:
```csharp
var userId = 42;
var action = "Login";

// FALSCH (SigNoz sieht nur einen langen Text, du kannst nicht nach UserId filtern):
_logger.LogInformation($"User {userId} hat {action} ausgeführt.");

// RICHTIG (SigNoz speichert "UserId" und "Action" als extra Felder in z.B. Signoz filter):
_logger.LogInformation("User {UserId} hat {Action} ausgeführt.", userId, action);




////---------------- bei Try Catch kann man auch noch die Exception mitgeben
 try
{
    throw new Exception("Datenbank nicht erreichbar");
}
catch (Exception ex)
{
    var orderId = 99;
    _logger.LogError(ex, "Fehler beim Verarbeiten von Bestellung {OrderId}", orderId);
}
``` 

alle möglichen methoden für logger von .net

| Methode | Wann verwenden? | Beispiel-Szenario |
| --- | --- | --- |
| `LogTrace()` | Feinste Details, oft nur temporär aktiviert | "Starte For-Schleife für Item 4 von 1000." |
| `LogDebug()` | Wertvolle Daten für Entwickler zur Fehlersuche | "Lade User-Profil aus Cache. Cache-Key: X." |
| `LogInformation()` | Wichtige Meilensteine im Geschäftsablauf | "Bestellung 1234 erfolgreich angelegt." |
| `LogWarning()` | Unerwartetes, aber das System läuft weiter | "API liefert Timeout, starte Retry 1/3." |
| `LogError()` | Der aktuelle Request ist fehlgeschlagen | "Datenbankabfrage für User 42 abgebrochen." |
| `LogCritical()` | Systemweiter Absturz oder Notfall | "Haupt-Datenbank nicht erreichbar!" |


### Setup OpenTelemetry in .NET 10
```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```
To Do: einbindung für C# Programm CS noch machen Grundsätzlich ja Optional dahwei wk später erst wenn zeit ist.





# Start

Siehe ./depl-kram ordner, hier befinden sich unterschieldiche docker compose datein welche für unterschiedliche zwekce vorhanden sind. 
nach cd zum ordner, kann man mittel compose den contaienr stack starten.
1. ein compose für die lokale run entwicklung, wo nur die dbs angelegt werden und ports gemapped werden.
2. ein compose wo die images direkt on up gebaut werden für ein locales testen builden
3. ein compose für die produktivumgebung, wo die images direkt vom Github reg. gezogen werden


## Frontend per CLI für dev Starten

```bash
cd ./frontend
npm install
npm run dev
```