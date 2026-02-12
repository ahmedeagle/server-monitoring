#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Setup AWS resources for GitHub Actions deployment
.DESCRIPTION
    Creates ECR repos, ECS cluster, and IAM roles needed for GitHub Actions CI/CD
#>

param(
    [string]$Region = "us-east-1"
)

$ErrorActionPreference = "Stop"

Write-Host "AWS Setup for GitHub Actions Deployment" -ForegroundColor Cyan
Write-Host ""

# Get AWS Account ID
Write-Host "Getting AWS Account ID..." -ForegroundColor Yellow
try {
    $accountId = aws sts get-caller-identity --query Account --output text
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] AWS CLI not configured. Run: aws configure" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] AWS Account: $accountId" -ForegroundColor Green
} catch {
    Write-Host "[X] Error getting AWS account info. Run: aws configure" -ForegroundColor Red
    exit 1
}

# Create ECR repositories
Write-Host "`nCreating ECR repositories..." -ForegroundColor Yellow
$repos = @("servermonitoring-api", "servermonitoring-web")
foreach ($repo in $repos) {
    try {
        $ErrorActionPreference = "SilentlyContinue"
        aws ecr describe-repositories --repository-names $repo --region $Region 2>&1 | Out-Null
        $exists = $LASTEXITCODE -eq 0
        $ErrorActionPreference = "Stop"
    } catch {
        $exists = $false
    }
    
    if (-not $exists) {
        aws ecr create-repository --repository-name $repo --region $Region 2>&1 | Out-Null
        Write-Host "[OK] Created $repo" -ForegroundColor Green
    } else {
        Write-Host "[OK] $repo already exists" -ForegroundColor Green
    }
}

# Create trust policy file
$trustPolicy = @"
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "ecs-tasks.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
"@

$trustPolicy | Out-File -FilePath "trust-policy.json" -Encoding UTF8

# Create ECS cluster
Write-Host "`nCreating ECS cluster..." -ForegroundColor Yellow
try {
    $ErrorActionPreference = "SilentlyContinue"
    $clusterCheck = aws ecs describe-clusters --clusters servermonitoring-cluster --region $Region --query 'clusters[0].status' --output text 2>&1
    $clusterExists = ($LASTEXITCODE -eq 0 -and $clusterCheck -eq "ACTIVE")
    $ErrorActionPreference = "Stop"
} catch {
    $clusterExists = $false
}

if (-not $clusterExists) {
    Write-Host "  Creating cluster..." -ForegroundColor Gray
    $result = aws ecs create-cluster --cluster-name servermonitoring-cluster --region $Region 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Cluster created" -ForegroundColor Green
    } else {
        Write-Host "[X] Failed to create cluster" -ForegroundColor Red
        Write-Host "    $result" -ForegroundColor Gray
    }
} else {
    Write-Host "[OK] Cluster already exists" -ForegroundColor Green
}

# Create IAM execution role
Write-Host "`nCreating IAM roles..." -ForegroundColor Yellow
$executionRole = "ecsTaskExecutionRole-ServerMonitoring"

Write-Host "  Checking if execution role exists..." -ForegroundColor Gray
try {
    $ErrorActionPreference = "SilentlyContinue"
    $roleCheck = aws iam get-role --role-name $executionRole --query 'Role.RoleName' --output text 2>&1
    $roleExists = ($LASTEXITCODE -eq 0)
    $ErrorActionPreference = "Stop"
} catch {
    $roleExists = $false
}

if (-not $roleExists) {
    Write-Host "  Creating execution role..." -ForegroundColor Gray
    try {
        aws iam create-role --role-name $executionRole --assume-role-policy-document file://trust-policy.json | Out-Null
        aws iam attach-role-policy --role-name $executionRole --policy-arn "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy" | Out-Null
        aws iam attach-role-policy --role-name $executionRole --policy-arn "arn:aws:iam::aws:policy/CloudWatchLogsFullAccess" | Out-Null
        Write-Host "[OK] Execution role created" -ForegroundColor Green
    } catch {
        Write-Host "[!] Execution role may already exist or error occurred" -ForegroundColor Yellow
    }
} else {
    Write-Host "[OK] Execution role already exists" -ForegroundColor Green
}

# Create IAM task role
$taskRole = "ecsTaskRole-ServerMonitoring"

Write-Host "  Checking if task role exists..." -ForegroundColor Gray
try {
    $ErrorActionPreference = "SilentlyContinue"
    $taskRoleCheck = aws iam get-role --role-name $taskRole --query 'Role.RoleName' --output text 2>&1
    $taskRoleExists = ($LASTEXITCODE -eq 0)
    $ErrorActionPreference = "Stop"
} catch {
    $taskRoleExists = $false
}

if (-not $taskRoleExists) {
    Write-Host "  Creating task role..." -ForegroundColor Gray
    try {
        aws iam create-role --role-name $taskRole --assume-role-policy-document file://trust-policy.json | Out-Null
        Write-Host "[OK] Task role created" -ForegroundColor Green
    } catch {
        Write-Host "[!] Task role may already exist or error occurred" -ForegroundColor Yellow
    }
} else {
    Write-Host "[OK] Task role already exists" -ForegroundColor Green
}

Remove-Item "trust-policy.json" -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "SETUP COMPLETE!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Push your code to GitHub" -ForegroundColor White
Write-Host "2. Add these secrets to GitHub (Settings > Secrets > Actions):" -ForegroundColor White
Write-Host "   - AWS_ACCESS_KEY_ID: YOUR_AWS_ACCESS_KEY" -ForegroundColor Gray
Write-Host "   - AWS_SECRET_ACCESS_KEY: YOUR_AWS_SECRET_KEY" -ForegroundColor Gray
Write-Host "   - AWS_ACCOUNT_ID: $accountId" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Push to main branch or manually trigger workflow in GitHub Actions" -ForegroundColor White
Write-Host ""
Write-Host "Resources Created:" -ForegroundColor Cyan
Write-Host "  - ECR: $accountId.dkr.ecr.$Region.amazonaws.com/servermonitoring-api" -ForegroundColor Gray
Write-Host "  - ECR: $accountId.dkr.ecr.$Region.amazonaws.com/servermonitoring-web" -ForegroundColor Gray
Write-Host "  - ECS Cluster: servermonitoring-cluster" -ForegroundColor Gray
Write-Host "  - IAM Roles: ecsTaskExecutionRole-ServerMonitoring, ecsTaskRole-ServerMonitoring" -ForegroundColor Gray
Write-Host ""
Write-Host "See docs/GITHUB_DEPLOYMENT_GUIDE.md for complete instructions" -ForegroundColor Yellow
Write-Host ""
