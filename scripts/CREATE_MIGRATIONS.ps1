# =============================================================================
# SQL Server Migration Setup Script
# =============================================================================
# This script creates EF Core migrations for SQL Server deployment
# Run this from a proper development location (not Downloads folder)
# =============================================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SQL Server Migration Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running from Downloads
$currentPath = Get-Location
if ($currentPath.Path -like "*Downloads*") {
    Write-Host "[WARNING] Running from Downloads folder detected!" -ForegroundColor Yellow
    Write-Host "Migrations work best from proper development location like C:\Projects\" -ForegroundColor Yellow
    Write-Host ""
    
    $continue = Read-Host "Continue anyway? (y/n)"
    if ($continue -ne 'y') {
        Write-Host "Exiting. Please move project to C:\Projects first." -ForegroundColor Yellow
        exit 1
    }
}

# Check if dotnet-ef is installed
Write-Host "[CHECK] Verifying EF Core tools..." -ForegroundColor Cyan
$efInstalled = dotnet tool list --global | Select-String "dotnet-ef"

if (-not $efInstalled) {
    Write-Host "[INSTALL] Installing dotnet-ef globally..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Failed to install dotnet-ef" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] dotnet-ef installed successfully" -ForegroundColor Green
} else {
    Write-Host "[OK] dotnet-ef is already installed" -ForegroundColor Green
}

Write-Host ""

# Navigate to Infrastructure project
$infraPath = "src\Infrastructure\ServerMonitoring.Infrastructure"
if (-not (Test-Path $infraPath)) {
    Write-Host "[ERROR] Infrastructure project not found at: $infraPath" -ForegroundColor Red
    exit 1
}

Write-Host "[INFO] Navigating to Infrastructure project..." -ForegroundColor Cyan
Set-Location $infraPath

# Create migration
Write-Host ""
Write-Host "[CREATE] Creating InitialCreate migration..." -ForegroundColor Cyan

$migrationCommand = "dotnet ef migrations add InitialCreate " +
    "--startup-project ..\..\Presentation\ServerMonitoring.API\ServerMonitoring.API.csproj " +
    "--context ApplicationDbContext " +
    "--output-dir Migrations"

Write-Host "[CMD] $migrationCommand" -ForegroundColor DarkGray
Write-Host ""

Invoke-Expression $migrationCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "[ERROR] Migration creation failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Common solutions:" -ForegroundColor Yellow
    Write-Host "  1. Stop any running API instances (dotnet run)" -ForegroundColor Yellow
    Write-Host "  2. Clean and rebuild solution: dotnet clean; dotnet build" -ForegroundColor Yellow
    Write-Host "  3. Check for compilation errors: dotnet build" -ForegroundColor Yellow
    Write-Host "  4. Move project to C:\Projects to avoid file lock issues" -ForegroundColor Yellow
    
    Set-Location ..\..\..
    exit 1
}

Write-Host ""
Write-Host "[SUCCESS] Migration created successfully!" -ForegroundColor Green
Write-Host ""

# Return to root
Set-Location ..\..\..

# Display migration files
Write-Host "[INFO] Migration files created:" -ForegroundColor Cyan
Get-ChildItem "$infraPath\Migrations" -Filter "*.cs" | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor White
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Review migration files in:" -ForegroundColor White
Write-Host "   $infraPath\Migrations" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Update database (when SQL Server is ready):" -ForegroundColor White
Write-Host "   cd $infraPath" -ForegroundColor Gray
Write-Host "   dotnet ef database update --startup-project ..\..\Presentation\ServerMonitoring.API" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Configure connection string in appsettings.json:" -ForegroundColor White
Write-Host '   "ConnectionStrings": {' -ForegroundColor Gray
Write-Host '     "DefaultConnection": "Server=localhost;Database=ServerMonitoring;User Id=sa;Password=YourPass;"' -ForegroundColor Gray
Write-Host '   }' -ForegroundColor Gray
Write-Host ""
Write-Host "4. Set UseInMemoryDatabase=false in appsettings.json" -ForegroundColor White
Write-Host ""
Write-Host "[COMPLETE] Migration setup finished!" -ForegroundColor Green
