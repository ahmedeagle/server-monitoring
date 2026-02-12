# Secrets Management in ECS Deployment

## Overview
This application uses **AWS Secrets Manager** to securely store and manage sensitive information like API keys, passwords, and JWT secrets.

## How It Works

### 1. **GitHub Actions Workflow**
The deployment workflow automatically:
- Generates strong random secrets for JWT, database, and Redis
- Stores them in AWS Secrets Manager under the namespace `servermonitoring/*`
- Configures ECS tasks to retrieve secrets at runtime

### 2. **AWS Secrets Manager**
Secrets are stored in:
- `servermonitoring/JWT_SECRET_KEY` - JWT signing key
- `servermonitoring/DB_SA_PASSWORD` - Database password
- `servermonitoring/REDIS_PASSWORD` - Redis password

### 3. **ECS Task Definitions**
- **Environment Variables**: Non-sensitive config (ports, URLs) are set as regular environment variables
- **Secrets**: Sensitive data is referenced from Secrets Manager using ARNs
- **IAM**: The execution role has `SecretsManagerReadWrite` policy to fetch secrets

### 4. **Security Benefits**
✅ Secrets are **never** stored in code or GitHub  
✅ Secrets are **encrypted at rest** in AWS Secrets Manager  
✅ Secrets are **injected at runtime** into containers  
✅ Different secrets per environment (dev/staging/prod)  
✅ Automatic rotation support  

## Viewing Secrets

To view or update secrets in AWS Console:
1. Go to AWS Console → Secrets Manager
2. Search for `servermonitoring/`
3. Click on a secret to view/edit

## Viewing Secrets via CLI

```bash
# List all secrets
aws secretsmanager list-secrets --query "SecretList[?starts_with(Name, 'servermonitoring/')].Name"

# Get a specific secret value
aws secretsmanager get-secret-value --secret-id servermonitoring/JWT_SECRET_KEY --query SecretString --output text
```

## Adding New Secrets

### Option 1: Update GitHub Actions Workflow
Add to the "Create/Update secrets" step in [.github/workflows/deploy-to-ecs.yml](.github/workflows/deploy-to-ecs.yml):

```yaml
NEW_SECRET=$(openssl rand -base64 32)
aws secretsmanager create-secret \
  --name servermonitoring/NEW_SECRET_NAME \
  --secret-string "$NEW_SECRET" 2>/dev/null
```

### Option 2: Manual via AWS Console
1. Go to Secrets Manager → Store a new secret
2. Select "Other type of secret"
3. Name: `servermonitoring/YOUR_SECRET_NAME`
4. Value: Your secret value

### Option 3: AWS CLI
```bash
aws secretsmanager create-secret \
  --name servermonitoring/YOUR_SECRET_NAME \
  --secret-string "your-secret-value"
```

## Using Secrets in Task Definitions

Update the task definition JSON to reference the secret:

```json
"secrets": [
  {
    "name": "YOUR_ENV_VAR_NAME",
    "valueFrom": "arn:aws:secretsmanager:us-east-1:ACCOUNT_ID:secret:servermonitoring/YOUR_SECRET_NAME"
  }
]
```

## Environment Files (.env)

⚠️ **IMPORTANT**: `.env` files are now blocked from being committed to Git.

- `.env.example` - Template file (safe to commit)
- `.env.production` - Local template (NOT committed)
- `.env` - Local development (NOT committed)

For local development:
1. Copy `.env.example` to `.env`
2. Fill in your local values
3. Never commit `.env` files!

## Best Practices

1. **Never** hardcode secrets in code
2. **Never** commit `.env` files with real secrets
3. **Always** use Secrets Manager for production
4. **Rotate** secrets regularly (Secrets Manager supports auto-rotation)
5. **Use different secrets** for dev/staging/prod environments
6. **Monitor** secret access via CloudWatch and CloudTrail

## Troubleshooting

### "ECS cannot pull secrets" error
**Cause**: Execution role lacks permission  
**Fix**: Ensure `ecsTaskExecutionRole-ServerMonitoring` has `SecretsManagerReadWrite` policy attached

### "Secret not found" error
**Cause**: Secret doesn't exist in Secrets Manager  
**Fix**: Run the GitHub Actions workflow to create secrets, or create manually

### Secret ARN format
Must be: `arn:aws:secretsmanager:REGION:ACCOUNT_ID:secret:NAME`  
Don't include the `-XXXXX` version suffix at the end!

## Cost
AWS Secrets Manager: $0.40 per secret per month + $0.05 per 10,000 API calls  
Typical cost for this app: ~$2-3/month
