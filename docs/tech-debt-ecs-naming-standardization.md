# Tech Debt: ECS Naming Convention Standardization

**Status**: Partially Resolved  
**Priority**: Medium  
**Category**: Infrastructure  
**Created**: 2025-07-07  
**Updated**: 2025-07-11
**Estimated Effort**: 4-6 hours  

## Problem Statement

The ECS infrastructure uses inconsistent naming conventions that don't align with the established standard pattern.

### Current Naming Pattern (FIXED FOR DEV)
- **Task Definition Family**: `${PROJECT_NAME}-app-${ENVIRONMENT}` → `centro-app-dev`
- **ECS Service**: `${PROJECT_NAME}-service-${ENVIRONMENT}` → `centro-service-dev` ✅
- **ECS Cluster**: `${PROJECT_NAME}-${ENVIRONMENT}-cluster` → `centro-dev-cluster` ✅
- **GitHub Actions Workflow**: Now correctly deploys to `centro-service-dev` ✅

### Standard Naming Pattern (TARGET)
All AWS resources should follow: `${PROJECT_NAME}-${ENVIRONMENT}-${FUNCTION}`

- **Task Definition Family**: `${PROJECT_NAME}-${ENVIRONMENT}-app` → `centro-dev-app`
- **ECS Service**: `${PROJECT_NAME}-${ENVIRONMENT}-service` → `centro-dev-service`
- **ECS Cluster**: `${PROJECT_NAME}-${ENVIRONMENT}-cluster` → `centro-dev-cluster` ✅

## Recent Changes

### 2025-07-11: Fixed GitHub Actions Deployment Naming
- **Issue**: GitHub Actions workflow was trying to deploy to `centro-dev-service` 
- **Actual**: ECS service is named `centro-service-dev`
- **Fix**: Updated `.github/workflows/deploy-to-ecs.yml` to use correct service name
- **Result**: Deployment pipeline now works correctly with existing infrastructure

**Commit**: `ec75d08` - "Fix ECS service naming mismatch in GitHub Actions deployment"

This resolves the immediate deployment failures while maintaining the current infrastructure naming pattern.

### Current Issues
1. **Inconsistent naming** makes resource identification harder
2. **Deviation from standards** creates confusion for new team members
3. **Infrastructure as Code** templates don't follow organization patterns
4. **Scaling to new environments** will perpetuate the inconsistency

### Risk Assessment
- **Deployment Risk**: Medium (requires careful migration)
- **Business Impact**: Low (functionality unaffected)
- **Technical Impact**: Medium (affects infrastructure clarity)

## Files Requiring Changes

### 1. ECS Task Definition
**File**: `src/Centro.Infrastructure/Ecs/task-definition.json`
```json
// Current
"family": "${PROJECT_NAME}-app-${ENVIRONMENT}"

// Target  
"family": "${PROJECT_NAME}-${ENVIRONMENT}-app"
```

### 2. GitHub Actions Workflow
**File**: `.github/workflows/deploy-to-ecs.yml`
```yaml
# Current
service: ${{ env.PROJECT_NAME }}-service-${{ github.event.inputs.environment || 'dev' }}

# Target
service: ${{ env.PROJECT_NAME }}-${{ github.event.inputs.environment || 'dev' }}-service
```

### 3. Container Name (Optional)
**File**: `src/Centro.Infrastructure/Ecs/task-definition.json`
```json
// Current
"name": "${PROJECT_NAME}-container"

// Target (for full consistency)
"name": "${PROJECT_NAME}-${ENVIRONMENT}-app-container"
```

## Migration Strategies

### Option A: Blue-Green Deployment (Recommended)
**Effort**: 4-6 hours  
**Risk**: Low  
**Downtime**: Zero  

**Steps**:
1. Update task definition and GitHub Actions files
2. Deploy new ECS service with correct naming
3. Update load balancer target group to point to new service
4. Monitor new service health
5. Delete old ECS service and task definitions
6. Update any monitoring/alerting references

**Pros**:
- Zero downtime
- Easy rollback if issues occur
- Clean migration path

**Cons**:
- Requires manual load balancer updates
- Temporary resource duplication costs

### Option B: In-Place Update
**Effort**: 2-3 hours  
**Risk**: Medium  
**Downtime**: 5-10 minutes  

**Steps**:
1. Update task definition and GitHub Actions files
2. Deploy (creates new task definition family)
3. Manually update ECS service to use new task definition
4. Wait for service to stabilize
5. Clean up old task definition revisions

**Pros**:
- Faster implementation
- No load balancer changes needed

**Cons**:
- Brief service disruption
- More complex rollback procedure
- Risk of deployment issues

### Option C: Progressive Implementation (Current Choice)
**Effort**: 1 hour  
**Risk**: None  
**Downtime**: Zero  

**Steps**:
1. Document the tech debt (this document)
2. Apply correct naming to new environments (staging, production)
3. Keep dev environment as-is for now
4. Migrate dev environment during next major infrastructure update

**Pros**:
- No immediate risk
- Prevents spreading inconsistency
- Can be addressed during planned maintenance

**Cons**:
- Dev environment remains inconsistent
- Tech debt accumulates

## Implementation Plan

### Immediate Actions (Option C - Current)
- [x] Document tech debt
- [ ] Apply correct naming to staging environment
- [ ] Apply correct naming to production environment
- [ ] Update infrastructure templates for future environments

### Future Migration (When Ready)
1. **Pre-Migration**
   - Schedule maintenance window
   - Prepare rollback procedures
   - Update monitoring dashboards with new resource names

2. **Migration** (Choose Option A or B)
   - Execute chosen migration strategy
   - Verify all services are healthy
   - Update documentation and runbooks

3. **Post-Migration**
   - Clean up old resources
   - Update monitoring/alerting
   - Verify CI/CD pipeline functionality

## Monitoring & Verification

**Resources to Check Post-Migration**:
- ECS Service health in AWS Console
- Application load balancer target groups
- CloudWatch metrics and alarms
- GitHub Actions deployment success
- Application health endpoint: `https://api.dev.centro.3degrees.xyz/health`

**Key Metrics**:
- Service running task count
- Task health check status
- API response times
- Error rates

## Related Documentation

- [Environment Configuration Guide](./ENVIRONMENT-GUIDE.md)
- [Deployment Runbook](./runbooks/deployment.md)
- [AWS Infrastructure Setup](./aws-infrastructure-setup.md)

## Decision Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2025-07-07 | Implement Option C (Progressive) | Dev environment working well, low risk approach preferred |
| 2025-07-11 | Fix GitHub Actions naming mismatch | Deployment failures due to service name mismatch - aligned workflow with actual infrastructure |
| TBD | Final migration strategy | To be decided based on business priorities and maintenance windows |

---

**Next Review**: Q2 2025 or during next major infrastructure update
**Owner**: Infrastructure Team
**Stakeholders**: DevOps, Backend Team