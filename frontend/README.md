# Frontend Architektur & API Guide

> 📖 **Detaillierter KI- & Entwickler-Leitfaden:** Siehe auch [AI_SYSTEM_GUIDE.md](./AI_SYSTEM_GUIDE.md) für eine vollständige Komponenten-, Flow- und Routing-Dokumentation.

Kompakte Übersicht über Konfiguration, Backend-Kopplung, Auth/Bearer-Token und Ordnerstruktur.

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

## 2. Backend-Routing & Auth Übersicht

Alle Aufrufe aus dem Frontend laufen über relative Pfade (Same-Origin):

| Service | Frontend-Pfad | Lokaler Vite-Proxy (`.env`) | Docker-Netzwerk | Auth / Bearer |
| :--- | :--- | :--- | :--- | :--- |
| **Identity** | `/api/identity/*` | `VITE_IDENTITY_URL` (Port 67) | `http://identity:8080` | **Ja** (JWT Bearer für User, Me, Categories, Assignments) |
| **Aggregator** | `/api/aggregator/*` | `VITE_AGGREGATOR_URL` (Port 88) | `http://aggregator:8080` | Nein (öffentlich / intern) |
| **Incident Manager** | `/api/incident-manager/*` | `VITE_INCIDENT_MANAGER_URL` (Port 420) | `http://incident-manager:8080` | Nein (öffentlich / intern) |
| **STIX Ingest** | `/api/stix/*` | `VITE_STIX_URL` (Port 8080) | `http://sims-stix-ingest:8080` | Nein (öffentlich / intern) |

---

## 3. Bearer-Token & Payload-Regeln

### Automatischer Bearer Token
- Der zentrale Client in `src/api/client.ts` prüft automatisch bei jedem Request, ob ein Token im `useAuthStore` bzw. `localStorage` vorhanden ist.
- Ist ein Token da und `skipAuth !== true`, wird der Header `Authorization: Bearer <accessToken>` automatisch injiziert.
- Bei `/api/v1/Auth/login`, `/api/v1/Auth/refresh` und `/api/v1/User/toNotify` wird `skipAuth: true` gesetzt.

### Wichtige Payload-Eigenheiten der Backends:
- **Login (`POST /api/v1/Auth/login`)**:
  - Request: `{ email: string, password: string }`
  - Response: `{ accessToken: string, refreshToken: string }`
- **STIX Ingest (`PUT /api/v1/Stix`)**:
  - Request: JSON-Objekt mit `"type": "bundle"`, `"id": "bundle--..."` und `"objects": [{ id, type, ... }]`
  - Validierungshelfer: `validateStixBundle(data)` in `src/composables/queries` oder `src/api`
- **Eskalation (`POST /api/v1/Eskalation`)**:
  - Request: `{ incidentId: string, message: string }`
- **User Update (`PUT /api/v1/User/{id}`)**:
  - Request: `{ email?, password?, is_deleted?, is_Admin?, is_ToNotify? }` (Admin Toggle!)

---

## 4. Ordnerstruktur: Types, API & TanStack Queries

### `src/types/` (TypeScript DTOs)
- `auth.ts`, `identity.ts`, `aggregator.ts`, `incidentManager.ts`, `stix.ts`
- Vollständige Typdefinitionen aller Request- und Response-Objekte.

### `src/stores/auth.ts` (Pinia Auth Store)
- Speichert `accessToken`, `refreshToken`, `currentUser` und `userDetails`.
- Berechnet `isAuthenticated` und `isAdmin`.

### `src/api/` (Domain API Services)
Reine HTTP-Funktionen ohne Vue-Reaktivität:
- `identityApi`: `login`, `getMe`, `getUsers`, `updateUser`, `assignLevel`, etc.
- `aggregatorApi`: `getIncidents(page, count)`, `getIncidentById`, `getRelationshipsByIncident`, etc.
- `incidentManagerApi`: `escalate({ incidentId, message })`
- `stixApi`: `uploadStix(bundle)`, `validateStixBundle(bundle)`

### `src/composables/queries/` (TanStack Query Hooks)
Reaktives State-Management mit automatischem Caching und Invalidierung:
- **`queryKeys.ts`**: Zentrale Factory zur Vermeidung von Typo-Strings im Cache.
- **`useAuthQueries.ts`**: `useLoginMutation`, `useCurrentUserQuery`, `useLogoutMutation`
- **`useUserQueries.ts`**: `useUsersQuery`, `useUserDetailsQuery`, `useUpdateUserMutation`, `useAssignLevelMutation`
- **`useIncidentQueries.ts`**: `useIncidentsQuery`, `useIncidentDetailQuery`, `useIncidentRelationshipsQuery`
- **`useEscalationQueries.ts`**: `useEscalateMutation`
- **`useStixQueries.ts`**: `useStixUploadMutation` (invalidiert nach Upload automatisch Incidents-Cache!)

---

## 5. Anwendungsbeispiel in Vue-Komponenten

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useIncidentsQuery, useEscalateMutation } from '@/composables/queries'

const page = ref(1)
const count = ref(20)

// Reaktives Abrufen mit TanStack Query (automatisch gecacht)
const { data: incidents, isLoading, error } = useIncidentsQuery(page, count)

// Eskalations-Mutation
const { mutate: escalate, isPending: isEscalating } = useEscalateMutation()

function handleEscalate(incidentId: string) {
  escalate({
    incidentId,
    message: 'Kritischer Vorfall erfordert sofortige Prüfung!',
  })
}
</script>
```
