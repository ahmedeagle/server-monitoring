# ALB Connection Timeout - Root Cause & Fix

## Problem
ALB returning ERR_CONNECTION_TIMED_OUT when accessed from browser

## Root Cause
ECS Services were created **WITHOUT** load balancer configuration initially.

When services already exist, the GitHub Actions workflow runs:
```bash
aws ecs update-service \
  --task-definition NEW_VERSION \
  --force-new-deployment
```

**CRITICAL**: ECS service load balancer configuration is **IMMUTABLE**. You cannot add `--load-balancers` to an existing service via update.

Result:
- ALB exists ✅
- Target groups exist ✅  
- Services exist ✅
- **BUT**: Tasks are NOT registered with target groups ❌
- ALB has NO HEALTHY TARGETS → Connection timeout

## Solution
1. **Delete both ECS services** (forces recreation)
2. **Trigger new deployment** (GitHub Actions)
3. Services recreate with proper `--load-balancers` parameter:
   ```bash
   aws ecs create-service \
     --load-balancers "targetGroupArn=$API_TG_ARN,containerName=api,containerPort=8080"
   ```
4. Tasks automatically register with target groups
5. Health checks pass after ~2-3 minutes
6. ALB becomes accessible! 🎉

## Files Affected
- `.github/workflows/deploy-to-ecs.yml` - Contains service creation logic  
- Services: `servermonitoring-api-service`, `servermonitoring-web-service`

## Prevention
Once services are recreated  with proper ALB configuration, future deployments will work correctly since the load balancer config persists across updates.
