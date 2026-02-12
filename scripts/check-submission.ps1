#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verify submission readiness
.DESCRIPTION
    Checks that all files are in place and ready for GitHub submission
#>

$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Submission Readiness Check" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# Check source code
Write-Host "1. Checking Source Code..." -ForegroundColor Yellow
$sourceFolders = @(
    "src/Domain",
    "src/Application",
    "src/Infrastructure",
    "src/Presentation",
    "ServerMonitoring.Web",
    "tests/ServerMonitoring.UnitTests",
    "tests/ServerMonitoring.IntegrationTests"
)

foreach ($folder in $sourceFolders) {
    if (Test-Path $folder) {
        Write-Host "   ✓ $folder" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $folder MISSING" -ForegroundColor Red
        $allGood = $false
    }
}

# Check documentation
Write-Host ""
Write-Host "2. Checking Documentation..." -ForegroundColor Yellow
$docFiles = @(
    "README.md",
    "ARCHITECTURE.md",
    "DEPLOYMENT.md",
    "LOCAL_TESTING_GUIDE.md",
    "TESTING_COMPLETE.md",
    "AWS_DEPLOYMENT.md",
    "SUBMISSION_GUIDE.md",
    "START_HERE.md"
)

foreach ($file in $docFiles) {
    if (Test-Path $file) {
        Write-Host "   ✓ $file" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $file MISSING" -ForegroundColor Red
        $allGood = $false
    }
}

# Check Docker files
Write-Host ""
Write-Host "3. Checking Docker Configuration..." -ForegroundColor Yellow
$dockerFiles = @(
    "docker-compose.yml",
    "docker-compose.swarm.yml",
    "src/Presentation/ServerMonitoring.API/Dockerfile",
    "ServerMonitoring.Web/Dockerfile"
)

foreach ($file in $dockerFiles) {
    if (Test-Path $file) {
        Write-Host "   ✓ $file" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $file MISSING" -ForegroundColor Red
        $allGood = $false
    }
}

# Check AWS files
Write-Host ""
Write-Host "4. Checking AWS Deployment Files..." -ForegroundColor Yellow
$awsFiles = @(
    "aws/cloudformation/ecs-cluster.yml",
    "aws/ecs/api-task-definition.json",
    "aws/ecs/web-task-definition.json",
    "aws/ecs/api-service.json",
    "aws/ecs/web-service.json",
    "push-to-ecr.ps1",
    "deploy-to-ecs.ps1"
)

foreach ($file in $awsFiles) {
    if (Test-Path $file) {
        Write-Host "   ✓ $file" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $file MISSING" -ForegroundColor Red
        $allGood = $false
    }
}

# Check CI/CD
Write-Host ""
Write-Host "5. Checking CI/CD Configuration..." -ForegroundColor Yellow
$cicdFiles = @(
    ".github/workflows/ci-cd.yml",
    ".github/workflows/deploy-aws.yml",
    ".github/workflows/rollback.yml"
)

foreach ($file in $cicdFiles) {
    if (Test-Path $file) {
        Write-Host "   ✓ $file" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $file MISSING" -ForegroundColor Red
        $allGood = $false
    }
}

# Check solution file
Write-Host ""
Write-Host "6. Checking Solution File..." -ForegroundColor Yellow
if (Test-Path "ServerMonitoring.sln") {
    Write-Host "   ✓ ServerMonitoring.sln" -ForegroundColor Green
} else {
    Write-Host "   ✗ ServerMonitoring.sln MISSING" -ForegroundColor Red
    $allGood = $false
}

# Check .gitignore
Write-Host ""
Write-Host "7. Checking .gitignore..." -ForegroundColor Yellow
if (Test-Path ".gitignore") {
    Write-Host "   ✓ .gitignore exists" -ForegroundColor Green
} else {
    Write-Host "   ✗ .gitignore MISSING" -ForegroundColor Red
    $allGood = $false
}

# Test dotnet build
Write-Host ""
Write-Host "8. Testing .NET Build..." -ForegroundColor Yellow
try {
    $buildOutput = dotnet build ServerMonitoring.sln --no-restore -v quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Solution builds successfully" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Build failed" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "   ⚠ Unable to test build (dotnet not found or restore needed)" -ForegroundColor Yellow
}

# Test Docker Compose syntax
Write-Host ""
Write-Host "9. Validating Docker Compose..." -ForegroundColor Yellow
try {
    docker compose config > $null 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ docker-compose.yml is valid" -ForegroundColor Green
    } else {
        Write-Host "   ✗ docker-compose.yml has errors" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "   ⚠ Unable to validate (Docker not running)" -ForegroundColor Yellow
}

# Check file sizes (no huge files)
Write-Host ""
Write-Host "10. Checking for Large Files..." -ForegroundColor Yellow
$largeFiles = Get-ChildItem -Recurse -File -ErrorAction SilentlyContinue | 
    Where-Object { $_.Length -gt 50MB -and $_.FullName -notmatch "node_modules|obj|bin" } |
    Select-Object -First 5

if ($largeFiles.Count -gt 0) {
    Write-Host "   ⚠ Warning: Large files found (may slow down Git)" -ForegroundColor Yellow
    foreach ($file in $largeFiles) {
        $sizeMB = [math]::Round($file.Length / 1MB, 2)
        Write-Host "     - $($file.Name) ($sizeMB MB)" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✓ No problematic large files" -ForegroundColor Green
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

if ($allGood) {
    Write-Host "✅ ALL CHECKS PASSED!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 You are ready to submit!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Test locally: .\test-local.ps1" -ForegroundColor White
    Write-Host "2. Initialize Git: git init" -ForegroundColor White
    Write-Host "3. Add files: git add ." -ForegroundColor White
    Write-Host "4. Commit: git commit -m 'Complete implementation'" -ForegroundColor White
    Write-Host "5. Create GitHub repo (public)" -ForegroundColor White
    Write-Host "6. Push: git remote add origin [URL] && git push -u origin main" -ForegroundColor White
    Write-Host "7. Send repository link via email" -ForegroundColor White
    Write-Host ""
    Write-Host "📚 See SUBMISSION_GUIDE.md for detailed instructions" -ForegroundColor Cyan
} else {
    Write-Host "⚠️ SOME CHECKS FAILED" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please fix the issues above before submitting." -ForegroundColor Yellow
    Write-Host "Some files may be missing or have errors." -ForegroundColor Yellow
}

Write-Host ""
