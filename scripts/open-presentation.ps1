#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Opens the project presentation in your default browser
.DESCRIPTION
    Launches index.html to show the interactive architecture overview
#>

$htmlPath = Join-Path $PSScriptRoot "index.html"

if (Test-Path $htmlPath) {
    Write-Host "🎨 Opening presentation..." -ForegroundColor Cyan
    Start-Process $htmlPath
    Write-Host "✅ Presentation opened in your browser!" -ForegroundColor Green
} else {
    Write-Host "❌ index.html not found!" -ForegroundColor Red
}
