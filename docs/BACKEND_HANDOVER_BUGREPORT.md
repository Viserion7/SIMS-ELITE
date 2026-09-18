# Handover & Bugreport für Backend / Datenbank-Zuständige

> **Zusammenfassung:**  
> Dieses Dokument richtet sich an das Team bzw. die Zuständigen für **`sims-aggregator`** . Es beschreibt einen kritischen Punkt im Backend, die das Speichern von Vorfällen betreffen.

---

## 1. Kritischer Bug in `sims-aggregator`: Incidents können nicht angelegt werden (HTTP 500)

### Fehlerbeschreibung

Beim Hochladen eines STIX 2.1 Bundles über den STIX-Ingest (`sims-stix-ingest`) parst dieser die Objekte und sendet jedes Domain-Objekt per HTTP POST an den Aggregator:
`POST http://aggregator:8080/api/v1/Incidents`

Der Aggregator stürzt dabei mit einem **HTTP 500 Internal Server Error** ab.

### Genaue Ursache (aus den Container-Logs)

```text
Npgsql.PostgresException (0x80004005): 42804: column "deleted_by" is of type uuid but expression is of type integer
Hint: You will need to rewrite or cast the expression.
at sims_aggregator.Controllers.IncidentsController.AddIncident(CreateIncidentDto arg_incident) in /src/Controllers/IncidentsController.cs:line 80
```

1. In `sims-aggregator/Models/Incident.cs` wurde das Feld `deleted_by` auf `int?` geändert:
   ```csharp
   public int? deleted_by { get; set; } = null;
   ```
   _(Dies geschah, um zur User-ID aus `sims-identity` zu passen)._
2. In der existierenden PostgreSQL-Datenbank `aggregator` (Tabelle `public.Incidents`) ist die Spalte `deleted_by` jedoch noch als **`uuid`** definiert:
   ```sql
   Column     | Type
   -----------+--------------------------
   deleted_by | uuid
   ```
3. Weil Entity Framework Core beim `AddIncident` nun versucht, einen Integer-Wert (oder einen als integer typisierten Parameter) in eine `uuid`-Spalte einzufügen, bricht PostgreSQL mit Fehler `42804` ab.
4. **Folge:**
   - Keines der Incidents wird in der Datenbank gespeichert.
   - `sims-stix-ingest` meldet: `incidentsCreated: 0`.
   - Die nachfolgenden `relationship`-Objekte scheitern mit HTTP 404, da die referenzierten Incidents nicht existieren.

### Was muss das Aggregator-Team tun?

Es gibt zwei Möglichkeiten, das zu beheben:

#### Option A: Spaltentyp in PostgreSQL anpassen (Sofortlösung)

In der Datenbank `aggregator` auf dem Postgres-Server ausführen:

```sql
ALTER TABLE "Incidents" ALTER COLUMN deleted_by TYPE integer USING NULL;
```

_(Hinweis: Befindet sich der Aggregator in Docker: `docker exec -it <dbAggregator-container> psql -U postgres -d aggregator -c "ALTER TABLE \"Incidents\" ALTER COLUMN deleted_by TYPE integer USING NULL;"`)_

#### Option B: Saubere EF Core Migration erstellen

Im Projekt `sims-aggregator`:

1. Neue Migration anlegen:
   ```bash
   dotnet ef migrations add ChangeDeletedByToInteger
   ```
2. In `sims-aggregator/Program.cs` sicherstellen, dass Migrationen beim Start automatisch angewendet werden:
   ```csharp
   // Statt EnsureCreated() (das bestehende Tabellen nicht aktualisiert):
   db.Database.Migrate();
   ```
