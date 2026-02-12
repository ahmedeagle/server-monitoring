#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deploy application to AWS ECS Fargate
.DESCRIPTION
    This script deploys the Server Monitoring application to AWS ECS Fargate.
    It creates/updates ECS services and task definitions.
.PARAMETER Environment
    The environment to deploy to (development, staging, production)
.PARAMETER Region
    AWS region (default: us-east-1)
.PARAMETER ImageTag
    Docker image tag (default: latest)
.EXAMPLE
    .\deploy-to-ecs.ps1 -Environment production -ImageTag v1.0.0
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('development', 'staging', 'production')]
    [string]$Environment = 'development',
    
    [Parameter(Mandatory=$false)]
    [string]$Region = 'us-east-1',
    
    [Parameter(Mandatory=$false)]
    [string]$ImageTag = 'latest'
)

$ErrorActionPreference = 'Stop'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AWS ECS Fargate Deployment Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Region: $Region" -ForegroundColor Yellow
Write-Host "Image Tag: $ImageTag" -ForegroundColor Yellow
Write-Host ""

# Get AWS Account ID
Write-Host "Getting AWS Account ID..." -ForegroundColor Yellow
$AWS_ACCOUNT_ID = (aws sts get-caller-identity --query Account --output text)
Write-Host "✓ AWS Account ID: $AWS_ACCOUNT_ID" -ForegroundColor Green

# Get Stack Outputs
Write-Host "Getting CloudFormation stack outputs..." -ForegroundColor Yellow
$CLUSTER_NAME = "$Environment-servermonitoring-cluster"
$ECS_SECURITY_GROUP = (aws cloudformation describe-stacks --stack-name $CLUSTER_NAME --query "Stacks[0].Outputs[?OutputKey=='ECSSecurityGroupId'].OutputValue" --output text --region $Region)
$PRIVATE_SUBNET_1 = (aws cloudformation describe-stacks --stack-name $CLUSTER_NAME --query "Stacks[0].Outputs[?OutputKey=='PrivateSubnet1Id'].OutputValue" --output text --region $Region)
$PRIVATE_SUBNET_2 = (aws cloudformation describe-stacks --stack-name $CLUSTER_NAME --query "Stacks[0].Outputs[?OutputKey=='PrivateSubnet2Id'].OutputValue" --output text --region $Region)
$API_TARGET_GROUP_ARN = (aws cloudformation describe-stacks --stack-name $CLUSTER_NAME --query "Stacks[0].Outputs[?OutputKey=='APITargetGroupArn'].OutputValue" --output text --region $Region)
$WEB_TARGET_GROUP_ARN = (aws cloudformation describe-stacks --stack-name $CLUSTER_NAME --query "Stacks[0].Outputs[?OutputKey=='WebTargetGroupArn'].OutputValue" --output text --region $Region)
$TASK_EXECUTION_ROLE = (aws cloudformation describe-stacks --stack-name $CLUSTER_NAME --query "Stacks[0].Outputs[?OutputKey=='ECSTaskExecutionRoleArn'].OutputValue" --output text --region $Region)
$TASK_ROLE = (aws cloudformation describe-stacks --stack-name $CLUSTER_NAME --query "Stacks[0].Outputs[?OutputKey=='ECSTaskRoleArn'].OutputValue" --output text --region $Region)
$ALB_DNS = (aws cloudformation describe-stacks --stack-name $CLUSTER_NAME --query "Stacks[0].Outputs[?OutputKey=='ALBDNSName'].OutputValue" --output text --region $Region)

Write-Host "✓ Stack outputs retrieved" -ForegroundColor Green
Write-Host "  Cluster: $CLUSTER_NAME" -ForegroundColor White
Write-Host "  ALB DNS: $ALB_DNS" -ForegroundColor White

# Function to replace variables in task definition
function Update-TaskDefinition {
    param(
        [string]$TemplateFile,
        [string]$OutputFile
    )
    
    $content = Get-Content $TemplateFile -Raw
    
    $content = $content -replace '\$\{AWS_ACCOUNT_ID\}', $AWS_ACCOUNT_ID
    $content = $content -replace '\$\{AWS_REGION\}', $Region
    $content = $content -replace '\$\{ENVIRONMENT\}', $Environment
    $content = $content -replace '\$\{IMAGE_TAG\}', $ImageTag
    $content = $content -replace '\$\{ECS_TASK_EXECUTION_ROLE_ARN\}', $TASK_EXECUTION_ROLE
    $content = $content -replace '\$\{ECS_TASK_ROLE_ARN\}', $TASK_ROLE
    $content = $content -replace '\$\{API_URL\}', "http://$ALB_DNS"
    
    $content | Set-Content $OutputFile
}

# Register API Task Definition
Write-Host ""
Write-Host "Registering API task definition..." -ForegroundColor Yellow
$API_TASK_DEF_TEMP = "aws/ecs/api-task-definition-temp.json"
Update-TaskDefinition -TemplateFile "aws/ecs/api-task-definition.json" -OutputFile $API_TASK_DEF_TEMP

$API_TASK_DEF_ARN = (aws ecs register-task-definition --cli-input-json file://$API_TASK_DEF_TEMP --region $Region --query 'taskDefinition.taskDefinitionArn' --output text)
Remove-Item $API_TASK_DEF_TEMP
Write-Host "✓ API task definition registered: $API_TASK_DEF_ARN" -ForegroundColor Green

# Register Web Task Definition
Write-Host ""
Write-Host "Registering Web task definition..." -ForegroundColor Yellow
$WEB_TASK_DEF_TEMP = "aws/ecs/web-task-definition-temp.json"
Update-TaskDefinition -TemplateFile "aws/ecs/web-task-definition.json" -OutputFile $WEB_TASK_DEF_TEMP

$WEB_TASK_DEF_ARN = (aws ecs register-task-definition --cli-input-json file://$WEB_TASK_DEF_TEMP --region $Region --query 'taskDefinition.taskDefinitionArn' --output text)
Remove-Item $WEB_TASK_DEF_TEMP
Write-Host "✓ Web task definition registered: $WEB_TASK_DEF_ARN" -ForegroundColor Green

# Function to create or update service
function Deploy-ECSService {
    param(
        [string]$ServiceName,
        [string]$TaskDefinitionArn,
        [string]$TargetGroupArn,
        [int]$ContainerPort
    )
    
    Write-Host ""
    Write-Host "Checking if service '$ServiceName' exists..." -ForegroundColor Yellow
    
    $serviceExists = aws ecs describe-services --cluster $CLUSTER_NAME --services $ServiceName --region $Region --query 'services[0].status' --output text 2>$null
    
    if ($serviceExists -eq "ACTIVE") {
        Write-Host "Service exists, updating..." -ForegroundColor Yellow
        aws ecs update-service `
            --cluster $CLUSTER_NAME `
            --service $ServiceName `
            --task-definition $TaskDefinitionArn `
            --force-new-deployment `
            --region $Region `
            --output json | Out-Null
        Write-Host "✓ Service '$ServiceName' updated" -ForegroundColor Green
    } else {
        Write-Host "Service does not exist, creating..." -ForegroundColor Yellow
        aws ecs create-service `
            --cluster $CLUSTER_NAME `
            --service-name $ServiceName `
            --task-definition $TaskDefinitionArn `
            --desired-count 2 `
            --launch-type FARGATE `
            --platform-version LATEST `
            --network-configuration "awsvpcConfiguration={subnets=[$PRIVATE_SUBNET_1,$PRIVATE_SUBNET_2],securityGroups=[$ECS_SECURITY_GROUP],assignPublicIp=DISABLED}" `
            --load-balancers "targetGroupArn=$TargetGroupArn,containerName=$ServiceName,containerPort=$ContainerPort" `
            --health-check-grace-period-seconds 60 `
            --deployment-configuration "maximumPercent=200,minimumHealthyPercent=100,deploymentCircuitBreaker={enable=true,rollback=true}" `
            --enable-ecs-managed-tags `
            --propagate-tags SERVICE `
            --tags "key=Environment,value=$Environment" `
            --region $Region `
            --output json | Out-Null
        Write-Host "✓ Service '$ServiceName' created" -ForegroundColor Green
    }
}

# Deploy API Service
Deploy-ECSService `
    -ServiceName "servermonitoring-api-service" `
    -TaskDefinitionArn $API_TASK_DEF_ARN `
    -TargetGroupArn $API_TARGET_GROUP_ARN `
    -ContainerPort 8080

# Deploy Web Service
Deploy-ECSService `
    -ServiceName "servermonitoring-web-service" `
    -TaskDefinitionArn $WEB_TASK_DEF_ARN `
    -TargetGroupArn $WEB_TARGET_GROUP_ARN `
    -ContainerPort 80

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ Deployment Completed Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Application URL: http://$ALB_DNS" -ForegroundColor Cyan
Write-Host "API URL: http://$ALB_DNS/api" -ForegroundColor Cyan
Write-Host "Swagger: http://$ALB_DNS/swagger" -ForegroundColor Cyan
Write-Host "Hangfire: http://$ALB_DNS/hangfire" -ForegroundColor Cyan
Write-Host ""
Write-Host "ECS Cluster: $CLUSTER_NAME" -ForegroundColor Yellow
Write-Host "Region: $Region" -ForegroundColor Yellow
Write-Host ""
Write-Host "Monitor deployment:" -ForegroundColor Yellow
Write-Host "  aws ecs describe-services --cluster $CLUSTER_NAME --services servermonitoring-api-service --region $Region" -ForegroundColor White
Write-Host "  aws ecs describe-services --cluster $CLUSTER_NAME --services servermonitoring-web-service --region $Region" -ForegroundColor White
Write-Host ""
Write-Host "View logs:" -ForegroundColor Yellow
Write-Host "  aws logs tail /ecs/$Environment/servermonitoring-api --follow --region $Region" -ForegroundColor White
Write-Host "  aws logs tail /ecs/$Environment/servermonitoring-web --follow --region $Region" -ForegroundColor White
Write-Host ""
