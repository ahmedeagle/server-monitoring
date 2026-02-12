# ================================
# Build Stage
# ================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy everything
COPY . .

# Restore, build, and publish in one go (most reliable)
RUN dotnet publish "src/Presentation/ServerMonitoring.API/ServerMonitoring.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ================================
# Runtime Stage
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS publish

WORKDIR /app

# Install curl for health checks and su-exec for user switching
RUN apt-get update && apt-get install -y curl su-exec && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r -g 1001 appuser && useradd -r -g appuser -u 1001 appuser

# Create data directory for SQLite database with proper permissions
RUN mkdir -p /data && chown -R appuser:appuser /data

# Copy from build stage
COPY --from=build --chown=appuser:appuser /app/publish .

# Copy entrypoint script
COPY src/Presentation/ServerMonitoring.API/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Note: We start as root to fix volume permissions, then switch to appuser in entrypoint
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["/docker-entrypoint.sh"]
