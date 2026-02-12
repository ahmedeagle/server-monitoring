# ================================
# QUICK START - Full Application
# ================================

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Server Monitoring System - Quick Start" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "This will open 2 terminals:" -ForegroundColor Yellow
Write-Host "  1. Backend API (http://localhost:5000)" -ForegroundColor White
Write-Host "  2. Frontend React (http://localhost:5173)" -ForegroundColor White
Write-Host ""

$baseDir = "c:\Users\lenovo\Downloads\assesment"

# Terminal 1: Start Backend
Write-Host "Opening Terminal 1: Starting Backend API..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir'; .\scripts\start-backend.ps1"

Start-Sleep -Seconds 3

# Terminal 2: Start Frontend
Write-Host "Opening Terminal 2: Starting Frontend..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir'; .\scripts\start-frontend.ps1"

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "All services starting!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Wait 30 seconds then access:" -ForegroundColor Yellow
Write-Host "  Backend Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "  Frontend App:    http://localhost:5173" -ForegroundColor Cyan
Write-Host ""
Write-Host "Login Credentials:" -ForegroundColor Yellow
Write-Host "  Username: admin" -ForegroundColor White
Write-Host "  Password: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C in each terminal to stop services" -ForegroundColor Gray
