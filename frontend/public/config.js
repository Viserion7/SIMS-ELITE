// Standardkonfiguration für lokale Entwicklung (Vite Dev Server)
// Im Docker-Container wird diese Datei zur Laufzeit durch docker-entrypoint.sh überschrieben.
window.APP_CONFIG = {
  identityUrl: '/api/identity',
  aggregatorUrl: '/api/aggregator',
  incidentManagerUrl: '/api/incident-manager',
  stixUrl: '/api/stix',
};
