# Logger Usage (Recherche LG Yannick)

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

| Methode            | Wann verwenden?                                | Beispiel-Szenario                           |
| ------------------ | ---------------------------------------------- | ------------------------------------------- |
| `LogTrace()`       | Feinste Details, oft nur temporär aktiviert    | "Starte For-Schleife für Item 4 von 1000."  |
| `LogDebug()`       | Wertvolle Daten für Entwickler zur Fehlersuche | "Lade User-Profil aus Cache. Cache-Key: X." |
| `LogInformation()` | Wichtige Meilensteine im Geschäftsablauf       | "Bestellung 1234 erfolgreich angelegt."     |
| `LogWarning()`     | Unerwartetes, aber das System läuft weiter     | "API liefert Timeout, starte Retry 1/3."    |
| `LogError()`       | Der aktuelle Request ist fehlgeschlagen        | "Datenbankabfrage für User 42 abgebrochen." |
| `LogCritical()`    | Systemweiter Absturz oder Notfall              | "Haupt-Datenbank nicht erreichbar!"         |
