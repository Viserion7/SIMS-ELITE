#!/bin/sh
set -e

# Interne Service-URLs im Docker-Netzwerk (Upstream für Nginx Reverse-Proxy)
export IDENTITY_URL="${IDENTITY_URL:-http://identity:8080}"
export AGGREGATOR_URL="${AGGREGATOR_URL:-http://aggregator:8080}"
export INCIDENT_MANAGER_URL="${INCIDENT_MANAGER_URL:-http://incident-manager:8080}"
export STIX_URL="${STIX_URL:-http://sims-stix-ingest:8080}"

# Öffentliche Pfade für den Browser (Standard: Same-Origin über Nginx Reverse-Proxy)
export PUBLIC_IDENTITY_URL="${PUBLIC_IDENTITY_URL:-/api/identity}"
export PUBLIC_AGGREGATOR_URL="${PUBLIC_AGGREGATOR_URL:-/api/aggregator}"
export PUBLIC_INCIDENT_MANAGER_URL="${PUBLIC_INCIDENT_MANAGER_URL:-/api/incident-manager}"
export PUBLIC_STIX_URL="${PUBLIC_STIX_URL:-/api/stix}"

# Template mit Umgebungsvariablen füllen (nur explizite Variablen ersetzen, um $host/$uri zu schützen)
if [ -f /etc/nginx/nginx.conf.template ]; then
  envsubst '${IDENTITY_URL} ${AGGREGATOR_URL} ${INCIDENT_MANAGER_URL} ${STIX_URL}' \
    < /etc/nginx/nginx.conf.template \
    > /etc/nginx/conf.d/default.conf
fi

# Laufzeit-Konfiguration für das Frontend generieren
cat > /usr/share/nginx/html/config.js <<EOF
window.APP_CONFIG = {
  identityUrl: "${PUBLIC_IDENTITY_URL}",
  aggregatorUrl: "${PUBLIC_AGGREGATOR_URL}",
  incidentManagerUrl: "${PUBLIC_INCIDENT_MANAGER_URL}",
  stixUrl: "${PUBLIC_STIX_URL}"
};
EOF

exec nginx -g "daemon off;"
