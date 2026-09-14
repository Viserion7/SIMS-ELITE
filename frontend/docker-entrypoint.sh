#!/bin/sh
set -e

IDENTITY_URL="${IDENTITY_URL:-}"
AGGREGATOR_URL="${AGGREGATOR_URL:-}"

cat > /usr/share/nginx/html/config.js <<EOF
window.APP_CONFIG = { identityUrl: "${IDENTITY_URL}", aggregatorUrl: "${AGGREGATOR_URL}" };
EOF

exec nginx -g "daemon off;"
