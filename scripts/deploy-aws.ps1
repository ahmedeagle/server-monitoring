#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Quick AWS ECS deployment script
.DESCRIPTION
    Deploys Server Monitoring System to AWS ECS Fargate in one command
#>

param(
    [string]$Region = "us-east-1",
    [string]$Environment = "production",
    [string]$ClusterName = "servermonitoring-cluster"
)

$ErrorActionPreference = "Stop"

Write-Host "AWS ECS Quick Deployment" -ForegroundColor Cyan

Write-Host ""

# Step 1: Verify AWS CLI is installed and configured
Write-Host "Step 1/6: Verifying AWS CLI..." -ForegroundColor Yellow
if (-not (Get-Command aws -ErrorAction SilentlyContinue)) {
    Write-Host "[X] AWS CLI not found. Please install: https://aws.amazon.com/cli/" -ForegroundColor Red
    exit 1
}

try {
    $accountId = (aws sts get-caller-identity --query Account --output text 2>&1)
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] AWS CLI not configured. Run: aws configure" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] AWS Account: $accountId" -ForegroundColor Green
} catch {
    Write-Host "[X] Error checking AWS credentials" -ForegroundColor Red
    exit 1
}

# Step 2: Create ECR repositories
Write-Host "`nStep 2/6: Creating ECR repositories..." -ForegroundColor Yellow

$repositories = @("servermonitoring-api", "servermonitoring-web")
foreach ($repo in $repositories) {
    Write-Host "  Checking $repo..." -ForegroundColor Gray
    
    # Check if repository exists
    try {
        $ErrorActionPreference = "Continue"
        & aws ecr describe-repositories --repository-names $repo --region $Region 2>&1 | Out-Null
        $repoExists = $LASTEXITCODE -eq 0
        $ErrorActionPreference = "Stop"
    } catch {
        $repoExists = $false
    }
    
    if (-not $repoExists) {
        # Repository doesn't exist, create it
        Write-Host "  Creating $repo..." -ForegroundColor Gray
        try {
            $ErrorActionPreference = "Continue"
            & aws ecr create-repository --repository-name $repo --region $Region 2>&1 | Out-Null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "    [OK] Created $repo" -ForegroundColor Green
            } else {
                Write-Host "    [X] Failed to create $repo" -ForegroundColor Red
            }
            $ErrorActionPreference = "Stop"
        } catch {
            Write-Host "    [X] Error creating $repo" -ForegroundColor Red
        }
    } else {
        Write-Host "    [OK] $repo already exists" -ForegroundColor Green
    }
}

# Step 3: Build and push Docker images
Write-Host "`nStep 3/6: Building and pushing Docker images..." -ForegroundColor Yellow

# Login to ECR
Write-Host "  Logging into ECR..." -ForegroundColor Gray
& aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin "$accountId.dkr.ecr.$Region.amazonaws.com"

if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] Failed to login to ECR" -ForegroundColor Red
    exit 1
}

# Build and push API
Write-Host "  Building API image..." -ForegroundColor Gray
docker build -t servermonitoring-api -f src/Presentation/ServerMonitoring.API/Dockerfile .
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] Failed to build API image" -ForegroundColor Red
    exit 1
}
docker tag servermonitoring-api:latest "$accountId.dkr.ecr.$Region.amazonaws.com/servermonitoring-api:latest"
docker push "$accountId.dkr.ecr.$Region.amazonaws.com/servermonitoring-api:latest"
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] Failed to push API image" -ForegroundColor Red
    exit 1
}
Write-Host "    [OK] API image pushed" -ForegroundColor Green

# Build and push Web
Write-Host "  Building Web image..." -ForegroundColor Gray
docker build -t servermonitoring-web -f ServerMonitoring.Web/Dockerfile ServerMonitoring.Web
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] Failed to build Web image" -ForegroundColor Red
    exit 1
}
docker tag servermonitoring-web:latest "$accountId.dkr.ecr.$Region.amazonaws.com/servermonitoring-web:latest"
docker push "$accountId.dkr.ecr.$Region.amazonaws.com/servermonitoring-web:latest"
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] Failed to push Web image" -ForegroundColor Red
    exit 1
}
Write-Host "    [OK] Web image pushed" -ForegroundColor Green

# Step 4: Create secrets
Write-Host "`nStep 4/6: Setting up secrets..." -ForegroundColor Yellow

$jwtSecret = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString() + (New-Guid).ToString()))

Write-Host "  Creating JWT secret..." -ForegroundColor Gray
$ErrorActionPreference = "Continue"
& aws secretsmanager create-secret --name "servermonitoring/$Environment/jwt-secret" --description "JWT secret key" --secret-string $jwtSecret --region $Region 2>&1 | Out-Null
$ErrorActionPreference = "Stop"

if ($LASTEXITCODE -ne 0) {
    Write-Host "    [!] Secret may already exist (this is OK)" -ForegroundColor Yellow
}

Write-Host "  Creating DB connection secret..." -ForegroundColor Gray
$dbConnection = "Server=servermonitoring-sqlserver,1433;Database=ServerMonitoring;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
$ErrorActionPreference = "Continue"
& aws secretsmanager create-secret --name "servermonitoring/$Environment/db-connection" --description "Database connection string" --secret-string $dbConnection --region $Region 2>&1 | Out-Null
$ErrorActionPreference = "Stop"

if ($LASTEXITCODE -ne 0) {
    Write-Host "    [!] Secret may already exist (this is OK)" -ForegroundColor Yellow
}

Write-Host "[OK] Secrets configured" -ForegroundColor Green

# Step 5: Create ECS cluster
Write-Host "`nStep 5/6: Creating ECS cluster..." -ForegroundColor Yellow

$ErrorActionPreference = "Continue"
& aws ecs describe-clusters --clusters $ClusterName --region $Region 2>&1 | Out-Null
$clusterExists = $LASTEXITCODE -eq 0
$ErrorActionPreference = "Stop"

if (-not $clusterExists) {
    & aws ecs create-cluster --cluster-name $ClusterName --region $Region 2>&1 | Out-Null
    Write-Host "[OK] Cluster created: $ClusterName" -ForegroundColor Green
} else {
    Write-Host "[OK] Cluster exists: $ClusterName" -ForegroundColor Green
}

# Step 6: Create task execution role
Write-Host "`nStep 6/6: Creating IAM roles..." -ForegroundColor Yellow

$executionRoleName = "ecsTaskExecutionRole-ServerMonitoring"
$taskRoleName = "ecsTaskRole-ServerMonitoring"

# Check if execution role exists
$ErrorActionPreference = "Continue"
$executionRoleArn = & aws iam get-role --role-name $executionRoleName --query 'Role.Arn' --output text 2>&1
$roleExists = $LASTEXITCODE -eq 0
$ErrorActionPreference = "Stop"

if (-not $roleExists) {
    Write-Host "  Creating execution role..." -ForegroundColor Gray
    
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
    & aws iam create-role --role-name $executionRoleName --assume-role-policy-document file://trust-policy.json 2>&1 | Out-Null
    & aws iam attach-role-policy --role-name $executionRoleName --policy-arn "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy" 2>&1 | Out-Null
    & aws iam attach-role-policy --role-name $executionRoleName --policy-arn "arn:aws:iam::aws:policy/SecretsManagerReadWrite" 2>&1 | Out-Null
    Remove-Item "trust-policy.json" -ErrorAction SilentlyContinue
    
    $executionRoleArn = "arn:aws:iam::${accountId}:role/$executionRoleName"
    Write-Host "    [OK] Execution role created" -ForegroundColor Green
} else {
    Write-Host "  [OK] Execution role exists" -ForegroundColor Green
}

# Check if task role exists
$ErrorActionPreference = "Continue"
$taskRoleArn = & aws iam get-role --role-name $taskRoleName --query 'Role.Arn' --output text 2>&1
$taskRoleExists = $LASTEXITCODE -eq 0
$ErrorActionPreference = "Stop"

if (-not $taskRoleExists) {
    Write-Host "  Creating task role..." -ForegroundColor Gray
    
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
    & aws iam create-role --role-name $taskRoleName --assume-role-policy-document file://trust-policy.json 2>&1 | Out-Null
    Remove-Item "trust-policy.json" -ErrorAction SilentlyContinue
    
    $taskRoleArn = "arn:aws:iam::${accountId}:role/$taskRoleName"
    Write-Host "    [OK] Task role created" -ForegroundColor Green
} else {
    Write-Host "  [OK] Task role exists" -ForegroundColor Green
}

Write-Host ""
Write-Host "DEPLOYMENT COMPLETE!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Go to AWS Console > ECS > Clusters > $ClusterName" -ForegroundColor White
Write-Host "2. Click Create Service or use the AWS Console to:" -ForegroundColor White
Write-Host "   - Deploy the API task definition" -ForegroundColor White
Write-Host "   - Deploy the Web task definition" -ForegroundColor White
Write-Host "   - Configure Load Balancer (optional)" -ForegroundColor White
Write-Host ""
Write-Host "Images pushed to:" -ForegroundColor Cyan
Write-Host "   - $accountId.dkr.ecr.$Region.amazonaws.com/servermonitoring-api:latest" -ForegroundColor Gray
Write-Host "   - $accountId.dkr.ecr.$Region.amazonaws.com/servermonitoring-web:latest" -ForegroundColor Gray
Write-Host ""
Write-Host "Secrets created:" -ForegroundColor Cyan
Write-Host "   - servermonitoring/$Environment/jwt-secret" -ForegroundColor Gray
Write-Host "   - servermonitoring/$Environment/db-connection" -ForegroundColor Gray
Write-Host ""
Write-Host "For complete deployment with ALB and RDS, see: docs/AWS_DEPLOYMENT.md" -ForegroundColor Yellow
Write-Host ""
