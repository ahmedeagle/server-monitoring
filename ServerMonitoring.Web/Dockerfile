# ================================
# Build Stage
# ================================
FROM node:20-alpine AS build

# Add metadata labels
LABEL maintainer="ServerMonitoring Team"
LABEL description="Server Monitoring Web UI - React + Vite"
LABEL version="1.0"

WORKDIR /app

# Install dependencies for node-gyp if needed
RUN apk add --no-cache python3 make g++

# Copy package files (layer caching optimization)
COPY package*.json ./

# Install dependencies with clean install
RUN npm ci --silent --no-audit --no-fund \
    && npm cache clean --force

# Copy source code
COPY . .

# Build the application
RUN npm run build \
    && rm -rf node_modules

# ================================
# Runtime Stage (Nginx)
# ================================
FROM nginx:alpine AS runtime

# Add metadata labels
LABEL maintainer="ServerMonitoring Team"
LABEL description="Server Monitoring Web UI Runtime"
LABEL version="1.0"

# Install security updates
RUN apk upgrade --no-cache \
    && apk add --no-cache \
    ca-certificates \
    tzdata

# Copy custom nginx configuration as template
COPY nginx.conf /etc/nginx/nginx.conf.template

# Copy entrypoint script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Copy built app from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Set default API URL (can be overridden at runtime)
ENV API_URL=http://localhost:8080

# Use custom entrypoint
ENTRYPOINT ["/docker-entrypoint.sh"]

# Create non-root user (security best practice)
RUN addgroup -g 1001 -S appuser \
    && adduser -u 1001 -S appuser -G appuser

# Set ownership and permissions
RUN chown -R appuser:appuser /usr/share/nginx/html \
    && chown -R appuser:appuser /var/cache/nginx \
    && chown -R appuser:appuser /var/log/nginx \
    && chown -R appuser:appuser /etc/nginx/conf.d \
    && touch /var/run/nginx.pid \
    && chown -R appuser:appuser /var/run/nginx.pid

# Remove default nginx files
RUN rm -f /etc/nginx/conf.d/default.conf

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:3000 || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
