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

envsubst '${API_URL}' < /etc/nginx/nginx.conf.template > /etc/nginx/nginx.conf

echo "Nginx configuration:"
grep -A 5 "location /api" /etc/nginx/nginx.conf || echo "Could not find /api location block"

echo "====================================="
echo "Starting nginx..."
echo "====================================="

# Start nginx
exec nginx -g 'daemon off;'
