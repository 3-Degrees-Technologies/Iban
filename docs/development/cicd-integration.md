# CI/CD Integration with Environment-Based Branching

## Overview

Centro's CI/CD pipeline now automatically maps git branches to AWS environments, ensuring consistent and safe deployment practices that align with our development workflow.

## Branch to Environment Mapping

### Automatic Deployments
| Branch | Environment | AWS Resources | Trigger |
|--------|-------------|---------------|---------|
| `main` | `production` | `centro-production-*` | Push to main |
| `test` | `test` | `centro-test-*` | Push to test |
| `dev` | `dev` | `centro-dev-*` | Push to dev |
| `local-*` | None | No deployment | Local development only |

### Manual Deployments
- **Workflow Dispatch**: Manual trigger with environment selection
- **Override Capability**: Can deploy any branch to any environment
- **Emergency Hotfixes**: Direct deployment to production if needed

## Deployment Flow

### 1. Development Phase
```bash
# Work on local-sam branch
git checkout local-sam
git commit -m "Add new corridor optimization"
git push origin local-sam
# ❌ No deployment triggered (safe development)
```

### 2. Integration Phase  
```bash
# Merge to dev branch
gh pr create --base dev --head local-sam
gh pr merge --squash
# ✅ Automatically deploys to DEV environment
```

### 3. Staging Phase
```bash
# Merge to test branch  
gh pr create --base test --head dev
gh pr merge --squash
# ✅ Automatically deploys to TEST environment
```

### 4. Production Phase
```bash
# Merge to main branch
gh pr create --base main --head test  
gh pr merge --squash
# ✅ Automatically deploys to PRODUCTION environment
```

## GitHub Actions Configuration

### Environment-Aware Triggers
```yaml
on:
  push:
    branches: 
      - main      # → production environment
      - test      # → test environment  
      - dev       # → dev environment
    paths-ignore:
      - '**.md'
      - 'docs/**'
      - 'bruno/**'
      - 'scripts/**'
```

### Automatic Environment Detection
The workflow automatically determines the target environment:
```yaml
env:
  ENVIRONMENT: ${{ 
    github.event.inputs.environment ||           # Manual override
    (github.ref_name == 'main' && 'production') || 
    (github.ref_name == 'test' && 'test') || 
    (github.ref_name == 'dev' && 'dev') || 
    'dev'  # Default fallback
  }}
```

### AWS Resource Mapping
Each environment deploys to its own isolated AWS resources:
```yaml
# ECS Services
centro-dev-cluster → centro-service-dev
centro-test-cluster → centro-service-test  
centro-production-cluster → centro-service-production

# ECR Repositories
dev/centro:sha
test/centro:sha
production/centro:sha

# IAM Roles
centro-dev-github-actions-role
centro-test-github-actions-role
centro-production-github-actions-role
```

## Security and Access Control

### Environment Protection Rules
- **Production**: Requires 2 reviewers + status checks
- **Test**: Requires 1 reviewer + status checks
- **Dev**: Status checks only
- **Local branches**: No restrictions

### GitHub Environment Secrets
Each environment has isolated secrets and variables:
```
production environment:
  - AWS_ACCOUNT_ID
  - WISE_API_PRODUCTION_ENDPOINT
  - DATABASE_CONNECTION_STRING

test environment:  
  - AWS_ACCOUNT_ID
  - WISE_API_SANDBOX_ENDPOINT
  - DATABASE_CONNECTION_STRING

dev environment:
  - AWS_ACCOUNT_ID  
  - WISE_API_SANDBOX_ENDPOINT
  - DATABASE_CONNECTION_STRING
```

### IAM Role Assumptions
Each environment uses dedicated IAM roles with minimal permissions:
- `centro-production-github-actions-role` - Production ECS/ECR access only
- `centro-test-github-actions-role` - Test ECS/ECR access only  
- `centro-dev-github-actions-role` - Dev ECS/ECR access only

## Deployment Safety Features

### Path Ignores
Deployments skip non-critical changes:
```yaml
paths-ignore:
  - '**.md'           # Documentation changes
  - 'docs/**'         # Documentation updates
  - 'bruno/**'        # API test changes
  - 'scripts/**'      # Local development scripts
```

### Health Checks
Each deployment includes comprehensive health validation:
1. **ECS Service Stability** - Wait for service to reach steady state
2. **ALB Target Health** - Verify targets are healthy
3. **Application Health** - Test health endpoints
4. **Rollback Capability** - Automatic rollback on failure

### Environment Isolation
- **Network Isolation**: Each environment in separate VPCs
- **Database Isolation**: Separate RDS instances per environment  
- **Secrets Isolation**: Environment-specific secrets management
- **Resource Tagging**: Clear environment identification

## Manual Override Capabilities

### Workflow Dispatch
For emergency deployments or testing:
```bash
# Deploy specific branch to specific environment
gh workflow run deploy-to-ecs.yml \
  --ref feature/hotfix-branch \
  --field environment=production \
  --field aws_region=eu-west-1
```

### Emergency Hotfix Process
```bash
# Create hotfix from main
git checkout main
git checkout -b hotfix/critical-fix
git commit -m "Fix critical security issue"
git push origin hotfix/critical-fix

# Deploy directly to production (manual)
gh workflow run deploy-to-ecs.yml \
  --ref hotfix/critical-fix \
  --field environment=production

# Then follow normal PR process to integrate fix
gh pr create --base main --head hotfix/critical-fix
```

## Monitoring and Observability

### Deployment Tracking
Each deployment creates CloudWatch metrics and logs:
- **Deployment Duration** - Time to complete deployment
- **Success/Failure Rates** - Deployment reliability metrics
- **Environment Health** - Post-deployment health status
- **Resource Utilization** - CPU/memory usage per environment

### GitHub Deployment Status
- **Environment Status** - Current deployment state per environment
- **Commit Tracking** - Which commit is deployed where
- **Rollback History** - Previous deployment versions
- **Actor Tracking** - Who triggered each deployment

### Slack Notifications
CI/CD events are posted to `#deployments` channel:
- ✅ Successful deployments
- ❌ Failed deployments  
- ⚠️ Health check warnings
- 🚀 Manual deployments

## Best Practices

### Pre-Deployment Checklist
- ✅ Unit tests pass locally
- ✅ Integration tests pass in dev
- ✅ Performance tests pass in test
- ✅ Security scans complete
- ✅ Database migrations tested

### Post-Deployment Validation
- ✅ Health endpoints responding
- ✅ Key business flows functional
- ✅ Error rates within acceptable limits
- ✅ Performance metrics stable
- ✅ Security headers present

### Rollback Procedures
```bash
# Quick rollback to previous version
aws ecs update-service \
  --cluster centro-production-cluster \
  --service centro-service-production \
  --task-definition centro-production:PREVIOUS_REVISION

# Or trigger redeployment of last known good commit
gh workflow run deploy-to-ecs.yml \
  --ref LAST_GOOD_COMMIT_SHA \
  --field environment=production
```

## Troubleshooting

### Common Issues

#### Deployment Fails with Permission Error
```bash
# Check IAM role has correct permissions
aws sts get-caller-identity
aws iam get-role --role-name centro-dev-github-actions-role
```

#### Environment Variables Not Set
```bash
# Verify GitHub environment secrets
gh secret list --env dev
gh variable list --env dev
```

#### ECS Service Won't Stabilize
```bash
# Check ECS service events
aws ecs describe-services \
  --cluster centro-dev-cluster \
  --services centro-service-dev

# Check CloudWatch logs
aws logs describe-log-groups --log-group-name-prefix="/aws/ecs/centro"
```

#### Wrong Environment Deployed
- Check branch name matches expected mapping
- Verify workflow_dispatch inputs if manual trigger
- Review GitHub Actions logs for environment detection

### Emergency Procedures

#### Stop All Deployments
```bash
# Cancel running workflows
gh run list --workflow=deploy-to-ecs.yml --status=in_progress
gh run cancel RUN_ID
```

#### Emergency Rollback
```bash
# Immediate ECS service rollback
aws ecs update-service \
  --cluster centro-production-cluster \
  --service centro-service-production \
  --force-new-deployment \
  --task-definition centro-production:PREVIOUS_REVISION
```

## Integration Benefits

### Development Velocity
- ✅ **Faster feedback loops** - Automatic dev deployments
- ✅ **Reduced manual steps** - No manual environment management  
- ✅ **Clear promotion path** - Structured dev→test→prod flow
- ✅ **Safe experimentation** - Isolated local branches

### Operational Reliability  
- ✅ **Environment consistency** - Same deployment process everywhere
- ✅ **Reduced human error** - Automated environment detection
- ✅ **Audit trail** - Complete deployment history
- ✅ **Rollback capability** - Quick recovery from issues

### Security Compliance
- ✅ **Principle of least privilege** - Environment-specific IAM roles
- ✅ **Secrets isolation** - Environment-specific secrets
- ✅ **Approval requirements** - Protected production deployments
- ✅ **Change tracking** - Full audit trail of all changes

This CI/CD integration ensures that our environment-based branching strategy is fully automated, secure, and reliable while maintaining the flexibility needed for emergency situations.