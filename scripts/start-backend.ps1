# ================================
# START BACKEND API WITH SQL SERVER
# ================================

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Starting Backend API with SQL Server" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker SQL Server is running
Write-Host "📦 Checking SQL Server..." -ForegroundColor Yellow
$sqlContainer = docker ps --filter "name=servermonitoring-sqlserver" --filter "status=running" --format "{{.Names}}" 2>$null

if ($sqlContainer -ne "servermonitoring-sqlserver") {
    Write-Host "⚠️  SQL Server container not running!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Starting SQL Server..." -ForegroundColor Yellow
    docker-compose up -d sqlserver redis
    Write-Host "⏳ Waiting 15 seconds for SQL Server to initialize..." -ForegroundColor Gray
    Start-Sleep -Seconds 15
    Write-Host "✅ SQL Server started" -ForegroundColor Green
} else {
    Write-Host "✅ SQL Server already running" -ForegroundColor Green
}

Write-Host ""
Write-Host "🚀 Starting API on http://localhost:5000" -ForegroundColor Green
Write-Host "📚 Swagger: http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "💾 Database: SQL Server (Docker)" -ForegroundColor Cyan
Write-Host ""

cd "c:\Users\lenovo\Downloads\assesment\src\Presentation\ServerMonitoring.API"

# DO NOT use in-memory - use real SQL Server
$env:UseInMemoryDatabase = "false"
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=ServerMonitoringDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true"
$env:ConnectionStrings__RedisConnection = "localhost:6379,password=YourRedis@Password,abortConnect=false"

# Start the API
dotnet run
