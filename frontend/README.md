# Frontend Architektur & API Guide

Kompakte Übersicht über Konfiguration, Backend-Kopplung und Ordnerstruktur.

---

## 1. Konfiguration & Backend-Zugriff

### Wie wird die Konfiguration geladen?
- **Laufzeit-Config (`window.APP_CONFIG`)**:
  - `index.html` lädt `/config.js` synchron vor der App (`main.ts`).
  - `src/config/index.ts` liest `window.APP_CONFIG` (Fallback auf `import.meta.env` oder Default-Routen `/api/*`).
  - **Vorteil**: Ein einziges Docker-Image funktioniert in jeder Umgebung ohne Re-Build.

### Lokale Entwicklung (`npm run dev`)
- **Config**: `public/config.js` liefert relative Standard-Pfade (`/api/...`).
- **Proxy**: Vite Dev-Server (`vite.config.ts`) fängt `/api/*` ab:
  - Liest Ziel-URLs aus `.env` (siehe `.env.example`).
  - Schneidet den Präfix `/api/<service>` ab (`rewrite`).
  - Leitet an lokale Backend-Ports weiter (keine CORS-Probleme).

### Docker / Production
- **Config**: `docker-entrypoint.sh` generiert `/config.js` dynamisch zur Container-Laufzeit mit `PUBLIC_*_URL`.
- **Proxy**: Nginx (`nginx.conf.template`) dient als Reverse-Proxy:
  - Fängt `/api/<service>/` ab.
  - Leitet intern an Docker-Services weiter (`IDENTITY_URL`, `AGGREGATOR_URL`, etc.).

---

## 2. Backend-Routing Übersicht

Alle Aufrufe aus dem Frontend laufen über relative Pfade (Same-Origin):

| Service | Frontend-Pfad | Lokaler Vite-Proxy (`.env`) | Docker-Netzwerk (Nginx Upstream) |
| :--- | :--- | :--- | :--- |
| **Identity** | `/api/identity/*` | `VITE_IDENTITY_URL` (Port 67) | `http://identity:8080` |
| **Aggregator** | `/api/aggregator/*` | `VITE_AGGREGATOR_URL` (Port 88) | `http://aggregator:8080` |
| **Incident Manager** | `/api/incident-manager/*` | `VITE_INCIDENT_MANAGER_URL` (Port 420) | `http://incident-manager:8080` |
| **STIX Ingest** | `/api/stix/*` | `VITE_STIX_URL` (Port 8080) | `http://sims-stix-ingest:8080` |

---

## 3. Ordnerstruktur: `src/api` vs. `src/composables/queries`

### `src/api/` (HTTP-Client Layer)
- **Zweck**: Reine HTTP-Kommunikation (`fetch`) mit den Microservices.
- **Inhalt (`src/api/client.ts`)**:
  - `apiClient(endpoint, options, service)`: Zentraler Fetch-Wrapper (JSON-Handling, Error-Handling, Auth-Header).
  - Vorkonfigurierte Service-Clients:
    - `identityApiClient(...)`
    - `aggregatorApiClient(...)`
    - `incidentManagerApiClient(...)`
    - `stixApiClient(...)`
- **Umgang**: Keine UI-Logik oder State-Handling hier; nur API-Aufrufe und DTO-Typen.

### `src/composables/queries/` (Query- & State Layer)
- **Zweck**: Server-State-Management mit `@tanstack/vue-query` (Caching, Loading/Error States, Refetching).
- **Inhalt**: Vue-Composables (`use...Query`, `use...Mutation`).
- **Umgang / Best Practice**:
  - Für jede Backend-Ressource ein Composable erstellen (z. B. `useIncidentsQuery.ts`).
  - In `queryFn` den passenden Client aus `src/api/client.ts` aufrufen:
    ```ts
    import { useQuery } from '@tanstack/vue-query'
    import { aggregatorApiClient } from '@/api/client'

    export function useExampleQuery() {
      return useQuery({
        queryKey: ['example'],
        queryFn: () => aggregatorApiClient('/example'), // ruft /api/aggregator/example auf
      })
    }
    ```
  - In Vue-Komponenten ausschließlich Composables importieren:
    ```ts
    const { data, isLoading, error } = useExampleQuery()
    ```
- **Regel**: Komponenten rufen niemals `fetch` oder `apiClient` direkt auf, sondern nutzen immer die Composables.

---

## 4. Kurzanleitung: Neuen Endpunkt anbinden

1. **Service identifizieren**: Welches Backend wird benötigt (z. B. Aggregator)?
2. **Query anlegen**: Datei in `src/composables/queries/use<Name>Query.ts` erstellen.
3. **Client aufrufen**: Den passenden Client (`aggregatorApiClient`, etc.) mit dem relativen Endpunkt ohne Service-Präfix aufrufen.
4. **Verwenden**: Composable in der Vue-Komponente per `use...Query()` einbinden.
