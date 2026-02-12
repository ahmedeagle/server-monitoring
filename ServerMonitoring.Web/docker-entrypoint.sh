#!/bin/sh
set -e

# Substitute environment variables in nginx config
echo "Configuring nginx with API_URL=${API_URL}"
envsubst '${API_URL}' < /etc/nginx/nginx.conf.template > /etc/nginx/nginx.conf

# Start nginx
exec nginx -g 'daemon off;'
