# ================================
# START FRONTEND (React)
# ================================

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Starting Frontend on http://localhost:5173" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

cd "c:\Users\lenovo\Downloads\assesment\ServerMonitoring.Web"

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies..." -ForegroundColor Yellow
    npm install
}

# Start the dev server
npm run dev
