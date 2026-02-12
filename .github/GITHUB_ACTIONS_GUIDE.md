# GitHub Actions CI/CD Guide

## 🎯 Overview

This repository includes **comprehensive GitHub Actions workflows** for automated testing, deployment, and rollback with Docker Swarm integration.

---

## 📋 Workflows

### **1. CI/CD Pipeline** (`ci-cd.yml`)

Automated build, test, and deployment pipeline.

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests
- Manual workflow dispatch

**Stages:**
1. **Code Quality** - Linting, formatting, TypeScript checks
2. **Security Scan** - Trivy vulnerability scanning, dependency checks
3. **Backend Tests** - Unit tests with coverage
4. **Frontend Tests** - Jest/Vitest tests with coverage
5. **Build Images** - Docker multi-arch builds, push to GHCR
6. **Deploy Dev** - Auto-deploy to development (develop branch)
7. **Deploy Staging** - Auto-deploy to staging (main branch)
8. **Deploy Prod** - Manual approval required
9. **Integration Tests** - Post-deployment verification

**Usage:**
```bash
# Automatic on push
git push origin main

# Manual deployment
gh workflow run ci-cd.yml -f environment=production
```

---

### **2. Rollback Workflow** (`rollback.yml`)

Fast rollback to previous stable version.

**Features:**
- Rollback to previous version (automatic)
- Rollback to specific version (by tag)
- Production requires 2 approvals
- Health checks after rollback
- Slack notifications

**Usage:**
```bash
# Rollback to previous version
gh workflow run rollback.yml -f environment=production

# Rollback to specific version
gh workflow run rollback.yml -f environment=production -f rollback-to=v1.2.3
```

**Manual via GitHub UI:**
1. Go to Actions → Rollback Deployment
2. Click "Run workflow"
3. Select environment
4. (Optional) Specify version tag
5. Click "Run workflow"

---

### **3. Security Scanning** (`security-scan.yml`)

Daily automated security scans of Docker images.

**Features:**
- Trivy vulnerability scanning
- Snyk security analysis
- SARIF upload to GitHub Security
- Auto-create issues for vulnerabilities

**Triggers:**
- Daily at 2 AM UTC (scheduled)
- When Dockerfile changes
- Manual dispatch

---

### **4. Performance Tests** (`performance-tests.yml`)

Load testing with k6.

**Features:**
- Simulates 100-200 concurrent users
- Configurable test duration
- Performance thresholds:
  - 95% requests < 500ms
  - 99% requests < 1000ms
  - Error rate < 1%

**Usage:**
```bash
# Run performance test
gh workflow run performance-tests.yml -f environment=staging -f duration=10
```

---

## 🔐 Required Secrets

Configure these in GitHub Settings → Secrets and variables → Actions:

### **Docker & Registry**
```
GITHUB_TOKEN (auto-provided)
```

### **Swarm Hosts**
```
DEV_SWARM_HOST=dev-swarm.example.com
DEV_SWARM_USER=deploy
DEV_SWARM_SSH_KEY=<ssh-private-key>

STAGING_SWARM_HOST=staging-swarm.example.com
STAGING_SWARM_USER=deploy
STAGING_SWARM_SSH_KEY=<ssh-private-key>

PROD_SWARM_HOST=prod-swarm.example.com
PROD_SWARM_USER=deploy
PROD_SWARM_SSH_KEY=<ssh-private-key>
```

### **Notifications**
```
SLACK_WEBHOOK=https://hooks.slack.com/services/xxx/yyy/zzz
```

### **Security Scanning**
```
SNYK_TOKEN=<snyk-api-token>
```

### **Approvers**
```
PROD_APPROVERS=user1,user2,user3
```

---

## 🚀 Deployment Flow

### **Development Environment**
```
develop branch → Auto-deploy to dev.servermonitoring.com
├── Code quality checks
├── Security scanning
├── Unit tests (backend + frontend)
├── Build Docker images
└── Deploy to dev swarm
```

### **Staging Environment**
```
main branch → Auto-deploy to staging.servermonitoring.com
├── All dev checks
├── Deploy to staging swarm
├── Health checks
├── Smoke tests
└── Integration tests
```

### **Production Environment**
```
Manual workflow dispatch → Manual approval → Deploy to servermonitoring.com
├── All staging checks
├── Require 2 approvals
├── Create backup snapshot
├── Rolling update (1 container at a time)
├── Comprehensive health checks
├── Smoke tests
└── Slack notifications
```

---

## 🔄 Rollback Process

### **Automatic Rollback**
If deployment fails health checks, Docker Swarm automatically rolls back:
```yaml
update-failure-action: rollback
```

### **Manual Rollback**

#### **To Previous Version:**
```bash
# Via GitHub CLI
gh workflow run rollback.yml -f environment=production

# Or via GitHub UI
Actions → Rollback Deployment → Run workflow
```

#### **To Specific Version:**
```bash
gh workflow run rollback.yml \
  -f environment=production \
  -f rollback-to=sha-abc123
```

**Available Tags:**
- `latest` - Latest from main branch
- `main-sha-abc123` - Specific commit
- `v1.2.3` - Semantic version (if using tags)

---

## 📊 Monitoring Workflows

### **View Workflow Runs**
```bash
# List recent runs
gh run list

# View specific run
gh run view <run-id>

# Watch live run
gh run watch
```

### **Check Deployment Status**
```bash
# Check service status
ssh deploy@prod-swarm.example.com "docker service ls"

# View service logs
ssh deploy@prod-swarm.example.com "docker service logs servermonitoring_api --tail 100"
```

---

## 🎯 Workflow Best Practices

### **1. Branch Strategy**
- `develop` → Development environment (auto-deploy)
- `main` → Staging environment (auto-deploy)
- Tags/Manual → Production (requires approval)

### **2. Testing Strategy**
```
Commit → Push
  ├── Lint & Format
  ├── Unit Tests (90%+ coverage)
  ├── Security Scan
  ├── Build Docker Images
  └── Deploy to Dev
        ├── Smoke Tests
        └── Integration Tests
              └── Deploy to Staging
                    ├── Load Tests
                    └── Manual Approval
                          └── Deploy to Production
```

### **3. Approval Gates**
- **Development:** No approval needed
- **Staging:** No approval needed
- **Production:** 2 approvals required from `PROD_APPROVERS`

### **4. Health Checks**
After each deployment:
```
30s delay → Health check
  ├── /health (overall)
  ├── /health/ready (database)
  └── /health/live (app)
```

---

## 🔧 Customization

### **Modify Deployment Targets**

Edit `.github/workflows/ci-cd.yml`:
```yaml
deploy-production:
  environment:
    name: production
    url: https://your-domain.com  # Change this
```

### **Adjust Test Coverage Thresholds**

Edit test configurations:
```yaml
- name: Check coverage
  run: |
    dotnet test --collect:"XPlat Code Coverage" \
      --threshold 80  # Adjust coverage requirement
```

### **Change Deployment Strategy**

Edit Swarm update parameters:
```yaml
docker service update \
  --update-parallelism 2 \      # Update 2 at a time
  --update-delay 5s \            # 5s between updates
  --update-failure-action rollback
```

---

## 🚨 Troubleshooting

### **Deployment Failed**

1. **Check workflow logs:**
```bash
gh run view --log-failed
```

2. **Check service status:**
```bash
ssh deploy@host "docker service ps servermonitoring_api --no-trunc"
```

3. **View service logs:**
```bash
ssh deploy@host "docker service logs servermonitoring_api --tail 100"
```

### **Rollback Not Working**

1. **Check image availability:**
```bash
docker image ls | grep servermonitoring
```

2. **Manual rollback command:**
```bash
docker service rollback servermonitoring_api
```

3. **Rollback to specific image:**
```bash
docker service update \
  --image ghcr.io/your-org/servermonitoring-api:sha-abc123 \
  servermonitoring_api
```

### **Health Checks Failing**

1. **Check endpoint manually:**
```bash
curl -v https://your-domain.com/health
```

2. **Check service health in Swarm:**
```bash
docker service ps servermonitoring_api --format "{{.Name}} {{.CurrentState}}"
```

---

## 📈 Metrics & Reporting

### **Code Coverage**
Uploaded to Codecov after each test run.

### **Performance Reports**
k6 results uploaded as artifacts after load tests.

### **Security Reports**
SARIF files uploaded to GitHub Security tab.

---

## 🎓 Advanced Usage

### **Multi-Environment Deployments**

Create environment-specific configs:
```bash
.github/
  workflows/
    ci-cd.yml
  environments/
    development.yml
    staging.yml
    production.yml
```

### **Canary Deployments**

Modify service update strategy:
```yaml
docker service update \
  --update-parallelism 1 \
  --update-delay 60s \      # 1 minute between updates
  --update-order start-first
```

### **Blue-Green Deployments**

Use stack deployment:
```bash
# Deploy blue stack
docker stack deploy -c docker-compose.blue.yml servermonitoring-blue

# Switch traffic
docker service update --label-add traefik.enable=true servermonitoring-blue_api

# Remove green stack
docker stack rm servermonitoring-green
```

---

## 📚 Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Swarm Documentation](https://docs.docker.com/engine/swarm/)
- [k6 Load Testing](https://k6.io/docs/)
- [Trivy Security Scanner](https://github.com/aquasecurity/trivy)

---

## ✅ Checklist

Before using workflows:
- ✅ Configure all required secrets
- ✅ Set up Swarm hosts (dev, staging, prod)
- ✅ Configure Slack webhook for notifications
- ✅ Set production approvers list
- ✅ Test rollback procedure in staging
- ✅ Review and customize health check URLs
- ✅ Adjust resource limits if needed
- ✅ Configure monitoring dashboards

---

**Your CI/CD pipeline is production-ready!** 🚀
