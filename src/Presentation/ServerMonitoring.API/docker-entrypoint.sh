#!/bin/bash
set -e

# Fix permissions for /data directory if it exists
# This is necessary because volume mounts override Dockerfile permissions
if [ -d "/data" ]; then
    echo "Fixing /data directory permissions for non-root user..."
    chown -R appuser:appuser /data
    chmod -R 755 /data
fi

# Switch to non-root user and execute the application
echo "Starting application as appuser..."
exec gosu appuser dotnet ServerMonitoring.API.dll
