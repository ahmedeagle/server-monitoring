# GitHub to AWS ECS Deployment Guide

Deploy Server Monitoring System to AWS ECS using GitHub Actions CI/CD.

---

## 🚀 Quick Setup (5 Steps)

### Step 1: Create AWS Resources (One-Time Setup)

Run this locally to create ECR repos, ECS cluster, and IAM roles:

```powershell
# Configure AWS CLI
aws configure

# Run setup script
.\scripts\setup-aws-for-github.ps1
```

Or manually:

```powershell

# Create ECR repositories
aws ecr create-repository --repository-name servermonitoring-api --region us-east-1
aws ecr create-repository --repository-name servermonitoring-web --region us-east-1

# Create ECS cluster
aws ecs create-cluster --cluster-name servermonitoring-cluster --region us-east-1

# Create IAM execution role
aws iam create-role --role-name ecsTaskExecutionRole-ServerMonitoring --assume-role-policy-document file://trust-policy.json
aws iam attach-role-policy --role-name ecsTaskExecutionRole-ServerMonitoring --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy

# Create IAM task role
aws iam create-role --role-name ecsTaskRole-ServerMonitoring --assume-role-policy-document file://trust-policy.json
```

**trust-policy.json:**
```json
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
```

### Step 2: Push Code to GitHub

```powershell
# Initialize git (if not already done)
git init

# Add all files
git add .

# Commit
git commit -m "Initial commit - Server Monitoring System"

# Create GitHub repo at https://github.com/new

# Add remote and push
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
git branch -M main
git push -u origin main
```

### Step 3: Add GitHub Secrets

Go to your GitHub repository:
1. Click **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Add these secrets:

| Secret Name | Value | How to Get |
|------------|-------|------------|
| `AWS_ACCESS_KEY_ID` | Your AWS access key | AWS Console → IAM → Users → Security credentials |
| `AWS_SECRET_ACCESS_KEY` | Your AWS secret key | AWS Console → IAM → Users → Security credentials |
| `AWS_ACCOUNT_ID` | Your AWS account ID | Run: `aws sts get-caller-identity --query Account --output text` |

### Step 4: Trigger Deployment

**Option A: Automatic (on push)**
```powershell
# Make any change and push
git add .
git commit -m "Trigger deployment"
git push
```

**Option B: Manual (via GitHub UI)**
1. Go to **Actions** tab in GitHub
2. Click **Deploy to AWS ECS** workflow
3. Click **Run workflow** → **Run workflow**

### Step 5: Access Your Application

1. Go to **Actions** tab in GitHub
2. Click the running workflow
3. Wait for it to complete (~10 minutes)
4. Check the last step **"Get service URL"** for your application URL
5. Access:
   - API: `http://PUBLIC_IP:8080/swagger`
   - Health: `http://PUBLIC_IP:8080/health`

---

## 📋 What the Workflow Does

1. ✅ Checks out your code
2. ✅ Configures AWS credentials from GitHub Secrets
3. ✅ Logs into Amazon ECR
4. ✅ Builds Docker images on GitHub runners (no local Docker needed!)
5. ✅ Pushes images to ECR with git SHA tags + latest
6. ✅ Creates/updates ECS task definitions
7. ✅ Creates ECS service (first time) or updates it (subsequent runs)
8. ✅ Waits for deployment to complete
9. ✅ Outputs the public IP/URL

---

## 🔧 Customization

### Change Region

Edit `.github/workflows/deploy-to-ecs.yml`:
```yaml
env:
  AWS_REGION: us-west-2  # Change this
```

### Use RDS Database

Edit the task definition in workflow:
```yaml
{
  "name": "ConnectionStrings__DefaultConnection",
  "value": "Server=YOUR_RDS_ENDPOINT;Database=ServerMonitoring;..."
}
```

Remove:
```yaml
{
  "name": "UseInMemoryDatabase",
  "value": "true"
}
```

### Scale Up

Edit the service creation:
```yaml
--desired-count 3  # Run 3 tasks instead of 1
```

---

## 🛠️ Troubleshooting

### Workflow Fails at "Build Docker Image"

**Issue:** Docker build fails
**Solution:** Check Dockerfile paths in workflow match your repo structure

### Workflow Fails at "Deploy to ECS"

**Issue:** IAM permissions
**Solution:** Ensure AWS user has these policies:
- `AmazonECS_FullAccess`
- `AmazonEC2ContainerRegistryFullAccess`
- `IAMReadOnlyAccess`

### Can't Access Public IP

**Issue:** Security group blocking traffic
**Solution:** The workflow creates the security group automatically. If it fails:
```powershell
# Manually allow port 8080
aws ec2 authorize-security-group-ingress \
  --group-id sg-XXXXX \
  --protocol tcp \
  --port 8080 \
  --cidr 0.0.0.0/0
```

### Need to See Logs

```powershell
# View ECS logs
aws logs tail /ecs/servermonitoring-api --follow
```

---

## 📊 Workflow Status Badge

Add this to your README.md:
```markdown
![Deploy to AWS ECS](https://github.com/YOUR_USERNAME/YOUR_REPO/actions/workflows/deploy-to-ecs.yml/badge.svg)
```

---

## 💰 Cost Estimate

- **ECR Storage**: ~$0.10/GB/month
- **ECS Fargate**: ~$0.04/hour per task (~$30/month for 1 task)
- **Data Transfer**: First 100GB free, then $0.09/GB

**Total**: ~$30-50/month for development/testing

---

## 🎯 Benefits of GitHub Actions Approach

✅ **No local Docker required** - Builds run on GitHub servers
✅ **Automatic deployments** - Push to main = automatic deploy
✅ **Version tracking** - Each deployment tagged with git SHA
✅ **Rollback easy** - Just deploy previous git SHA
✅ **Free for public repos** - 2000 minutes/month for private repos
✅ **Logs & history** - All deployment logs saved in GitHub

---

## 🔄 Update Flow

```
Code Change → Git Push → GitHub Actions Triggered → Docker Build → ECR Push → ECS Deploy → Live!
```

**Time: ~10 minutes** from push to live

---

## 📝 Next Steps for Production

1. **Add Load Balancer**: Use Application Load Balancer for HTTPS and better availability
2. **Add RDS**: Replace in-memory database with RDS SQL Server
3. **Add Redis**: Use ElastiCache for distributed caching
4. **Add Auto Scaling**: Scale based on CPU/Memory metrics
5. **Add Custom Domain**: Use Route 53 + ACM for custom domain with SSL
6. **Add Monitoring**: Use CloudWatch dashboards and alarms

See [docs/AWS_DEPLOYMENT.md](docs/AWS_DEPLOYMENT.md) for production setup details.
