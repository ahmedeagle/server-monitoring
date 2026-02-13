# Recreate Web Service with Latest Task Definition
Write-Host "🚀 Creating Web service with latest task definition..." -ForegroundColor Green

# Get latest task definition
$taskDef = (aws ecs describe-task-definition --task-definition servermonitoring-web --query 'taskDefinition.taskDefinitionArn' --output text --no-cli-pager)
Write-Host "Using task definition: $taskDef"

# Get target group ARN
$webTgArn = "arn:aws:elasticloadbalancing:us-east-1:043309367638:targetgroup/servermonitoring-web-tg/f014d77158ddfc13"

# Get security group
$webSg = "sg-0b615be83658998d6"

# Create service
Write-Host "`nCreating service..."
aws ecs create-service `
    --cluster servermonitoring-cluster `
    --service-name servermonitoring-web-service `
    --task-definition $taskDef `
    --desired-count 1 `
    --launch-type FARGATE `
    --health-check-grace-period-seconds 300 `
    --load-balancers "targetGroupArn=$webTgArn,containerName=web,containerPort=80" `
    --network-configuration "awsvpcConfiguration={subnets=[subnet-005aa247cccb3f330,subnet-0fb94c7ba7bfd5f56],securityGroups=[$webSg],assignPublicIp=ENABLED}" `
    --region us-east-1 `
    --no-cli-pager

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Web service created!" -ForegroundColor Green
    Write-Host "`n⏰ Waiting 90 seconds for task to start and register..."
    Start-Sleep -Seconds 90
    
    # Check health
    Write-Host "`nChecking target health..."
    aws elbv2 describe-target-health --target-group-arn $webTgArn --region us-east-1 --query 'TargetHealthDescriptions[0].TargetHealth' --output json --no-cli-pager
    
    Write-Host "`n✅ DONE! Test the endpoints:" -ForegroundColor Green
    Write-Host "Dashboard: http://servermonitoring-alb-1100072309.us-east-1.elb.amazonaws.com/"
    Write-Host "Hangfire: http://servermonitoring-alb-1100072309.us-east-1.elb.amazonaws.com/hangfire"
} else {
    Write-Host "`n❌ Failed to create service!" -ForegroundColor Red
}
