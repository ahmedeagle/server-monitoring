# 🧪 LOCAL TESTING GUIDE

Complete guide to test the Server Monitoring System locally with Docker.

---

## ✅ Prerequisites

- **Docker Desktop** installed and running
  - Windows: [Download Docker Desktop](https://www.docker.com/products/docker-desktop/)
  - Make sure Docker Desktop is running (check system tray)
- **PowerShell 7+** or **Command Prompt**
- **Windows 10/11** (or Linux/Mac with Docker)

---

## 🚀 Quick Start (5 Minutes)

### Option 1: Automated Testing Script (Recommended)

```powershell
# Navigate to project folder
cd C:\Users\lenovo\Downloads\assesment

# Run automated test script
.\test-local.ps1
```

This script will:
1. ✓ Check Docker installation
2. ✓ Clean up existing containers
3. ✓ Build and start all services
4. ✓ Wait for services to be healthy
5. ✓ Test all endpoints
6. ✓ Display access URLs

**Expected time**: 5-10 minutes (first run)

---

### Option 2: Manual Testing

```powershell
# Navigate to project folder
cd C:\Users\lenovo\Downloads\assesment

# Start all services
docker compose up -d --build

# Wait 2-3 minutes for services to start

# Check status
docker compose ps

# View logs
docker compose logs -f
```

---

## 🌐 Access Points

Once services are running:

| Service | URL | Credentials |
|---------|-----|-------------|
| **Web App** | http://localhost:3000 | admin / Admin123! |
| **API** | http://localhost:5000 | - |
| **Swagger** | http://localhost:5000/swagger | - |
| **Hangfire** | http://localhost:5000/hangfire | admin / admin123 |
| **SQL Server** | localhost:1433 | sa / YourStrong@Passw0rd |
| **Redis** | localhost:6379 | password: YourRedis@Password |

---

## 📋 Verification Checklist

### 1. Check Docker Containers Running

```powershell
docker compose ps
```

**Expected output**: All 4 containers running (sqlserver, redis, api, web)

```
NAME                        STATUS          PORTS
servermonitoring-api        Up (healthy)    0.0.0.0:5000->8080/tcp
servermonitoring-web        Up (healthy)    0.0.0.0:3000->3000/tcp
servermonitoring-sqlserver  Up (healthy)    0.0.0.0:1433->1433/tcp
servermonitoring-redis      Up (healthy)    0.0.0.0:6379->6379/tcp
```

### 2. Test Health Endpoints

```powershell
# API Health Check
Invoke-RestMethod http://localhost:5000/health

# Expected: {"status": "Healthy"}
```

### 3. Test Swagger UI

Open browser: http://localhost:5000/swagger

**Verify:**
- ✓ Swagger UI loads
- ✓ API endpoints visible
- ✓ Can expand and view endpoint details

### 4. Test Web Frontend

Open browser: http://localhost:3000

**Verify:**
- ✓ Login page displays
- ✓ Can login with admin/Admin123!
- ✓ Dashboard loads after login
- ✓ No console errors (F12 → Console)

### 5. Test Background Jobs

Open browser: http://localhost:5000/hangfire

Login: admin / admin123

**Verify:**
- ✓ Hangfire dashboard displays
- ✓ Jobs are scheduled
- ✓ Recurring jobs visible (MetricsCollection)

### 6. Test Real-Time Updates

1. Login to web app
2. Navigate to Dashboard
3. Watch for real-time metric updates
4. Metrics should update automatically (SignalR)

---

## 🔍 Testing Features

### Test Authentication

```powershell
# Test login endpoint
$body = @{
    username = "admin"
    password = "Admin123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body

# Response should contain accessToken and refreshToken
$response.accessToken
```

### Test API Endpoints

```powershell
# First, get token
$loginBody = @{
    username = "admin"
    password = "Admin123!"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $loginBody

$token = $loginResponse.accessToken

# Test servers endpoint
$headers = @{
    Authorization = "Bearer $token"
}

$servers = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/servers" `
    -Method GET `
    -Headers $headers

$servers
```

### Test Database Connection

```powershell
# Using SQL Server Management Studio or Azure Data Studio
Server: localhost,1433
Login: sa
Password: YourStrong@Passw0rd
Database: ServerMonitoringDb

# Or using sqlcmd
docker exec -it servermonitoring-sqlserver /opt/mssql-tools/bin/sqlcmd `
    -S localhost -U sa -P "YourStrong@Passw0rd" `
    -Q "SELECT * FROM sys.databases WHERE name='ServerMonitoringDb'"
```

---

## 📊 View Logs

### All Services
```powershell
docker compose logs -f
```

### Specific Services
```powershell
# API logs
docker compose logs -f api

# Web logs
docker compose logs -f web

# SQL Server logs
docker compose logs -f sqlserver

# Last 50 lines
docker compose logs --tail=50 api
```

---

## 🔧 Common Issues & Solutions

### Issue 1: Docker not running

**Error**: "Cannot connect to Docker daemon"

**Solution**:
- Start Docker Desktop
- Wait for it to fully start (icon in system tray)
- Run command again

### Issue 2: Port already in use

**Error**: "Port 5000 is already allocated"

**Solution**:
```powershell
# Stop conflicting services or change ports in docker-compose.yml
# Or stop existing containers
docker compose down

# Find process using port
netstat -ano | findstr :5000
# Kill process (replace PID with actual number)
taskkill /PID <PID> /F
```

### Issue 3: Containers fail to start

**Error**: Container keeps restarting

**Solution**:
```powershell
# Check logs
docker compose logs api

# Common fixes:
# 1. Check SQL Server is healthy
docker inspect servermonitoring-sqlserver --format='{{.State.Health.Status}}'

# 2. Reset and rebuild
docker compose down -v
docker compose up -d --build
```

### Issue 4: Database connection errors

**Error**: "Cannot connect to database"

**Solution**:
```powershell
# Wait for SQL Server to fully start (can take 30-60 seconds)
docker compose logs sqlserver

# Check health status
docker compose ps

# If needed, restart just the API
docker compose restart api
```

### Issue 5: Web app shows connection error

**Error**: "Cannot connect to API"

**Solution**:
1. Verify API is running: http://localhost:5000/health
2. Check web container environment variables
3. Restart web service:
```powershell
docker compose restart web
```

---

## 🧹 Cleanup Commands

### Stop Services (Keep Data)
```powershell
docker compose down
```

### Stop and Remove Everything
```powershell
# Removes containers, networks, and volumes
docker compose down -v
```

### Remove Images (Force Fresh Build)
```powershell
docker compose down -v
docker rmi servermonitoring-api servermonitoring-web
```

### Complete Cleanup
```powershell
# Remove all stopped containers
docker container prune -f

# Remove all unused images
docker image prune -a -f

# Remove all unused volumes
docker volume prune -f

# Remove all unused networks
docker network prune -f
```

---

## 🎯 Testing Scenarios

### Scenario 1: First Time User

1. ✓ Start services: `docker compose up -d --build`
2. ✓ Wait 3-5 minutes
3. ✓ Open http://localhost:3000
4. ✓ Login with admin/Admin123!
5. ✓ See dashboard with metrics
6. ✓ Navigate to different pages
7. ✓ Check Swagger: http://localhost:5000/swagger

### Scenario 2: Testing Background Jobs

1. ✓ Open Hangfire: http://localhost:5000/hangfire
2. ✓ Login: admin/admin123
3. ✓ Go to "Recurring Jobs"
4. ✓ See "MetricsCollection" job
5. ✓ Click "Trigger Now"
6. ✓ Go to "Jobs" → see execution
7. ✓ Check dashboard for new metrics

### Scenario 3: Testing Real-Time Updates

1. ✓ Login to web app
2. ✓ Navigate to Dashboard
3. ✓ Open browser console (F12)
4. ✓ Check for SignalR connection logs
5. ✓ Watch metrics update in real-time
6. ✓ Open second browser tab
7. ✓ Verify both tabs update simultaneously

### Scenario 4: Testing API with Swagger

1. ✓ Open http://localhost:5000/swagger
2. ✓ Find "Auth" section
3. ✓ Execute POST /api/v1/auth/login
4. ✓ Use: `{"username":"admin","password":"Admin123!"}`
5. ✓ Copy accessToken from response
6. ✓ Click "Authorize" button (top right)
7. ✓ Paste token: `Bearer YOUR_TOKEN`
8. ✓ Test other endpoints

---

## 📸 Screenshots to Verify

### Expected Web App Views

1. **Login Page**: Modern login form with username/password fields
2. **Dashboard**: Charts showing CPU, Memory, Disk metrics
3. **Servers Page**: List of monitored servers
4. **Server Details**: Individual server metrics and history
5. **Alerts Page**: List of triggered alerts
6. **Reports Page**: Generated reports list

### Expected Swagger UI

1. **Auth Endpoints**: Login, Register, Refresh Token
2. **Servers Endpoints**: CRUD operations
3. **Metrics Endpoints**: Get metrics, history
4. **Alerts Endpoints**: Get alerts, acknowledge
5. **Reports Endpoints**: Generate, download

### Expected Hangfire Dashboard

1. **Jobs**: Succeeded, Failed, Processing
2. **Recurring Jobs**: MetricsCollection scheduled
3. **Servers**: Active Hangfire server
4. **Retries**: Failed jobs with retry attempts

---

## 🔄 Restarting Services

### Restart All Services
```powershell
docker compose restart
```

### Restart Specific Service
```powershell
docker compose restart api
docker compose restart web
```

### Rebuild and Restart
```powershell
docker compose down
docker compose up -d --build
```

---

## 💡 Pro Tips

### Tip 1: Quick Reset
```powershell
# Quick reset without losing data
docker compose down && docker compose up -d
```

### Tip 2: Background Mode
```powershell
# Run in background (detached mode)
docker compose up -d

# View logs anytime
docker compose logs -f
```

### Tip 3: Monitor Resources
```powershell
# Check resource usage
docker stats

# Shows CPU%, Memory, Network I/O for each container
```

### Tip 4: Access Container Shell
```powershell
# API container
docker exec -it servermonitoring-api /bin/bash

# SQL Server
docker exec -it servermonitoring-sqlserver /bin/bash
```

### Tip 5: Check Network
```powershell
# Inspect network
docker network inspect servermonitoring-network

# Shows all connected containers
```

---

## ✅ Success Criteria

You'll know everything is working when:

- ✓ All 4 containers show "Up (healthy)" status
- ✓ Web app loads at http://localhost:3000
- ✓ Can login successfully
- ✓ Dashboard displays metrics (even if simulated)
- ✓ Swagger UI accessible and interactive
- ✓ Hangfire dashboard shows scheduled jobs
- ✓ No error logs in `docker compose logs`
- ✓ Health endpoint returns healthy status
- ✓ SignalR connection established (check browser console)

---

## 📞 Need Help?

### Check Status
```powershell
# Full status check
docker compose ps
docker compose logs --tail=50
```

### Common Debug Commands
```powershell
# Inspect specific container
docker inspect servermonitoring-api

# Check health
docker inspect servermonitoring-api --format='{{.State.Health.Status}}'

# View environment variables
docker inspect servermonitoring-api --format='{{json .Config.Env}}'
```

---

## 🎉 Ready to Test!

Run the automated test script:
```powershell
.\test-local.ps1
```

Or start manually:
```powershell
docker compose up -d --build
```

Then open: **http://localhost:3000** 🚀

---

**Happy Testing!** If everything runs locally, you're ready to push to GitHub for submission! ✅
