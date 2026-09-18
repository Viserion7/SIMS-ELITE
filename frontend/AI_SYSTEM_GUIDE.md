# SIMS-ELITE Frontend – AI & Developer System Guide

> **Zweck dieses Dokuments:**  
> Dieses Dokument dient als Referenz und Leitfaden für KI-Assistenten und Entwickler, die an diesem Frontend arbeiten. Es beschreibt die genaue Architektur, den Datenfluss, alle implementierten Seiten und Features, die Verzeichnisstruktur sowie Best Practices und Stolperfallen.

---

## 1. Tech-Stack & Kerntechnologien

| Technologie | Version | Verwendung & Rolle |
| :--- | :--- | :--- |
| **Vue 3** | `^3.5.40` | Composition API mit `<script setup lang="ts">`. Keine Options-API. |
| **TypeScript** | `~6.0.0` | Strikte Typisierung aller DTOs, Komponenten-Props und API-Responses. |
| **Vite** | `^8.1.5` | Bundler & Dev-Server mit integriertem Reverse-Proxy für lokale Microservices. |
| **TanStack Vue Query** | `^5.102.8` | Server-State-Management, automatisches Caching, Refetching & Cache-Invalidierung. |
| **Pinia** | `^4.0.2` | Client-State: Auth-Status, JWT-Tokens, angemeldeter User. |
| **PrimeVue** | `4.5.5` | UI-Komponentenbibliothek (**100% freie MIT-Lizenz**, kein Lizenzbanner). |
| **@primevue/themes** | `^4.5.4` | Aura Theme Preset (MIT-lizenziert). |
| **PrimeIcons** | `^8.0.1` | Icon-Set (`pi pi-...`). |
| **Vue Router** | `^5.2.0` | Client-seitiges Routing mit Layout-System und Auth-Guards. |

> [!IMPORTANT]
> **PrimeVue Version 4.x (MIT) vs. 5.x (Commercial):**  
> PrimeVue 5.x und `@primeuix/themes` erzwingen ein proprietäres Lizenzprüfungs-Banner (`Invalid PrimeUI License`).  
> In diesem Projekt **muss** zwingend **PrimeVue 4.5.x** und **`@primevue/themes`** verwendet werden! Themes werden stets importiert als:  
> `import Aura from '@primevue/themes/aura'`

---

## 2. Gesamtarchitektur & Datenfluss

### 2.1 Backend-Microservices & Proxy-Routing
Das Frontend kommuniziert mit 4 verschiedenen Backend-Diensten. Um CORS-Probleme zu vermeiden und portable Docker-Deployments zu ermöglichen, erfolgen alle API-Aufrufe über **relative Same-Origin-Pfade** (`/api/<service>/*`):

```
                                  ┌───────────────────────────────┐
                                  │   Browser / Vue Frontend      │
                                  │   (Same-Origin: /api/...)     │
                                  └──────────────┬────────────────┘
                                                 │
                  ┌──────────────────────────────┴──────────────────────────────┐
                  ▼                                                             ▼
    Lokale Entwicklung (npm run dev)                              Produktion / Docker Container
    Vite Dev-Proxy (vite.config.ts)                               Nginx Reverse Proxy (nginx.conf)
                  │                                                             │
   ┌──────────────┼──────────────┬──────────────┐                ┌──────────────┼──────────────┬──────────────┐
   ▼              ▼              ▼              ▼                ▼              ▼              ▼              ▼
Identity     Aggregator      Escalator     STIX Ingest        Identity     Aggregator      Escalator     STIX Ingest
(Port 67)    (Port 88)       (Port 420)    (Port 8080)        (:8080)      (:8080)         (:8080)       (:8080)
```

1. **Identity Service** (`/api/identity/*`): Authentifizierung, Benutzerverwaltung, Eskalationsstufen/Kategorien.
2. **Aggregator Service** (`/api/aggregator/*`): Incidents (Tickets), Relationen, Details.
3. **Escalator / Incident Manager** (`/api/incident-manager/*`): Eskalieren von Vorfällen.
4. **STIX Ingest Service** (`/api/stix/*`): Upload von STIX 2.1 JSON-Bundles.

### 2.2 Standardisierter Request-Lifecycle
Für saubere Trennung von Belangen (*Separation of Concerns*) gilt immer folgender Ablauf:

```
[Vue Komponente] (z. B. MyIncidentsView.vue)
       │  ruft auf
       ▼
[TanStack Query Hook] (src/composables/queries/useIncidentQueries.ts)
       │  verwaltet Cache, Ladezustand (isLoading), Fehler (error) & queryKeys
       ▼
[Domain API Service] (src/api/aggregatorApi.ts)
       │  reine async/await Funktionen, kein Vue-State
       ▼
[Base HTTP Client] (src/api/client.ts)
       │  injiziert Bearer Token aus useAuthStore(), JSON-Parsing, Fehlerbehandlung
       ▼
[Backend Microservice via Proxy]
```

---

## 3. Vollständige Verzeichnisstruktur (`src/`)

```text
src/
├── api/                             # Reine API-Aufrufe (Fetch-Funktionen, kein UI-State)
│   ├── client.ts                    # Zentraler Fetch-Client (Token-Injection, 401-Handling)
│   ├── identityApi.ts               # Login, User, Me, Categories/Assignments
│   ├── aggregatorApi.ts             # Incidents-Liste, Details, Relationships
│   ├── incidentManagerApi.ts        # Vorfall eskalieren
│   └── stixApi.ts                   # STIX-Upload & Bundle-Validierung
├── assets/
│   └── main.css                     # Globale CSS-Variablen, Reset & Layout-Klassen
├── components/                      # Wiederverwendbare UI-Komponenten
│   ├── EmptyState.vue               # Platzhalter für leere Listen/Tabellen
│   ├── IncidentCard.vue             # Ticket-Karte mit Badges, Eskalation & Navigation
│   └── PageHeader.vue               # Einheitlicher Kopfbereich für Pages
├── composables/
│   ├── queries/                     # TanStack Vue Query Hooks (Server State)
│   │   ├── index.ts                 # Zentraler Export aller Query Hooks
│   │   ├── queryKeys.ts             # Zentrale Factory für Cache-Keys
│   │   ├── useAuthQueries.ts        # Login, Logout, CurrentUser
│   │   ├── useUserQueries.ts        # Users CRUD, Soft-Delete, Restore
│   │   ├── useIncidentQueries.ts    # Incidents List, Detail, Relationships
│   │   ├── useEscalationQueries.ts  # Eskalation ausführen
│   │   └── useStixQueries.ts        # STIX Upload (invalidiert Incident-Cache)
│   └── useMyCategories.ts          # Ermittelt Incident-Kategorien des eingeloggten Users
├── config/
│   └── index.ts                     # Runtime-Config (window.APP_CONFIG mit Env-Fallbacks)
├── layouts/                         # Layout-Templates
│   ├── AppLayout.vue                # Standard-Layout: Feste Sidebar links, Header, User-Footer
│   └── AuthLayout.vue               # Minimales Fullscreen-Layout (Login, STIX-Upload)
├── router/
│   └── index.ts                     # Routing-Tabelle, Auth-Guards & dynamisches Layout
├── stores/
│   └── auth.ts                      # Pinia Auth Store (Token, User-Info, Admin-Rolle)
├── types/                           # TypeScript DTOs (Request/Response Typen)
│   ├── auth.ts
│   ├── identity.ts
│   ├── aggregator.ts
│   ├── incidentManager.ts
│   └── stix.ts
├── views/                           # Seiten / Pages
│   ├── LoginView.vue                # Fullscreen Login
│   ├── MyIncidentsView.vue          # Eigene Incidents (gefiltert nach User-Level)
│   ├── AllIncidentsView.vue         # Alle Incidents (globale Übersicht)
│   ├── IncidentDetailView.vue       # Detailansicht mit Graph/Relationen & Eskalation
│   ├── StixUploadView.vue           # STIX 2.1 JSON Upload (auch ohne Login nutzbar)
│   └── admin/
│       ├── AdminUsersView.vue       # Benutzerverwaltung (Toggles, Soft-Delete/Restore)
│       └── AdminAssignmentsView.vue # Zuweisung von Eskalationsstufen zu Benutzern
├── App.vue                          # Root-Komponente mit dynamischem <component :is="layout">
└── main.ts                          # App-Initialisierung (PrimeVue, TanStack Query, Pinia, Router)
```

---

## 4. Implementierte Seiten & Funktionalitäten

### 4.1 Login (`/login`)
* **Layout:** `AuthLayout` (Zentriert, minimalistisch).
* **State:** Pinia `useAuthStore` speichert `accessToken`, `refreshToken` und `currentUser`.
* **Guard:** Nach erfolgreichem Login wird der User automatisch auf `/incidents` weitergeleitet.

### 4.2 STIX 2.1 Bundle Upload (`/upload`)
* **Besonderheit:** Kann **sowohl unangemeldet (für externe Partner/Sensor-Systeme) als auch angemeldet** genutzt werden.
* **Layout:** `AuthLayout` (Fullscreen). Wenn der Benutzer angemeldet ist, wird ein „Zurück zum Dashboard“-Button angezeigt.
* **Validierung:** `validateStixBundle(json)` prüft vor dem Senden:
  * `type === 'bundle'`
  * Gültige STIX 2.1 `id` (Muster `bundle--...`)
  * Vorhandensein eines nicht-leeren `objects`-Arrays.
* **Cache-Invalidierung:** Nach erfolgreichem Upload invalidiert `useStixUploadMutation` automatisch die Query-Keys `['incidents']`, sodass neue Vorfälle sofort im Dashboard sichtbar sind.

### 4.3 Meine Incidents (`/incidents`) & Filterung
* **Fachliche Logik:** Ein normaler Benutzer sieht initial nur Vorfälle, die seinen zugewiesenen Eskalationsstufen/Kategorien entsprechen.
* **Ablauf:**
  1. `useCurrentUserQuery` lädt die Details des angemeldeten Benutzers.
  2. `useMyCategories` ermittelt die zugeordneten Kategorien (`category_name` bzw. `level`).
  3. Incidents werden anhand dieser Kategorien gefiltert.
* **Pagination:** Enthält einen Button „Weitere laden“, um paginiert zusätzliche Incidents abzurufen.

### 4.4 Alle Incidents (`/incidents/all`)
* Globale Tabelle aller Vorfälle mit Filter nach Schweregrad/Level, Suchleiste und Schnellaktionen (Ansehen, Eskalieren).

### 4.5 Incident Detailansicht (`/incidents/:id`)
* **Stammdaten:** Titel, Beschreibung, Schweregrad, Zeitstempel.
* **Beziehungen / Graph:** Listet verwandte Indikatoren, Malware und Relationen aus dem Aggregator (`/api/v1/Relationships/byIncident/{id}`).
* **Eskalation:** Dialog zur direkten Eskalation des Vorfalls (`useEscalateMutation`).

### 4.6 Admin: Benutzerverwaltung (`/admin/users`)
* **Zugriff:** Geschützt durch `authStore.isAdmin`.
* **Benutzer anlegen:** Über den Button „Neuer Benutzer“ im Header öffnet sich ein Dialog (`POST /api/v1/User`), der neue Accounts mit E-Mail und initialem Passwort anlegt.
* **Benutzer bearbeiten (E-Mail & Passwort):** Über das Stift-Icon in der Spalte „Aktionen“ öffnet sich ein Dialog (`PUT /api/v1/User/{id}`), mit dem E-Mail-Adresse und/oder ein neues Passwort als Administrator aktualisiert werden können.
* **DataTable Sortierung:** Besitzt `sortField="id" :sortOrder="1"`. **Wichtig:** Dadurch springen die Tabellenzeilen nicht, wenn Toggles betätigt werden und TanStack Query im Hintergrund refetcht.
* **Rollen-Toggles:** Direktes Ändern von `is_Admin` und `is_ToNotify`.
* **Soft-Delete & Wiederherstellung:**
  * Löschen: Sendet `DELETE /api/v1/Users/{id}` (setzt im C#-Backend `is_deleted = true`).
  * Wiederherstellen: Sendet `PUT /api/v1/Users/{id}` mit `{ is_deleted: false }`.
  * Ein Status-Badge zeigt sofort an, ob ein Nutzer aktiv oder gelöscht ist.

### 4.7 Admin: Eskalationsstufen (`/admin/assignments`)
* Zuweisen und Entziehen von Eskalationsstufen (Low, Medium, High sowie dynamische Datenbank-Level) pro Benutzer über `/api/v1/Categories/assignLevel`.

---

## 5. Layout-System & Routing-Regeln

In `App.vue` wird das aktive Layout dynamisch gerendert:
```vue
<template>
  <component :is="layout">
    <router-view />
  </component>
  <Toast />
  <ConfirmDialog />
</template>
```

Im Router (`src/router/index.ts`) **muss** jede Route explizit ihr Layout deklarieren:
* **`AppLayout`**: Für geschützte Dashboard-Seiten (Sidebar, User-Info, Menüs).
* **`AuthLayout`**: Für Standalone-Vollbildseiten (`/login`, `/upload`).

```typescript
{
  path: '/upload',
  name: 'stix-upload',
  component: () => import('@/views/StixUploadView.vue'),
  meta: { requiresAuth: false, layout: AuthLayout }, // Explizit AuthLayout!
}
```

---

## 6. Best Practices & Do's and Don'ts für KIs

### ✅ DO:
1. **Verwende TanStack Query für alle Server-Daten:** Immer Hooks in `src/composables/queries/` anlegen oder nutzen.
2. **Definiere Query-Keys in `queryKeys.ts`:** Keine manuellen String-Arrays in Komponenten verstreuen.
3. **Setze `sortField` bei PrimeVue DataTables:** Verhindert Zeilenspringen bei reaktiven Updates.
4. **Typisiere jede API-Antwort:** Lege neue DTOs in `src/types/` ab.
5. **Nutze PrimeVue UI-Tokens:** Farben über CSS-Variablen steuern (`var(--primary-color)`, `var(--surface-color)`, `var(--text-color)`).

### ❌ DON'T:
1. **Kein `primevue@5` oder `@primeuix/themes` installieren:** Führt zu Lizenz-Banner-Fehlern (`Invalid PrimeUI License`). Stets `primevue@^4.5.5` und `@primevue/themes@^4.5.4` beibehalten.
2. **Keine Direktaufrufe von `fetch()` in Vue-Komponenten:** Immer über `src/api/` und `src/composables/queries/`.
3. **Keine hardcodierten URLs:** Keine `http://localhost:8080` Strings im Code. Immer relative Pfade `/api/...` nutzen, die über `src/config/` und Proxys geroutet werden.
4. **Keine Passwörter in Logs oder Stores halten:** Nur Tokens im Auth-Store persistieren.
