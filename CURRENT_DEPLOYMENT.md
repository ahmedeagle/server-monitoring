# 🚀 Current Deployment URLs

**Status:** ⏳ Deployment in Progress  
**Last Checked:** Manual check - waiting for GitHub Actions to complete

---

## 🌐 Access Your Application

### Previous Deployment IPs (may be outdated during deployment)

**Web Application (React Frontend)**
- Previous IP: http://34.201.65.116
- Login: http://34.201.65.116/login

**API Backend (.NET)**
- Previous IP: http://44.202.3.160:8080
- Swagger: http://44.202.3.160:8080/swagger
- Health: http://44.202.3.160:8080/health

**Test Credentials:**
- Admin: `admin` / `Admin@123`
- User: `user` / `User@123`

---

## ⏳ Current Status

**Deployment is IN PROGRESS** - GitHub Actions is:
1. Building new Docker images
2. Registering task definitions
3. Updating ECS services
4. Waiting for health checks

**Estimated time:** 8-10 minutes from last push

---

## 📍 How to Get Latest IPs

### Option 1: Wait for Auto-Update (Recommended)
This file will be **automatically updated** by GitHub Actions when deployment completes.

Run this command to get the latest:
```bash
git pull origin main
```

### Option 2: Check GitHub Actions
1. Go to: https://github.com/ahmedeagle/server-monitoring/actions
2. Click on the latest workflow run
3. Scroll to the bottom of the "Get service URLs" step
4. Copy the IPs from the output

### Option 3: AWS Console
1. Go to AWS ECS Console
2. Click on `servermonitoring-cluster`
3. Click on each service
4. Go to "Tasks" tab
5. Click on running task
6. Find "Public IP" in the details

---

## 🔄 Why IPs Change

ECS Fargate tasks are destroyed and recreated with each deployment, receiving new public IPs. This is normal behavior for containerized deployments.

**Solution for production:** Use Application Load Balancer (ALB) for stable DNS names.

---

## 🏗️ Infrastructure Details

- **Cloud Provider:** AWS (us-east-1)
- **Orchestration:** ECS Fargate
- **Container Registry:** Amazon ECR
- **Deployment:** GitHub Actions CI/CD
- **Network:** Public subnets with Internet Gateway

---

**✅ This file will be automatically updated with fresh IPs when deployment completes!**
