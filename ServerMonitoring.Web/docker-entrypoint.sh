#!/bin/sh
set -e

# Substitute environment variables in nginx config
echo "====================================="
echo "Configuring nginx..."
echo "API_URL=${API_URL}"
echo "====================================="

if [ -z "$API_URL" ]; then
    echo "WARNING: API_URL not set, using default http://localhost:8080"
    export API_URL="http://localhost:8080"
fi

# Extract hostname:port from API_URL (remove http:// or https://)
API_HOST=$(echo "$API_URL" | sed -e 's|^http://||' -e 's|^https://||' -e 's|/$||')
echo "API_HOST extracted: $API_HOST"

# Export for envsubst
export API_HOST

# Substitute both API_URL and API_HOST
envsubst '${API_URL} ${API_HOST}' < /etc/nginx/nginx.conf.template > /etc/nginx/nginx.conf

echo "Nginx configuration:"
grep -A 5 "upstream\|location /api" /etc/nginx/nginx.conf || echo "Could not find configuration blocks"

echo "====================================="
echo "Starting nginx..."
echo "====================================="

# Start nginx
exec nginx -g 'daemon off;'
