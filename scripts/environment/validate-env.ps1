# =========================================
# Environment Variables Validation Script
# =========================================
# This script validates that all required environment variables are present
# and have appropriate values before starting the application.
# =========================================

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Environment Variables Validation" -ForegroundColor Cyan
Write-Host "=========================================`n" -ForegroundColor Cyan

# Check if .env file exists
if (-not (Test-Path ".env")) {
    Write-Host "❌ ERROR: .env file not found!" -ForegroundColor Red
    Write-Host "   Run: Copy-Item .env.example .env" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ .env file found" -ForegroundColor Green

# Required variables
$requiredVars = @(
    'DB_SA_PASSWORD',
    'SQL_SERVER_HOST',
    'SQL_SERVER_PORT',
    'SQL_DATABASE_NAME',
    'REDIS_PASSWORD',
    'REDIS_HOST',
    'REDIS_PORT',
    'ASPNETCORE_ENVIRONMENT',
    'JWT_SECRET_KEY',
    'JWT_ISSUER',
    'JWT_AUDIENCE',
    'API_PORT',
    'WEB_PORT',
    'VITE_API_URL'
)

# Load .env file
$envContent = Get-Content .env -ErrorAction Stop
$envVars = @{}

foreach ($line in $envContent) {
    if ($line -match '^([^#][^=]+)=(.*)$') {
        $key = $matches[1].Trim()
        $value = $matches[2].Trim()
        $envVars[$key] = $value
    }
}

Write-Host "`nChecking required variables...`n" -ForegroundColor Cyan

$missing = @()
$warnings = @()
$passed = 0

foreach ($var in $requiredVars) {
    if ($envVars.ContainsKey($var)) {
        $value = $envVars[$var]
        
        # Check for empty values
        if ([string]::IsNullOrWhiteSpace($value)) {
            Write-Host "⚠️  $var is empty" -ForegroundColor Yellow
            $warnings += "$var is empty"
        }
        # Check for default/weak passwords
        elseif ($var -eq 'DB_SA_PASSWORD' -and $value -eq 'YourStrong@Passw0rd') {
            Write-Host "⚠️  $var is using default password (OK for dev, change for production)" -ForegroundColor Yellow
            $warnings += "$var uses default password"
        }
        elseif ($var -eq 'REDIS_PASSWORD' -and $value -eq 'YourRedis@Password') {
            Write-Host "⚠️  $var is using default password (OK for dev, change for production)" -ForegroundColor Yellow
            $warnings += "$var uses default password"
        }
        elseif ($var -eq 'JWT_SECRET_KEY' -and $value.Length -lt 32) {
            Write-Host "❌ $var must be at least 32 characters" -ForegroundColor Red
            $missing += "$var (too short)"
        }
        elseif ($var -eq 'JWT_SECRET_KEY' -and $value -like '*CHANGE*') {
            Write-Host "⚠️  $var contains 'CHANGE' - update for production" -ForegroundColor Yellow
            $warnings += "$var needs to be changed for production"
        }
        else {
            Write-Host "✅ $var" -ForegroundColor Green
            $passed++
        }
    }
    else {
        Write-Host "❌ $var is missing" -ForegroundColor Red
        $missing += $var
    }
}

# Check Frontend .env
Write-Host "`nChecking frontend environment...`n" -ForegroundColor Cyan

if (Test-Path "ServerMonitoring.Web\.env") {
    Write-Host "✅ Frontend .env file found" -ForegroundColor Green
    
    $frontendEnv = Get-Content "ServerMonitoring.Web\.env"
    if ($frontendEnv -match 'VITE_API_URL') {
        Write-Host "✅ VITE_API_URL configured" -ForegroundColor Green
    } else {
        Write-Host "⚠️  VITE_API_URL not found in frontend .env" -ForegroundColor Yellow
        $warnings += "Frontend VITE_API_URL not configured"
    }
} else {
    Write-Host "⚠️  Frontend .env file not found (will use defaults)" -ForegroundColor Yellow
    $warnings += "Frontend .env file missing"
}

# Summary
Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Validation Summary" -ForegroundColor Cyan
Write-Host "=========================================`n" -ForegroundColor Cyan

Write-Host "✅ Passed: $passed / $($requiredVars.Count)" -ForegroundColor Green

if ($warnings.Count -gt 0) {
    Write-Host "⚠️  Warnings: $($warnings.Count)" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "   - $warning" -ForegroundColor Yellow
    }
}

if ($missing.Count -gt 0) {
    Write-Host "❌ Missing/Invalid: $($missing.Count)" -ForegroundColor Red
    foreach ($miss in $missing) {
        Write-Host "   - $miss" -ForegroundColor Red
    }
    Write-Host "`n❌ Validation FAILED!" -ForegroundColor Red
    Write-Host "   Please fix the issues above before starting the application.`n" -ForegroundColor Yellow
    exit 1
}

# Production check
if ($envVars['ASPNETCORE_ENVIRONMENT'] -eq 'Production') {
    Write-Host "`n⚠️  PRODUCTION MODE DETECTED" -ForegroundColor Yellow
    Write-Host "   Please ensure:" -ForegroundColor Yellow
    Write-Host "   - All passwords are strong and unique" -ForegroundColor Yellow
    Write-Host "   - JWT_SECRET_KEY is cryptographically random" -ForegroundColor Yellow
    Write-Host "   - VITE_API_URL points to production domain" -ForegroundColor Yellow
    Write-Host "   - SSL/TLS is configured" -ForegroundColor Yellow
    Write-Host "   - Firewall rules are in place`n" -ForegroundColor Yellow
}

Write-Host "`n✅ Validation PASSED!" -ForegroundColor Green
Write-Host "   You can now run: docker-compose up -d`n" -ForegroundColor Green

exit 0
