# =========================================
# Environment Setup Script
# Automatically creates .env files from templates
# =========================================

param(
    [switch]$Force,
    [ValidateSet('Development', 'Production')]
    [string]$Environment = 'Development'
)

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Environment Setup Script" -ForegroundColor Cyan
Write-Host "=========================================`n" -ForegroundColor Cyan

$rootEnvFile = ".env"
$frontendEnvFile = "ServerMonitoring.Web\.env"
$created = @()
$skipped = @()

# Function to copy env file
function Copy-EnvFile {
    param(
        [string]$Source,
        [string]$Destination,
        [string]$Description
    )
    
    if (Test-Path $Destination) {
        if ($Force) {
            Write-Host "⚠️  Overwriting $Description" -ForegroundColor Yellow
            Copy-Item $Source $Destination -Force
            $script:created += $Description
        } else {
            Write-Host "⏭️  $Description already exists (use -Force to overwrite)" -ForegroundColor Gray
            $script:skipped += $Description
        }
    } else {
        Write-Host "✅ Creating $Description" -ForegroundColor Green
        Copy-Item $Source $Destination
        $script:created += $Description
    }
}

# Setup root .env file
Write-Host "Setting up root environment...`n" -ForegroundColor Cyan

if ($Environment -eq 'Production') {
    if (Test-Path ".env.production") {
        Copy-EnvFile -Source ".env.production" -Destination $rootEnvFile -Description "Root .env (Production)"
        Write-Host "⚠️  IMPORTANT: Edit .env and change all passwords and secrets!" -ForegroundColor Yellow
    } else {
        Write-Host "❌ .env.production template not found!" -ForegroundColor Red
        exit 1
    }
} else {
    if (Test-Path ".env.example") {
        Copy-EnvFile -Source ".env.example" -Destination $rootEnvFile -Description "Root .env (Development)"
    } else {
        Write-Host "⚠️  .env.example not found, using existing .env" -ForegroundColor Yellow
    }
}

# Setup frontend .env file
Write-Host "`nSetting up frontend environment...`n" -ForegroundColor Cyan

$frontendSource = if ($Environment -eq 'Production') { 
    "ServerMonitoring.Web\.env.production" 
} else { 
    "ServerMonitoring.Web\.env.development" 
}

if (Test-Path $frontendSource) {
    Copy-EnvFile -Source $frontendSource -Destination $frontendEnvFile -Description "Frontend .env ($Environment)"
} else {
    Write-Host "⚠️  Frontend template not found: $frontendSource" -ForegroundColor Yellow
}

# Summary
Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Setup Summary" -ForegroundColor Cyan
Write-Host "=========================================`n" -ForegroundColor Cyan

if ($created.Count -gt 0) {
    Write-Host "✅ Created/Updated ($($created.Count)):" -ForegroundColor Green
    foreach ($item in $created) {
        Write-Host "   - $item" -ForegroundColor Green
    }
}

if ($skipped.Count -gt 0) {
    Write-Host "`n⏭️  Skipped ($($skipped.Count)):" -ForegroundColor Gray
    foreach ($item in $skipped) {
        Write-Host "   - $item" -ForegroundColor Gray
    }
}

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Next Steps" -ForegroundColor Cyan
Write-Host "=========================================`n" -ForegroundColor Cyan

if ($Environment -eq 'Production') {
    Write-Host "⚠️  PRODUCTION MODE" -ForegroundColor Yellow
    Write-Host "`n1. Edit .env file:" -ForegroundColor White
    Write-Host "   notepad .env" -ForegroundColor Gray
    Write-Host "`n2. Change all passwords and secrets:" -ForegroundColor White
    Write-Host "   - DB_SA_PASSWORD" -ForegroundColor Gray
    Write-Host "   - REDIS_PASSWORD" -ForegroundColor Gray
    Write-Host "   - JWT_SECRET_KEY" -ForegroundColor Gray
    Write-Host "   - VITE_API_URL (to your domain)" -ForegroundColor Gray
    Write-Host "`n3. Validate configuration:" -ForegroundColor White
    Write-Host "   .\validate-env.ps1" -ForegroundColor Gray
    Write-Host "`n4. Deploy application:" -ForegroundColor White
    Write-Host "   docker-compose up -d" -ForegroundColor Gray
} else {
    Write-Host "✅ DEVELOPMENT MODE" -ForegroundColor Green
    Write-Host "`n1. (Optional) Validate configuration:" -ForegroundColor White
    Write-Host "   .\validate-env.ps1" -ForegroundColor Gray
    Write-Host "`n2. Start the application:" -ForegroundColor White
    Write-Host "   docker-compose up -d" -ForegroundColor Gray
    Write-Host "`n3. Access the application:" -ForegroundColor White
    Write-Host "   Frontend: http://localhost:3000" -ForegroundColor Gray
    Write-Host "   API:      http://localhost:5000" -ForegroundColor Gray
}

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Environment Setup Complete!" -ForegroundColor Green
Write-Host "=========================================`n" -ForegroundColor Cyan

exit 0
