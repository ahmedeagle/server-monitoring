# AWS ECS Deployment Checklist

Follow this checklist to deploy Server Monitoring System to AWS ECS.

## ✅ Pre-Deployment (5 mins)

- [ ] AWS Account created and active
- [ ] AWS CLI installed: `aws --version`
- [ ] Docker Desktop running: `docker ps`
- [ ] AWS configured: `aws configure`
- [ ] Verify access: `aws sts get-caller-identity`

## ✅ Quick Deploy (Automated - 10 mins)

### Option A: Use Automated Script ⭐ RECOMMENDED

```powershell
.\deploy-aws.ps1
```

**What it does:**
- Creates ECR repositories
- Builds Docker images
- Pushes to ECR
- Creates secrets
- Creates ECS cluster
- Creates IAM roles

### Option B: Manual Deployment

Continue with checklist below...

---

## ✅ Manual Deployment Steps

### Step 1: Get AWS Account ID

```powershell
aws sts get-caller-identity --query Account --output text
```

My Account ID: `___________________________`

### Step 2: Create ECR Repositories

```powershell
aws ecr create-repository --repository-name servermonitoring-api --region us-east-1
aws ecr create-repository --repository-name servermonitoring-web --region us-east-1
```

- [ ] API repository created
- [ ] Web repository created

### Step 3: Build & Push Images

```powershell
# Login to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com

# Build API
docker build -t servermonitoring-api -f src/Presentation/ServerMonitoring.API/Dockerfile .
docker tag servermonitoring-api:latest ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/servermonitoring-api:latest
docker push ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/servermonitoring-api:latest

# Build Web
docker build -t servermonitoring-web -f ServerMonitoring.Web/Dockerfile ServerMonitoring.Web
docker tag servermonitoring-web:latest ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/servermonitoring-web:latest
docker push ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/servermonitoring-web:latest
```

- [ ] Logged into ECR
- [ ] API image built
- [ ] API image pushed
- [ ] Web image built
- [ ] Web image pushed

### Step 4: Create Secrets

```powershell
# JWT Secret
aws secretsmanager create-secret --name "servermonitoring/production/jwt-secret" --secret-string "YourSuperSecretKeyHere_AtLeast32Characters" --region us-east-1

# DB Connection (in-memory for quick test)
aws secretsmanager create-secret --name "servermonitoring/production/db-connection" --secret-string "UseInMemoryDatabase=true" --region us-east-1
```

- [ ] JWT secret created
- [ ] DB connection created

### Step 5: Create ECS Cluster (AWS Console)

1. Go to: https://console.aws.amazon.com/ecs/
2. Click "Clusters" → "Create Cluster"
3. Name: `servermonitoring-cluster`
4. Infrastructure: AWS Fargate
5. Click "Create"

- [ ] Cluster created

### Step 6: Create Task Definition (AWS Console)

1. Click "Task Definitions" → "Create new task definition" → "Create new task definition with JSON"
2. Use this JSON (replace ACCOUNT_ID):

```json
{
  "family": "servermonitoring-api",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "1024",
  "memory": "2048",
  "executionRoleArn": "arn:aws:iam::ACCOUNT_ID:role/ecsTaskExecutionRole",
  "containerDefinitions": [
    {
      "name": "api",
      "image": "ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/servermonitoring-api:latest",
      "cpu": 1024,
      "memory": 2048,
      "essential": true,
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "ASPNETCORE_URLS",
          "value": "http://+:8080"
        },
        {
          "name": "UseInMemoryDatabase",
          "value": "true"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-create-group": "true",
          "awslogs-group": "/ecs/servermonitoring-api",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

- [ ] Task definition created

### Step 7: Run Task (AWS Console)

1. Go to Clusters → `servermonitoring-cluster`
2. Click "Tasks" tab → "Run new task"
3. Configuration:
   - **Compute options**: Launch type
   - **Launch type**: FARGATE
   - **Platform version**: LATEST
   - **Task definition**: servermonitoring-api
   - **Revision**: LATEST
4. Networking:
   - **VPC**: Select default VPC
   - **Subnets**: Select any **public** subnet
   - **Security group**: Create new
     - Allow inbound port 8080 from anywhere (0.0.0.0/0)
   - **Public IP**: ENABLED
5. Click "Create"

- [ ] Task created
- [ ] Task status = RUNNING

### Step 8: Access Application

1. Click on the running task
2. Copy the **Public IP** address
3. Open browser: `http://PUBLIC_IP:8080/swagger`

My Task Public IP: `___________________________`

Access URLs:
- [ ] API/Swagger: http://PUBLIC_IP:8080/swagger
- [ ] Health Check: http://PUBLIC_IP:8080/health
- [ ] Hangfire: http://PUBLIC_IP:8080/hangfire

---

## ✅ Production Enhancements (Optional)

### Set Up Application Load Balancer

- [ ] Create ALB in EC2 Console
- [ ] Create Target Group (port 8080)
- [ ] Add HTTPS certificate
- [ ] Update ECS Service to use ALB
- [ ] Configure health checks

### Set Up RDS Database

- [ ] Create RDS SQL Server instance
- [ ] Update secrets with connection string
- [ ] Remove UseInMemoryDatabase environment variable
- [ ] Restart tasks

### Set Up Auto Scaling

- [ ] Create ECS Service (instead of standalone tasks)
- [ ] Configure service auto scaling
- [ ] Set min/max task count

### Monitoring & Alerts

- [ ] Enable Container Insights
- [ ] Create CloudWatch dashboard
- [ ] Set up alarms for CPU/Memory
- [ ] Configure CloudWatch Logs retention

---

## 🎯 Success Criteria

- ✅ Images in ECR
- ✅ ECS cluster running
- ✅ Task in RUNNING state
- ✅ Swagger UI accessible
- ✅ Health check returns 200
- ✅ Can login with admin/Admin123!

---

## 🔧 Troubleshooting

**Task fails to start:**
- Check CloudWatch Logs: /ecs/servermonitoring-api
- Verify secrets exist: `aws secretsmanager list-secrets`
- Check IAM permissions on execution role

**Can't access via Public IP:**
- Verify Public IP is enabled
- Check security group allows port 8080
- Ensure subnet is public (has Internet Gateway)

**Image pull errors:**
- Verify image exists in ECR
- Check execution role has ECR pull permissions
- Ensure region matches

---

## 📚 Resources

- Full deployment guide: [docs/AWS_DEPLOYMENT.md](AWS_DEPLOYMENT.md)
- AWS ECS Documentation: https://docs.aws.amazon.com/ecs/
- AWS ECR Documentation: https://docs.aws.amazon.com/ecr/

---

**Estimated Time:**
- Automated (deploy-aws.ps1): ~10 minutes
- Manual deployment: ~20 minutes
- With production enhancements: ~1 hour

**Estimated Cost:**
- Development/Testing: ~$2-5/day
- Production with ALB + RDS: ~$200-500/month
