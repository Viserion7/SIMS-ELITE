# Systemarchitektur & Komponenten

Überblick über die Services, Datenflüsse, Observability und Security-Pipeline (Dependency-Track).

---

## 🏗️ Gesamtsystem (Mermaid)

```mermaid
flowchart TB
    subgraph Client ["Client / Browser"]
        FE["Vue 3 Frontend<br/>(Vite + PrimeVue + Pinia)"]
    end

    subgraph Gateway ["Reverse Proxy / Routing"]
        NGINX["Nginx Proxy (Docker: Port 2000)<br/><i>bzw. Vite Dev Proxy (Port 5173)</i>"]
    end

    subgraph Backend ["Backend Microservices (.NET 10)"]
        ID["sims-identity<br/>(Port 67 / :8080)<br/>JWT, User, Levels, Auth"]
        AGGR["sims-aggregator<br/>(Port 88 / :8080)<br/>Incidents & Relationen"]
        INC["sims-incidentManager<br/>(Port 420 / :8080)<br/>Eskalation & Alerts"]
        STIX["sims-stix-ingest<br/>(Port 8080)<br/>STIX 2.1 Threat Intel"]
    end

    subgraph Databases ["Datenbanken"]
        DB_ID[("PostgreSQL 16<br/>(Port 5433)<br/>DB: identity")]
        DB_AGGR[("PostgreSQL 16<br/>(Port 5434)<br/>DB: aggregator")]
        DB_STIX[("MongoDB 8.0<br/>(Port 27017)<br/>DB: stix_db")]
    end

    subgraph Observability ["Monitoring & Tracing"]
        SIGNOZ["SigNoz Cloud / OTLP<br/>(OpenTelemetry Traces & Logs)"]
    end

    subgraph External ["Externe Dienste"]
        SMTP["SMTP Mail Server<br/>(Eskalations-Mails)"]
        TAXII["Externe STIX/TAXII Feeds"]
    end

    subgraph Security_CICD ["CI/CD & Supply Chain Security"]
        GHA["GitHub Actions CI<br/>(Build & Test)"]
        CDX["CycloneDX CLI<br/>(SBOM Generierung)"]
        DTRACK["Dependency-Track<br/>(Port 6780 / 6781)<br/>CVE & Vulnerability Scanner"]
        GHCR["GitHub Container Registry<br/>(ghcr.io Images)"]
    end

    %% Client Routing
    FE --> NGINX
    NGINX -->|/api/identity/*| ID
    NGINX -->|/api/aggregator/*| AGGR
    NGINX -->|/api/incident-manager/*| INC
    NGINX -->|/api/stix/*| STIX

    %% Service to DB
    ID --> DB_ID
    AGGR --> DB_AGGR
    STIX --> DB_STIX

    %% Inter-Service Communication
    INC -->|GET /api/v1/User/toNotify| ID
    INC -->|Sende Alert| SMTP
    STIX -->|Push Incidents POST /api/v1/Incidents| AGGR
    TAXII -->|Upload Bundles| STIX

    %% Telemetry
    ID -.->|OTLP| SIGNOZ
    AGGR -.->|OTLP| SIGNOZ
    INC -.->|OTLP| SIGNOZ
    STIX -.->|OTLP| SIGNOZ

    %% CI/CD & Dependency Track Flow
    GHA -->|Push Images| GHCR
    GHA -->|dotnet-CycloneDX| CDX
    CDX -->|Upload SBOM bom-*.json| DTRACK
```

---

## 🧩 Services im Detail

### 1. `frontend` (Vue 3 + Vite)
- **Tech**: Vue 3, Vite, PrimeVue, Pinia, Vis-Network (Graphen-Visualisierung)
- **Dev-Modus**: Vite Dev-Server proxyed `/api/*` direkt auf die lokalen Backend-Ports
- **Docker-Modus**: Nginx serviert die statischen SPA-Dateien und fungiert als Reverse Proxy zu den Backend-Containern

### 2. `sims-identity` (.NET 10)
- **DB**: PostgreSQL (`identity`)
- **Aufgabe**: 
  - Registrierung, Login, JWT Access- & Refresh-Tokens
  - Rollen & Berechtigungsklassen (User, Level, Category)
  - Admin-Check & Liste der Notfall-Kontakte (`toNotify`)

### 3. `sims-aggregator` (.NET 10)
- **DB**: PostgreSQL (`aggregator`)
- **Aufgabe**:
  - Zentraler Daten-Hub für Security-Incidents
  - Speichert Incidents, Quelle (`taxii`, `raw json`) und Verknüpfungen (`Relationship`)
  - Soft-Delete mit Audit-Trail (`deleted_by`)

### 4. `sims-stix-ingest` (.NET 10)
- **DB**: MongoDB 8.0 (`stix_db`)
- **Aufgabe**:
  - Ingest von STIX 2.1 Threat-Intelligence JSON-Bundles
  - Speicherung der Rohdaten / Domain Objects in MongoDB
  - Normalisierung & Weiterleitung relevanter Incidents an `sims-aggregator`

### 5. `sims-incidentManager` (.NET 10)
- **DB**: Keine eigene DB (stateless)
- **Aufgabe**:
  - Eskalations-Management bei kritischen Incidents
  - Ruft `sims-identity` auf, um E-Mail-Adressen von berechtigten Admins abzufragen
  - Versendet Benachrichtigungen per SMTP mit Incident-Details

---

## 🛡️ CI/CD & Dependency-Track (Supply Chain Security)

```mermaid
sequenceDiagram
    autonumber
    actor Dev as Entwickler
    participant GH as GitHub Actions
    participant CDX as CycloneDX CLI
    participant DT as Dependency-Track Server
    participant GHCR as GitHub Container Registry

    Dev->>GH: Release / Tag pushen
    GH->>GHCR: Docker Images bauen & pushen
    GH->>CDX: dotnet-CycloneDX ausführen
    CDX-->>GH: SBOMs generieren (bom-*.json)
    GH->>DT: PUT /api/v1/bom (Upload SBOM via API-Key)
    DT->>DT: Abgleich gegen NVD & GitHub Security Advisories
    Note over DT: Alarmierung bei bekannten CVEs & veralteten Packages
    GH->>GH: SBOM als GitHub Release Asset anhängen
```

- **SBOM Generierung**: Für jeden Service wird mit `dotnet-CycloneDX` eine standardisierte Software Bill of Materials generiert.
- **Dependency-Track**: Eigenständiger Container-Stack (`apiserver`, `frontend`, `postgres`), der jede Komponente auf bekannte Schwachstellen (CVEs) und Lizenzrisiken scannt.
- **Release-Artefakte**: SBOMs werden automatisch an jedes GitHub Release angehängt.

---

## 📊 Observability (SigNoz)

- Alle 4 .NET Services sind via OpenTelemetry SDK instrumentiert
- **Metriken & Tracing**: Automatische HTTP- und ASP.NET Core-Instrumentation
- **Logging**: Structured Logging mit Kontext (z.B. `{UserId}`, `{OrderId}`), direkt gestreamt an den SigNoz OTLP Collector
