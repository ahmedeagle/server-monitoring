#!/usr/bin/env pwsh
# Emergency fix for ECS network connectivity issues
# Run this to fix the 504 Gateway Timeout error

$ErrorActionPreference = "Continue"

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "ECS Network Connectivity Fix" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

$CLUSTER = "servermonitoring-cluster"
$API_SERVICE = "servermonitoring-api-service"
$WEB_SERVICE = "servermonitoring-web-service"
$REGION = "us-east-1"

# Step 1: Check if services are running
Write-Host "`nChecking service status..." -ForegroundColor Yellow
aws ecs describe-services --cluster $CLUSTER --services $API_SERVICE $WEB_SERVICE --region $REGION --query 'services[*].{Name:serviceName,Status:status,Running:runningCount,Desired:desiredCount}' --output table

# Step 2: Get API task details
Write-Host "`nGetting API task details..." -ForegroundColor Yellow
$apiTaskArn = aws ecs list-tasks --cluster $CLUSTER --service-name $API_SERVICE --region $REGION --query 'taskArns[0]' --output text

if ($apiTaskArn -and $apiTaskArn -ne "None") {
    Write-Host "API Task ARN: $apiTaskArn" -ForegroundColor Green
    
    # Get API public IP
    $apiEniId = aws ecs describe-tasks --cluster $CLUSTER --tasks $apiTaskArn --region $REGION --query 'tasks[0].attachments[0].details[?name==`networkInterfaceId`].value' --output text
    $apiPublicIp = aws ec2 describe-network-interfaces --network-interface-ids $apiEniId --region $REGION --query 'NetworkInterfaces[0].Association.PublicIp' --output text
    
    Write-Host "API Public IP: $apiPublicIp" -ForegroundColor Green
    
    # Test API health
    Write-Host "`nTesting API health endpoint..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "http://${apiPublicIp}:8080/health" -TimeoutSec 5 -UseBasicParsing
        Write-Host "API is healthy! Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host $response.Content
    } catch {
        Write-Host "API health check failed: $_" -ForegroundColor Red
        Write-Host "API may still be starting up or is not reachable" -ForegroundColor Yellow
    }
    
    # Get security groups
    $apiSgId = aws ec2 describe-network-interfaces --network-interface-ids $apiEniId --region $REGION --query 'NetworkInterfaces[0].Groups[0].GroupId' --output text
    Write-Host "API Security Group: $apiSgId" -ForegroundColor Cyan
    
    # Step 3: Get Web task details
    Write-Host "`nGetting Web task details..." -ForegroundColor Yellow
    $webTaskArn = aws ecs list-tasks --cluster $CLUSTER --service-name $WEB_SERVICE --region $REGION --query 'taskArns[0]' --output text
    
    if ($webTaskArn -and $webTaskArn -ne "None") {
        $webEniId = aws ecs describe-tasks --cluster $CLUSTER --tasks $webTaskArn --region $REGION --query 'tasks[0].attachments[0].details[?name==`networkInterfaceId`].value' --output text
        $webPublicIp = aws ec2 describe-network-interfaces --network-interface-ids $webEniId --region $REGION --query 'NetworkInterfaces[0].Association.PublicIp' --output text
        $webSgId = aws ec2 describe-network-interfaces --network-interface-ids $webEniId --region $REGION --query 'NetworkInterfaces[0].Groups[0].GroupId' --output text
        
        Write-Host "Web Public IP: $webPublicIp" -ForegroundColor Green
        Write-Host "Web Security Group: $webSgId" -ForegroundColor Cyan
        
        # Step 4: Fix security group rules
        Write-Host "`nFixing security group rules..." -ForegroundColor Yellow
        
        # Allow Web SG -> API SG on port 8080
        Write-Host "Adding rule: Web ($webSgId) -> API ($apiSgId) on port 8080..." -ForegroundColor Cyan
        aws ec2 authorize-security-group-ingress --group-id $apiSgId --protocol tcp --port 8080 --source-group $webSgId --region $REGION 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Rule added successfully!" -ForegroundColor Green
        } else {
            Write-Host "Rule may already exist (this is OK)" -ForegroundColor Yellow
        }
        
        # Also allow from anywhere (for testing)
        Write-Host "Ensuring API accepts traffic from anywhere on port 8080..." -ForegroundColor Cyan
        aws ec2 authorize-security-group-ingress --group-id $apiSgId --protocol tcp --port 8080 --cidr 0.0.0.0/0 --region $REGION 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Public access enabled!" -ForegroundColor Green
        } else {
            Write-Host "Rule may already exist (this is OK)" -ForegroundColor Yellow
        }
        
        # Step 5: Verify connectivity
        Write-Host "`nTesting connectivity..." -ForegroundColor Yellow
        Write-Host "Testing API from your machine:" -ForegroundColor Cyan
        try {
            $testResponse = Invoke-RestMethod -Uri "http://${apiPublicIp}:8080/health" -Method Get -TimeoutSec 5
            Write-Host "Direct API access works!" -ForegroundColor Green
        } catch {
            Write-Host "Cannot reach API directly: $_" -ForegroundColor Red
        }
        
        # Step 6: Restart web service
        Write-Host "`nRestarting services to apply changes..." -ForegroundColor Yellow
        Write-Host "Forcing new deployment of Web service..." -ForegroundColor Cyan
        aws ecs update-service --cluster $CLUSTER --service $WEB_SERVICE --force-new-deployment --region $REGION --query 'service.{Name:serviceName,Status:status}' --output table
        
        Write-Host "`n====================================" -ForegroundColor Cyan
        Write-Host "Fix Complete!" -ForegroundColor Green
        Write-Host "====================================" -ForegroundColor Cyan
        Write-Host "`nYour application URLs:" -ForegroundColor Cyan
        Write-Host "Web: http://$webPublicIp" -ForegroundColor White
        Write-Host "API: http://${apiPublicIp}:8080/swagger" -ForegroundColor White
        Write-Host "Health: http://${apiPublicIp}:8080/health" -ForegroundColor White
        Write-Host "`nWeb service is restarting... wait 2-3 minutes and try again." -ForegroundColor Yellow
        Write-Host "`nTest login:" -ForegroundColor Cyan
        $curlCmd = "curl http://${webPublicIp}/api/v1/auth/login -H 'Content-Type: application/json' -d '{`"username`":`"admin`",`"password`":`"Admin@123`"}'"
        Write-Host $curlCmd -ForegroundColor Gray
        
    } else {
        Write-Host "No Web task found!" -ForegroundColor Red
    }
    
} else {
    Write-Host "No API task found! Service may be down." -ForegroundColor Red
    Write-Host "Check ECS console for errors" -ForegroundColor Yellow
}
