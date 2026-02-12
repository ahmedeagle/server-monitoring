#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and push Docker images to AWS ECR
.DESCRIPTION
    This script builds the API and Web Docker images and pushes them to AWS ECR.
    Requires AWS CLI and Docker to be installed and configured.
.PARAMETER Environment
    The environment to deploy to (development, staging, production)
.PARAMETER Region
    AWS region (default: us-east-1)
.PARAMETER ImageTag
    Docker image tag (default: latest)
.EXAMPLE
    .\push-to-ecr.ps1 -Environment production -ImageTag v1.0.0
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
Write-Host "AWS ECR Docker Image Push Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if AWS CLI is installed
Write-Host "Checking AWS CLI installation..." -ForegroundColor Yellow
try {
    $awsVersion = aws --version
    Write-Host "✓ AWS CLI found: $awsVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ AWS CLI not found. Please install AWS CLI first." -ForegroundColor Red
    Write-Host "  Download from: https://aws.amazon.com/cli/" -ForegroundColor Yellow
    exit 1
}

# Check if Docker is running
Write-Host "Checking Docker installation..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version
    Write-Host "✓ Docker found: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker not found or not running." -ForegroundColor Red
    exit 1
}

# Get AWS Account ID
Write-Host "Getting AWS Account ID..." -ForegroundColor Yellow
try {
    $AWS_ACCOUNT_ID = (aws sts get-caller-identity --query Account --output text)
    Write-Host "✓ AWS Account ID: $AWS_ACCOUNT_ID" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to get AWS Account ID. Please configure AWS CLI credentials." -ForegroundColor Red
    Write-Host "  Run: aws configure" -ForegroundColor Yellow
    exit 1
}

$API_REPO_NAME = "servermonitoring-api"
$WEB_REPO_NAME = "servermonitoring-web"
$ECR_REGISTRY = "$AWS_ACCOUNT_ID.dkr.ecr.$Region.amazonaws.com"

# Function to create ECR repository if it doesn't exist
function Ensure-ECRRepository {
    param([string]$RepoName)
    
    Write-Host "Checking if ECR repository '$RepoName' exists..." -ForegroundColor Yellow
    
    $repoExists = aws ecr describe-repositories --repository-names $RepoName --region $Region 2>$null
    
    if (-not $repoExists) {
        Write-Host "Creating ECR repository '$RepoName'..." -ForegroundColor Yellow
        aws ecr create-repository `
            --repository-name $RepoName `
            --region $Region `
            --image-scanning-configuration scanOnPush=true `
            --encryption-configuration encryptionType=AES256
        
        Write-Host "✓ Repository '$RepoName' created successfully" -ForegroundColor Green
        
        # Set lifecycle policy to keep only last 10 images
        $lifecyclePolicy = @"
{
  "rules": [
    {
      "rulePriority": 1,
      "description": "Keep only last 10 images",
      "selection": {
        "tagStatus": "any",
        "countType": "imageCountMoreThan",
        "countNumber": 10
      },
      "action": {
        "type": "expire"
      }
    }
  ]
}
"@
        
        $lifecyclePolicy | aws ecr put-lifecycle-policy `
            --repository-name $RepoName `
            --region $Region `
            --lifecycle-policy-text file:///dev/stdin
        
        Write-Host "✓ Lifecycle policy applied" -ForegroundColor Green
    } else {
        Write-Host "✓ Repository '$RepoName' already exists" -ForegroundColor Green
    }
}

# Login to ECR
Write-Host ""
Write-Host "Logging in to AWS ECR..." -ForegroundColor Yellow
aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin $ECR_REGISTRY

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to login to ECR" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Successfully logged in to ECR" -ForegroundColor Green

# Ensure ECR repositories exist
Ensure-ECRRepository -RepoName $API_REPO_NAME
Ensure-ECRRepository -RepoName $WEB_REPO_NAME

# Build and push API image
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building API Docker Image" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$API_IMAGE_URI = "$ECR_REGISTRY/${API_REPO_NAME}:${ImageTag}"
$API_IMAGE_URI_LATEST = "$ECR_REGISTRY/${API_REPO_NAME}:latest"

Write-Host "Building image: $API_IMAGE_URI" -ForegroundColor Yellow

docker build `
    -t $API_IMAGE_URI `
    -t $API_IMAGE_URI_LATEST `
    -f src/Presentation/ServerMonitoring.API/Dockerfile `
    .

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to build API image" -ForegroundColor Red
    exit 1
}

Write-Host "✓ API image built successfully" -ForegroundColor Green

Write-Host "Pushing API image to ECR..." -ForegroundColor Yellow
docker push $API_IMAGE_URI
docker push $API_IMAGE_URI_LATEST

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to push API image" -ForegroundColor Red
    exit 1
}

Write-Host "✓ API image pushed successfully" -ForegroundColor Green

# Build and push Web image
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Web Docker Image" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$WEB_IMAGE_URI = "$ECR_REGISTRY/${WEB_REPO_NAME}:${ImageTag}"
$WEB_IMAGE_URI_LATEST = "$ECR_REGISTRY/${WEB_REPO_NAME}:latest"

Write-Host "Building image: $WEB_IMAGE_URI" -ForegroundColor Yellow

docker build `
    -t $WEB_IMAGE_URI `
    -t $WEB_IMAGE_URI_LATEST `
    -f ServerMonitoring.Web/Dockerfile `
    ./ServerMonitoring.Web

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to build Web image" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Web image built successfully" -ForegroundColor Green

Write-Host "Pushing Web image to ECR..." -ForegroundColor Yellow
docker push $WEB_IMAGE_URI
docker push $WEB_IMAGE_URI_LATEST

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to push Web image" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Web image pushed successfully" -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ All Images Pushed Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "API Image: $API_IMAGE_URI" -ForegroundColor Cyan
Write-Host "Web Image: $WEB_IMAGE_URI" -ForegroundColor Cyan
Write-Host ""
Write-Host "Region: $Region" -ForegroundColor Yellow
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Tag: $ImageTag" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Deploy infrastructure: aws cloudformation deploy --template-file aws/cloudformation/ecs-cluster.yml" -ForegroundColor White
Write-Host "2. Register task definitions: .\deploy-to-ecs.ps1 -Environment $Environment" -ForegroundColor White
Write-Host "3. Access your application via ALB DNS name" -ForegroundColor White
Write-Host ""
