# 🚀 AWS ECS Fargate Deployment Guide

Complete guide for deploying Server Monitoring System to AWS ECS Fargate with ECR.

---

## ⚡ QUICK START (5 Commands)

**Want to deploy NOW? Run these:**

```powershell
# 1. Configure AWS (only once)
aws configure

# 2. Run deployment script
.\deploy-aws.ps1

# 3. Go to AWS Console > ECS > Clusters > servermonitoring-cluster

# 4. Click "Deploy" and select your task definitions

# 5. Access your app at the ECS task public IP
```

**That's it!** Your images are built, pushed to ECR, and ready to deploy.

For full production setup with Load Balancer + RDS, continue reading below.

---

## 📋 Table of Contents

- [Quick Start](#-quick-start-5-commands)
- [Prerequisites](#prerequisites)
- [Architecture Overview](#architecture-overview)
- [Step-by-Step Manual Deployment](#step-by-step-manual-deployment)
- [AWS Setup](#aws-setup)
- [GitHub Setup](#github-setup)
- [Manual Deployment](#manual-deployment)
- [Automated Deployment (GitHub Actions)](#automated-deployment-github-actions)
- [Configuration](#configuration)
- [Monitoring](#monitoring)
- [Troubleshooting](#troubleshooting)
- [Cost Estimation](#cost-estimation)

---

## 📦 Step-by-Step Manual Deployment

### Phase 1: Prepare Your Environment (2 minutes)

```powershell
# 1. Configure AWS CLI credentials
aws configure
# Enter: Access Key, Secret Key, Region (us-east-1), Format (json)

# 2. Verify connection
aws sts get-caller-identity
# Note your Account ID from output

# 3. Set environment variables
$AWS_ACCOUNT_ID = "YOUR_ACCOUNT_ID_HERE"
$AWS_REGION = "us-east-1"
```

### Phase 2: Create ECR Repositories (1 minute)

```powershell
# Create repositories for your images
aws ecr create-repository --repository-name servermonitoring-api --region $AWS_REGION
aws ecr create-repository --repository-name servermonitoring-web --region $AWS_REGION
```

### Phase 3: Build & Push Docker Images (5 minutes)

```powershell
# 1. Login to ECR
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"

# 2. Build API image
docker build -t servermonitoring-api -f src/Presentation/ServerMonitoring.API/Dockerfile .

# 3. Tag and push API
docker tag servermonitoring-api:latest "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/servermonitoring-api:latest"
docker push "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/servermonitoring-api:latest"

# 4. Build Web image
docker build -t servermonitoring-web -f ServerMonitoring.Web/Dockerfile ServerMonitoring.Web

# 5. Tag and push Web
docker tag servermonitoring-web:latest "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/servermonitoring-web:latest"
docker push "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/servermonitoring-web:latest"
```

### Phase 4: Create Secrets (1 minute)

```powershell
# JWT Secret
aws secretsmanager create-secret `
    --name "servermonitoring/production/jwt-secret" `
    --secret-string "YourSuperSecretKeyForJWT_MustBeAtLeast32CharactersLong_ChangeThis!" `
    --region $AWS_REGION

# Database Connection (using in-memory for quick test)
aws secretsmanager create-secret `
    --name "servermonitoring/production/db-connection" `
    --secret-string "UseInMemoryDatabase=true" `
    --region $AWS_REGION
```

### Phase 5: Deploy to ECS (2 minutes via Console)

1. **Go to AWS Console**: https://console.aws.amazon.com/ecs/
2. **Create Cluster**:
   - Click "Clusters" → "Create Cluster"
   - Name: `servermonitoring-cluster`
   - Select: AWS Fargate
   - Click "Create"

3. **Create Task Definition**:
   - Click "Task Definitions" → "Create new Task Definition"
   - Select: Fargate
   - Task Definition Name: `servermonitoring-api`
   - Task Role: Create new or use existing with basic permissions
   - Task Execution Role: ecsTaskExecutionRole
   - Task Size: 1 vCPU, 2 GB memory
   - Add Container:
     - Name: `api`
     - Image URI: `YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/servermonitoring-api:latest`
     - Port: `8080`
     - Environment: Add secrets from Secrets Manager
   - Click "Create"

4. **Run Task**:
   - Go to Clusters → `servermonitoring-cluster`
   - Click "Run new Task"
   - Launch Type: Fargate
   - Task Definition: `servermonitoring-api:1`
   - VPC: Select default VPC
   - Subnet: Select any public subnet
   - Security Group: Create new allowing port 8080
   - Auto-assign public IP: ENABLED
   - Click "Run Task"

5. **Access Your Application**:
   - Wait for task status = RUNNING
   - Click task → Note the Public IP
   - Access: `http://PUBLIC_IP:8080/swagger`

### Phase 6: (Optional) Set Up Load Balancer

For production, add an Application Load Balancer:

1. Create ALB in EC2 Console
2. Create Target Group for port 8080
3. Update ECS Service to use ALB
4. Access via ALB DNS name

**Done!** Your app is now running on AWS ECS.

---

## ✅ Prerequisites

### Required Software

- **AWS CLI v2** - [Install Guide](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **Git** - [Download](https://git-scm.com/)
- **PowerShell 7+** (Windows) or **Bash** (Linux/Mac)

### AWS Account Requirements

- Active AWS account with billing enabled
- IAM user with appropriate permissions
- AWS CLI configured with credentials

### Estimated Monthly Cost

- **Development**: ~$50-100/month
- **Production**: ~$200-500/month (depends on traffic)

See [Cost Estimation](#cost-estimation) for details.

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         Internet                             │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │  Application Load     │
              │     Balancer (ALB)    │
              └──────────┬───────────┘
                         │
          ┌──────────────┼──────────────┐
          │              │              │
          ▼              ▼              ▼
    ┌─────────┐    ┌─────────┐    ┌─────────┐
    │   API   │    │   API   │    │   Web   │
    │ Service │    │ Service │    │ Service │
    │ (Task)  │    │ (Task)  │    │ (Task)  │
    └────┬────┘    └────┬────┘    └─────────┘
         │              │
         └──────┬───────┘
                │
                ▼
        ┌──────────────┐
        │  RDS SQL      │
        │  Server       │
        └──────────────┘
```

### Components

1. **VPC**: Isolated network with public and private subnets across 2 AZs
2. **Application Load Balancer**: Routes traffic to API and Web services
3. **ECS Fargate Cluster**: Runs containerized applications without managing servers
4. **ECR**: Stores Docker images
5. **RDS SQL Server**: Managed database (optional, can use container)
6. **CloudWatch**: Logging and monitoring
7. **Secrets Manager**: Stores sensitive configuration

---

## 🔧 AWS Setup

### Step 1: Configure AWS CLI

```powershell
# Configure AWS CLI with your credentials
aws configure

# Enter your details:
AWS Access Key ID: YOUR_ACCESS_KEY
AWS Secret Access Key: YOUR_SECRET_KEY
Default region name: us-east-1
Default output format: json

# Test configuration
aws sts get-caller-identity
```

### Step 2: Create AWS Secrets

Create secrets for sensitive configuration:

```powershell
# Database connection string
aws secretsmanager create-secret \
    --name servermonitoring/production/db-connection \
    --description "Database connection string" \
    --secret-string "Server=your-rds-endpoint.rds.amazonaws.com,1433;Database=ServerMonitoring;User Id=admin;Password=YourPassword;TrustServerCertificate=True;" \
    --region us-east-1

# JWT secret key
aws secretsmanager create-secret \
    --name servermonitoring/production/jwt-secret \
    --description "JWT secret key" \
    --secret-string "YourSuperSecretKeyForJWT_MustBeAtLeast32Characters_ChangeInProduction!" \
    --region us-east-1

# Redis connection (optional)
aws secretsmanager create-secret \
    --name servermonitoring/production/redis-connection \
    --description "Redis connection string" \
    --secret-string "your-redis-endpoint.cache.amazonaws.com:6379,password=YourRedisPassword" \
    --region us-east-1
```

### Step 3: Create IAM Role for GitHub Actions (Optional for CI/CD)

Create a trust policy file `github-trust-policy.json`:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Federated": "arn:aws:iam::YOUR_ACCOUNT_ID:oidc-provider/token.actions.githubusercontent.com"
      },
      "Action": "sts:AssumeRoleWithWebIdentity",
      "Condition": {
        "StringEquals": {
          "token.actions.githubusercontent.com:aud": "sts.amazonaws.com"
        },
        "StringLike": {
          "token.actions.githubusercontent.com:sub": "repo:YOUR_USERNAME/YOUR_REPO:*"
        }
      }
    }
  ]
}
```

Create the role:

```powershell
# First, create OIDC provider (one-time setup)
aws iam create-open-id-connect-provider \
    --url https://token.actions.githubusercontent.com \
    --client-id-list sts.amazonaws.com \
    --thumbprint-list 6938fd4d98bab03faadb97b34396831e3780aca1

# Create IAM role
aws iam create-role \
    --role-name GitHubActionsECSDeployRole \
    --assume-role-policy-document file://github-trust-policy.json

# Attach policies
aws iam attach-role-policy \
    --role-name GitHubActionsECSDeployRole \
    --policy-arn arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryFullAccess

aws iam attach-role-policy \
    --role-name GitHubActionsECSDeployRole \
    --policy-arn arn:aws:iam::aws:policy/AmazonECS_FullAccess

aws iam attach-role-policy \
    --role-name GitHubActionsECSDeployRole \
    --policy-arn arn:aws:iam::aws:policy/CloudWatchLogsFullAccess
```

---

## 🐙 GitHub Setup

### Step 1: Create GitHub Repository

```powershell
# Navigate to your project
cd C:\Users\lenovo\Downloads\assesment

# Initialize git (if not already done)
git init

# Add all files
git add .
git commit -m "Initial commit - Server Monitoring System"

# Create repository on GitHub (via web interface or gh CLI)
# Then push
git remote add origin https://github.com/YOUR_USERNAME/server-monitoring.git
git branch -M main
git push -u origin main
```

### Step 2: Configure GitHub Secrets

Go to your GitHub repository → Settings → Secrets and variables → Actions

Add the following secrets:

| Secret Name | Value | Description |
|------------|-------|-------------|
| `AWS_ACCOUNT_ID` | Your AWS Account ID | 12-digit AWS account number |
| `AWS_ROLE_ARN` | arn:aws:iam::ACCOUNT_ID:role/GitHubActionsECSDeployRole | IAM role ARN |
| `AWS_REGION` | us-east-1 | AWS region |

---

## 🛠️ Manual Deployment

### Step 1: Deploy Infrastructure

```powershell
# Navigate to project root
cd C:\Users\lenovo\Downloads\assesment

# Deploy CloudFormation stack
aws cloudformation deploy \
    --template-file aws/cloudformation/ecs-cluster.yml \
    --stack-name production-servermonitoring-cluster \
    --parameter-overrides EnvironmentName=production \
    --capabilities CAPABILITY_IAM \
    --region us-east-1

# Wait for completion (10-15 minutes)
aws cloudformation wait stack-create-complete \
    --stack-name production-servermonitoring-cluster \
    --region us-east-1
```

### Step 2: Build and Push Docker Images

```powershell
# Run the ECR push script
.\push-to-ecr.ps1 -Environment production -ImageTag v1.0.0

# This will:
# 1. Login to ECR
# 2. Create ECR repositories if needed
# 3. Build API and Web Docker images
# 4. Push images to ECR
```

**Expected Output:**
```
========================================
✓ All Images Pushed Successfully!
========================================

API Image: 123456789012.dkr.ecr.us-east-1.amazonaws.com/servermonitoring-api:v1.0.0
Web Image: 123456789012.dkr.ecr.us-east-1.amazonaws.com/servermonitoring-web:v1.0.0
```

### Step 3: Deploy ECS Services

```powershell
# Run the ECS deployment script
.\deploy-to-ecs.ps1 -Environment production -ImageTag v1.0.0

# This will:
# 1. Register task definitions
# 2. Create or update ECS services
# 3. Wait for services to stabilize
```

**Expected Output:**
```
========================================
✓ Deployment Completed Successfully!
========================================

Application URL: http://prod-alb-123456789.us-east-1.elb.amazonaws.com
API URL: http://prod-alb-123456789.us-east-1.elb.amazonaws.com/api
Swagger: http://prod-alb-123456789.us-east-1.elb.amazonaws.com/swagger
Hangfire: http://prod-alb-123456789.us-east-1.elb.amazonaws.com/hangfire
```

### Step 4: Verify Deployment

```powershell
# Check service status
aws ecs describe-services \
    --cluster production-servermonitoring-cluster \
    --services servermonitoring-api-service \
    --region us-east-1

# Check running tasks
aws ecs list-tasks \
    --cluster production-servermonitoring-cluster \
    --service-name servermonitoring-api-service \
    --region us-east-1

# View logs
aws logs tail /ecs/production/servermonitoring-api --follow --region us-east-1
```

---

## 🤖 Automated Deployment (GitHub Actions)

Once GitHub Actions is set up, deployment happens automatically:

### Automatic Triggers

- **Push to `main` branch** → Deploys to production
- **Push to `develop` branch** → Deploys to development
- **Manual trigger** → Deploy to any environment with custom tag

### Manual Deployment via GitHub Actions

1. Go to GitHub repository → Actions
2. Select "Deploy to AWS ECS Fargate" workflow
3. Click "Run workflow"
4. Select:
   - Environment: `production`, `staging`, or `development`
   - Image tag: e.g., `v1.0.0` or leave as `latest`
5. Click "Run workflow"

### Workflow Steps

1. **Determine Environment**: Based on branch or manual input
2. **Build and Test**: Runs all backend and frontend tests
3. **Build and Push Images**: Builds Docker images and pushes to ECR
4. **Deploy Infrastructure**: Creates/updates CloudFormation stack
5. **Deploy Services**: Registers task definitions and updates ECS services
6. **Verify**: Waits for services to stabilize

---

## ⚙️ Configuration

### Environment Variables

Update task definitions in `aws/ecs/` with your environment-specific values:

**API Task Definition** (`api-task-definition.json`):
- `ConnectionStrings__DefaultConnection` - From Secrets Manager
- `Jwt__SecretKey` - From Secrets Manager
- `Redis__ConnectionString` - From Secrets Manager (optional)

**Web Task Definition** (`web-task-definition.json`):
- `VITE_API_URL` - ALB DNS name (auto-configured)
- `VITE_SIGNALR_HUB_URL` - ALB DNS + /hubs/monitoring

### Scaling Configuration

**Modify task definitions for different resource allocations:**

```json
{
  "cpu": "1024",     // 1 vCPU (256, 512, 1024, 2048, 4096)
  "memory": "2048"   // 2 GB (512, 1024, 2048, 4096, 8192)
}
```

**Modify service desired count:**

```powershell
aws ecs update-service \
    --cluster production-servermonitoring-cluster \
    --service servermonitoring-api-service \
    --desired-count 4 \
    --region us-east-1
```

### Auto Scaling (Optional)

Create auto-scaling policies in CloudFormation or via CLI:

```powershell
# Register scalable target
aws application-autoscaling register-scalable-target \
    --service-namespace ecs \
    --scalable-dimension ecs:service:DesiredCount \
    --resource-id service/production-servermonitoring-cluster/servermonitoring-api-service \
    --min-capacity 2 \
    --max-capacity 10 \
    --region us-east-1

# Create scaling policy (CPU-based)
aws application-autoscaling put-scaling-policy \
    --policy-name cpu-scale-policy \
    --service-namespace ecs \
    --scalable-dimension ecs:service:DesiredCount \
    --resource-id service/production-servermonitoring-cluster/servermonitoring-api-service \
    --policy-type TargetTrackingScaling \
    --target-tracking-scaling-policy-configuration file://scaling-policy.json \
    --region us-east-1
```

---

## 📊 Monitoring

### CloudWatch Dashboards

View metrics in AWS Console:
- ECS → Clusters → Your Cluster → Metrics
- CloudWatch → Dashboards

Key Metrics:
- CPU Utilization
- Memory Utilization
- Request Count
- Target Response Time
- Unhealthy Host Count

### Logs

```powershell
# API logs
aws logs tail /ecs/production/servermonitoring-api --follow --region us-east-1

# Web logs
aws logs tail /ecs/production/servermonitoring-web --follow --region us-east-1

# Search logs
aws logs filter-log-events \
    --log-group-name /ecs/production/servermonitoring-api \
    --filter-pattern "ERROR" \
    --region us-east-1
```

### Alerts

Create CloudWatch alarms:

```powershell
# High CPU alarm
aws cloudwatch put-metric-alarm \
    --alarm-name api-high-cpu \
    --alarm-description "API CPU utilization too high" \
    --metric-name CPUUtilization \
    --namespace AWS/ECS \
    --statistic Average \
    --period 300 \
    --threshold 80 \
    --comparison-operator GreaterThanThreshold \
    --evaluation-periods 2 \
    --region us-east-1
```

---

## 🔍 Troubleshooting

### Common Issues

#### 1. Service fails to start

**Symptoms**: Tasks keep stopping and restarting

**Solutions**:
```powershell
# Check service events
aws ecs describe-services \
    --cluster production-servermonitoring-cluster \
    --services servermonitoring-api-service \
    --region us-east-1 \
    --query 'services[0].events[0:10]'

# Check task stopped reason
aws ecs describe-tasks \
    --cluster production-servermonitoring-cluster \
    --tasks TASK_ID \
    --region us-east-1 \
    --query 'tasks[0].stoppedReason'

# Common fixes:
# - Check health check endpoint is accessible
# - Verify database connection string
# - Check secrets are accessible
# - Ensure security groups allow traffic
```

#### 2. Cannot pull ECR image

**Symptoms**: "CannotPullContainerError"

**Solutions**:
```powershell
# Verify task execution role has ECR permissions
# Verify image exists in ECR
aws ecr describe-images \
    --repository-name servermonitoring-api \
    --region us-east-1

# Check ECR repository policy
aws ecr get-repository-policy \
    --repository-name servermonitoring-api \
    --region us-east-1
```

#### 3. Database connection fails

**Symptoms**: "Cannot connect to database" errors in logs

**Solutions**:
- Verify RDS security group allows inbound from ECS security group
- Check connection string in Secrets Manager
- Verify RDS endpoint DNS is resolvable
- Test connection from ECS task:

```powershell
# Execute command in running task
aws ecs execute-command \
    --cluster production-servermonitoring-cluster \
    --task TASK_ID \
    --container servermonitoring-api-service \
    --interactive \
    --command "/bin/bash" \
    --region us-east-1
```

#### 4. ALB health checks failing

**Symptoms**: Targets marked unhealthy

**Solutions**:
```powershell
# Check target group health
aws elbv2 describe-target-health \
    --target-group-arn TARGET_GROUP_ARN \
    --region us-east-1

# Common fixes:
# - Verify health check path returns 200
# - Check container port mapping
# - Ensure app starts within health check grace period
# - Verify security group allows ALB → ECS traffic
```

---

## 💰 Cost Estimation

### Development Environment

| Service | Configuration | Monthly Cost |
|---------|--------------|--------------|
| ECS Fargate | 2 API tasks (1 vCPU, 2GB) | ~$35 |
| ECS Fargate | 2 Web tasks (0.5 vCPU, 1GB) | ~$17 |
| ALB | 1 Application Load Balancer | ~$23 |
| ECR | 5GB storage | ~$0.50 |
| CloudWatch Logs | 5GB logs | ~$2.50 |
| **Total** | | **~$78/month** |

### Production Environment

| Service | Configuration | Monthly Cost |
|---------|--------------|--------------|
| ECS Fargate | 4 API tasks (1 vCPU, 2GB) | ~$70 |
| ECS Fargate | 4 Web tasks (0.5 vCPU, 1GB) | ~$34 |
| RDS SQL Server | db.t3.small | ~$120 |
| ElastiCache Redis | cache.t3.micro | ~$15 |
| ALB | 1 Application Load Balancer | ~$23 |
| ECR | 20GB storage | ~$2 |
| CloudWatch Logs | 20GB logs | ~$10 |
| Data Transfer | 100GB outbound | ~$9 |
| **Total** | | **~$283/month** |

**Cost Optimization Tips**:
- Use Savings Plans or Reserved Instances for predictable workloads
- Use Fargate Spot for non-critical tasks (70% cost reduction)
- Set up log retention policies
- Enable ECR lifecycle policies
- Use Auto Scaling to right-size capacity

---

## 🔄 Rollback

### Manual Rollback

```powershell
# List previous task definitions
aws ecs list-task-definitions \
    --family-prefix servermonitoring-api \
    --region us-east-1

# Update service to previous version
aws ecs update-service \
    --cluster production-servermonitoring-cluster \
    --service servermonitoring-api-service \
    --task-definition servermonitoring-api:REVISION_NUMBER \
    --force-new-deployment \
    --region us-east-1
```

### Automatic Rollback

ECS deployment circuit breaker is enabled in service definitions. Failed deployments automatically roll back.

---

## 🧹 Cleanup

To completely remove all AWS resources:

```powershell
# Delete ECS services
aws ecs delete-service \
    --cluster production-servermonitoring-cluster \
    --service servermonitoring-api-service \
    --force \
    --region us-east-1

aws ecs delete-service \
    --cluster production-servermonitoring-cluster \
    --service servermonitoring-web-service \
    --force \
    --region us-east-1

# Wait for services to drain
Start-Sleep -Seconds 60

# Delete CloudFormation stack
aws cloudformation delete-stack \
    --stack-name production-servermonitoring-cluster \
    --region us-east-1

# Delete ECR repositories
aws ecr delete-repository \
    --repository-name servermonitoring-api \
    --force \
    --region us-east-1

aws ecr delete-repository \
    --repository-name servermonitoring-web \
    --force \
    --region us-east-1

# Delete secrets
aws secretsmanager delete-secret \
    --secret-id servermonitoring/production/db-connection \
    --force-delete-without-recovery \
    --region us-east-1
```

---

## 📚 Additional Resources

- [AWS ECS Best Practices](https://docs.aws.amazon.com/AmazonECS/latest/bestpracticesguide/intro.html)
- [AWS Fargate Pricing](https://aws.amazon.com/fargate/pricing/)
- [AWS CloudFormation Documentation](https://docs.aws.amazon.com/cloudformation/)
- [GitHub Actions AWS Integration](https://github.com/aws-actions)

---

## 🆘 Support

For issues and questions:
- Check CloudWatch Logs first
- Review [Troubleshooting](#troubleshooting) section
- Check AWS Service Health Dashboard
- Review ECS service events

---
