# Scripts Directory

All PowerShell scripts for building, testing, and deploying the Server Monitoring System.

---

## 🚀 Main Scripts (Start Here)

### test-local.ps1 ⭐
**Purpose:** Main testing script - runs full application with Docker  
**Usage:** `.\scripts\test-local.ps1` or `.\test.ps1`  
**What it does:**
- Validates Docker is running
- Builds all 4 services (SQL, Redis, API, Web)
- Waits for health checks
- Tests endpoints
- Displays access URLs

**Use this for:** Assessment, demo, local testing

---

## ☁️ AWS Deployment Scripts

### setup-aws-for-github.ps1
**Purpose:** One-time AWS setup for GitHub Actions CI/CD  
**Usage:** `.\scripts\setup-aws-for-github.ps1`

### deploy-aws.ps1
**Purpose:** Deploy directly from local machine to AWS ECS  
**Usage:** `.\scripts\deploy-aws.ps1`  
**Note:** Requires Docker Desktop running

---

## ✅ Validation Scripts

### check-submission.ps1
**Purpose:** Validate project structure and readiness  
**Usage:** `.\scripts\check-submission.ps1`

---

## 🛠️ Development Scripts

### START.ps1
**Purpose:** Quick start for development (opens backend + frontend terminals)

### start-backend.ps1
**Purpose:** Start API only in development mode

### start-frontend.ps1
**Purpose:** Start Web UI only in development mode

### run-tests.ps1
**Purpose:** Run unit and integration tests

### CREATE_MIGRATIONS.ps1
**Purpose:** Create Entity Framework migrations

### open-presentation.ps1
**Purpose:** Open index.html presentation in browser

---

## 📋 Quick Reference

```powershell
# For Assessors/Demo
.\scripts\test-local.ps1          # ⭐ Main testing script
# or use convenience launcher:
.\test.ps1

# For AWS Deployment via GitHub Actions
.\scripts\setup-aws-for-github.ps1  # Setup AWS (one-time)
git push                            # Auto-deploys via GitHub Actions

# For Validation
.\scripts\check-submission.ps1    # Verify project readiness

# For Development
.\scripts\START.ps1               # Start dev environment
.\scripts\run-tests.ps1           # Run test suite
```

---

## 📚 Documentation

See `docs/` folder for detailed guides:
- [QUICK_START.md](../docs/QUICK_START.md)
- [GITHUB_DEPLOYMENT_GUIDE.md](../docs/GITHUB_DEPLOYMENT_GUIDE.md)
- [AWS_DEPLOYMENT.md](../docs/AWS_DEPLOYMENT.md)
- [AWS_DEPLOYMENT_CHECKLIST.md](../docs/AWS_DEPLOYMENT_CHECKLIST.md)

### Test Application
```powershell
# From root folder
.\test-local.ps1                 # Full automated test with Docker
.\check-submission.ps1           # Verify submission completeness
```

### Environment Configuration
```powershell
# Development (uses defaults)
docker-compose up -d

# Production setup
.\scripts\environment\setup-env.ps1 -Environment Production
.\scripts\environment\generate-secrets.ps1
notepad .env
.\scripts\environment\validate-env.ps1
docker-compose up -d
```

### Deployment
```powershell
# AWS ECS
.\scripts\deployment\push-to-ecr.ps1
.\scripts\deployment\deploy-to-ecs.ps1

# Docker Swarm
.\scripts\deployment\deploy-swarm.ps1
```

## 📝 Note

**Essential scripts remain in root:**
- `START.ps1` - Main entry point
- `test-local.ps1` - Quick test script
- `check-submission.ps1` - Submission verification

All other scripts are organized here for a cleaner root folder.
