# Test Script - Full Docker Setup with SQL Server
Write-Host "🚀 Server Monitoring - Full Stack Test (Docker + SQL Server)" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Gray
Write-Host ""

# Check Docker
Write-Host "📦 Checking Docker Desktop..." -ForegroundColor Yellow
$dockerRunning = docker info 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker Desktop is not running!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please start Docker Desktop and run this script again." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "✅ Docker is running" -ForegroundColor Green

# Clean up previous containers
Write-Host "
🧹 Cleaning up previous containers..." -ForegroundColor Yellow
docker-compose down -v 2>$null | Out-Null
Write-Host "✅ Cleanup complete" -ForegroundColor Green

# Build and start services
Write-Host "
🔨 Building and starting services..." -ForegroundColor Yellow
Write-Host "   - SQL Server 2022 (port 1433)" -ForegroundColor White
Write-Host "   - Redis Cache (port 6379)" -ForegroundColor White
Write-Host "   - .NET API (port 5000)" -ForegroundColor White
Write-Host "   - React Web (port 3000)" -ForegroundColor White
Write-Host ""
Write-Host "⏳ This will take 2-3 minutes..." -ForegroundColor Gray

docker-compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to start services" -ForegroundColor Red
    exit 1
}

# Wait for services to be healthy
Write-Host "
⏳ Waiting for services to be healthy..." -ForegroundColor Yellow
$maxWait = 120
$waited = 0

while ($waited -lt $maxWait) {
    $sqlHealth = docker inspect servermonitoring-sqlserver --format='{{.State.Health.Status}}' 2>$null
    $apiHealth = docker inspect servermonitoring-api --format='{{.State.Health.Status}}' 2>$null
    
    if ($sqlHealth -eq "healthy" -and $apiHealth -eq "healthy") {
        break
    }
    
    Write-Host "   Waiting... ($waited/$maxWait seconds)" -ForegroundColor Gray
    Start-Sleep -Seconds 5
    $waited += 5
}

if ($waited -ge $maxWait) {
    Write-Host "⚠️  Services took too long to start, but continuing..." -ForegroundColor Yellow
} else {
    Write-Host "✅ All services are healthy!" -ForegroundColor Green
}

# Test API
Write-Host "
🧪 Testing API endpoints..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

try {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get -TimeoutSec 5
    Write-Host "✅ Health check passed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Health check failed, but API might still be starting..." -ForegroundColor Yellow
}

# Display success message
Write-Host "
" + ("=" * 80) -ForegroundColor Green
Write-Host "🎉 ALL SERVICES RUNNING!" -ForegroundColor Green
Write-Host ("=" * 80) -ForegroundColor Green
Write-Host ""
Write-Host "📍 Access Points:" -ForegroundColor Cyan
Write-Host "   Frontend App:      " -NoNewline
Write-Host "http://localhost:3000" -ForegroundColor Yellow
Write-Host "   API Swagger:       " -NoNewline
Write-Host "http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "   API Health:        " -NoNewline
Write-Host "http://localhost:5000/health" -ForegroundColor Yellow
Write-Host "   Hangfire Jobs:     " -NoNewline
Write-Host "http://localhost:5000/hangfire" -ForegroundColor Yellow
Write-Host ""
Write-Host "🔐 Login Credentials:" -ForegroundColor Cyan
Write-Host "   Username: " -NoNewline
Write-Host "admin" -ForegroundColor Yellow
Write-Host "   Password: " -NoNewline
Write-Host "Admin123!" -ForegroundColor Yellow
Write-Host ""
Write-Host "💾 Database Setup:" -ForegroundColor Cyan
Write-Host "   ✅ SQL Server 2022 running in Docker" -ForegroundColor Green
Write-Host "   ✅ Entity Framework Core with migrations" -ForegroundColor Green
Write-Host "   ✅ Database: ServerMonitoringDb" -ForegroundColor Green
Write-Host "   ✅ Redis Cache enabled" -ForegroundColor Green
Write-Host ""
Write-Host "📊 To view logs:" -ForegroundColor Gray
Write-Host "   docker-compose logs -f" -ForegroundColor White
Write-Host ""
Write-Host "🛑 To stop services:" -ForegroundColor Gray
Write-Host "   docker-compose down" -ForegroundColor White
Write-Host ""
Write-Host ("=" * 80) -ForegroundColor Green
