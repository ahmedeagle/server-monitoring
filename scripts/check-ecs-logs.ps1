#!/usr/bin/env pwsh
# Script to check ECS logs for debugging

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("api", "web", "both")]
    [string]$Service = "both",
    
    [Parameter(Mandatory=$false)]
    [int]$Lines = 50,
    
    [Parameter(Mandatory=$false)]
    [string]$Region = "us-east-1"
)

$Cluster = "servermonitoring-cluster"

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "ECS Service Logs Checker" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

function Get-ServiceLogs {
    param($ServiceName, $LogGroup, $DisplayName)
    
    Write-Host "📋 Fetching $DisplayName logs..." -ForegroundColor Yellow
    Write-Host ""
    
    # Get latest log stream
    $streams = aws logs describe-log-streams `
        --log-group-name $LogGroup `
        --region $Region `
        --order-by LastEventTime `
        --descending `
        --max-items 5 `
        --query 'logStreams[*].logStreamName' `
        --output json 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Could not fetch log streams for $LogGroup" -ForegroundColor Red
        Write-Host $streams -ForegroundColor Red
        return
    }
    
    $streamList = $streams | ConvertFrom-Json
    
    if ($streamList.Count -eq 0) {
        Write-Host "⚠️  No log streams found for $LogGroup" -ForegroundColor Yellow
        return
    }
    
    Write-Host "Found $($streamList.Count) recent log streams" -ForegroundColor Green
    Write-Host ""
    
    # Get logs from the most recent stream
    $latestStream = $streamList[0]
    Write-Host "Reading from: $latestStream" -ForegroundColor Cyan
    Write-Host ("=" * 80) -ForegroundColor Gray
    Write-Host ""
    
    $logs = aws logs get-log-events `
        --log-group-name $LogGroup `
        --log-stream-name $latestStream `
        --region $Region `
        --limit $Lines `
        --query 'events[*].[timestamp,message]' `
        --output text 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        $logs | ForEach-Object {
            $line = $_
            # Color code based on content
            if ($line -match "error|exception|fail") {
                Write-Host $line -ForegroundColor Red
            }
            elseif ($line -match "warn") {
                Write-Host $line -ForegroundColor Yellow
            }
            elseif ($line -match "success|✅|started") {
                Write-Host $line -ForegroundColor Green
            }
            else {
                Write-Host $line
            }
        }
    } else {
        Write-Host "❌ Failed to fetch logs" -ForegroundColor Red
        Write-Host $logs -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host ("=" * 80) -ForegroundColor Gray
    Write-Host ""
}

function Get-ServiceStatus {
    param($ServiceName, $DisplayName)
    
    Write-Host "🔍 Checking $DisplayName service status..." -ForegroundColor Cyan
    
    $status = aws ecs describe-services `
        --cluster $Cluster `
        --services $ServiceName `
        --region $Region `
        --query 'services[0].{Status:status,Running:runningCount,Desired:desiredCount,Deployments:deployments[0].{Status:status,Running:runningCount}}' `
        --output json 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        $statusObj = $status | ConvertFrom-Json
        Write-Host "Status: " -NoNewline
        Write-Host $statusObj.Status -ForegroundColor Green
        Write-Host "Running: $($statusObj.Running) / Desired: $($statusObj.Desired)"
        Write-Host ""
    } else {
        Write-Host "❌ Service not found or error" -ForegroundColor Red
        Write-Host ""
    }
}

function Get-TaskErrors {
    param($ServiceName, $DisplayName)
    
    Write-Host "🔍 Checking for stopped tasks (errors)..." -ForegroundColor Cyan
    
    $stoppedTasks = aws ecs list-tasks `
        --cluster $Cluster `
        --service-name $ServiceName `
        --region $Region `
        --desired-status STOPPED `
        --max-items 3 `
        --query 'taskArns' `
        --output json 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        $taskList = $stoppedTasks | ConvertFrom-Json
        
        if ($taskList.Count -gt 0) {
            Write-Host "Found $($taskList.Count) recently stopped tasks" -ForegroundColor Yellow
            
            foreach ($taskArn in $taskList) {
                $taskDetails = aws ecs describe-tasks `
                    --cluster $Cluster `
                    --tasks $taskArn `
                    --region $Region `
                    --query 'tasks[0].{StoppedReason:stoppedReason,StopCode:stopCode,Containers:containers[*].{Name:name,Reason:reason,ExitCode:exitCode}}' `
                    --output json | ConvertFrom-Json
                
                Write-Host ""
                Write-Host "Stopped Reason: " -NoNewline
                Write-Host $taskDetails.StoppedReason -ForegroundColor Red
                Write-Host "Stop Code: " -NoNewline
                Write-Host $taskDetails.StopCode -ForegroundColor Red
                
                foreach ($container in $taskDetails.Containers) {
                    if ($container.ExitCode -ne 0 -or $container.Reason) {
                        Write-Host "  Container: $($container.Name)" -ForegroundColor Yellow
                        Write-Host "  Exit Code: $($container.ExitCode)" -ForegroundColor Red
                        Write-Host "  Reason: $($container.Reason)" -ForegroundColor Red
                    }
                }
            }
        } else {
            Write-Host "✅ No stopped tasks found (good!)" -ForegroundColor Green
        }
    }
    Write-Host ""
}

# Main execution
if ($Service -eq "api" -or $Service -eq "both") {
    Write-Host ""
    Write-Host "🔥 API SERVICE DIAGNOSTICS" -ForegroundColor Magenta
    Write-Host ""
    
    Get-ServiceStatus "servermonitoring-api-service" "API"
    Get-TaskErrors "servermonitoring-api-service" "API"
    Get-ServiceLogs "servermonitoring-api-service" "/ecs/servermonitoring-api" "API"
}

if ($Service -eq "web" -or $Service -eq "both") {
    Write-Host ""
    Write-Host "🌐 WEB SERVICE DIAGNOSTICS" -ForegroundColor Magenta
    Write-Host ""
    
    Get-ServiceStatus "servermonitoring-web-service" "Web"
    Get-TaskErrors "servermonitoring-web-service" "Web"
    Get-ServiceLogs "servermonitoring-web-service" "/ecs/servermonitoring-web" "Web"
}

Write-Host ""
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Quick Commands:" -ForegroundColor Cyan
Write-Host "  API only:     .\scripts\check-ecs-logs.ps1 -Service api" -ForegroundColor Gray
Write-Host "  Web only:     .\scripts\check-ecs-logs.ps1 -Service web" -ForegroundColor Gray
Write-Host "  More lines:   .\scripts\check-ecs-logs.ps1 -Lines 100" -ForegroundColor Gray
Write-Host "  Real-time:    .\scripts\check-ecs-logs.ps1 -Service api -Lines 20" -ForegroundColor Gray
Write-Host "                (then run again to see new logs)" -ForegroundColor Gray
Write-Host "=================================" -ForegroundColor Cyan
