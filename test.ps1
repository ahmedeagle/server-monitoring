#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Quick launcher for test-local script
.DESCRIPTION
    Convenience script that runs the main test script from scripts folder
#>

Write-Host "Launching test-local.ps1..." -ForegroundColor Cyan
& "$PSScriptRoot\scripts\test-local.ps1" @args
