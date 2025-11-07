# API Gateway Route Configuration

This document explains how to configure API Gateway routes for the Centro API using the automated GitHub Actions workflow.

## Overview

The Centro API is deployed to ECS and accessible via an Application Load Balancer (ALB). To make it publicly accessible, we need to configure API Gateway routes that forward requests to the ALB.

## Current API Structure

### Centro API Endpoints (ECS/ALB)
```
GET /api/v1/balance/{tenantId}  - Get balances for a tenant
GET /health                     - Basic health check  
GET /health/ready              - Readiness check
GET /health/live               - Liveness check
```

### API Gateway Routes (Public)
```
GET /health                     - Health check (configured) ✅
GET /api/v1/balance/{tenantId} - Balance endpoint (needs configuration) ⚠️
GET /api/v1/quotes             - Legacy route (unused) ⚠️
POST /api/v1/payments          - Legacy route (unused) ⚠️
GET /api/v1/payments/{id}      - Legacy route (unused) ⚠️
```

## Route Configuration Workflow

### Automated Setup
Use the GitHub Actions workflow to automatically configure the balance endpoint:

1. **Trigger the workflow:**
   ```bash
   # Via GitHub UI: Actions → Update API Gateway Configuration → Run workflow
   # Select environment (dev/test/staging/prod) and region
   ```

2. **The workflow will:**
   - ✅ Create `/api/v1/balance` resource
   - ✅ Create `/api/v1/balance/{tenantId}` resource  
   - ✅ Configure GET method with API key requirement
   - ✅ Set up integration to ALB endpoint
   - ✅ Configure CORS with OPTIONS method
   - ✅ Deploy changes to API Gateway

### Manual Configuration (if needed)
If you need to configure routes manually:

```bash
# Get API Gateway ID
API_ID=$(aws apigateway get-rest-apis --query "items[?name=='centro-dev-api'].id" --output text)

# Get ALB DNS name
ALB_DNS=$(aws cloudformation list-exports --query "Exports[?Name=='centro-dev-alb-dns'].Value" --output text)

# Create balance resource (if not exists)
PARENT_ID=$(aws apigateway get-resources --rest-api-id $API_ID --query "items[?path=='/api/v1'].id" --output text)
BALANCE_ID=$(aws apigateway create-resource --rest-api-id $API_ID --parent-id $PARENT_ID --path-part "balance" --query 'id' --output text)

# Create {tenantId} resource
TENANT_ID=$(aws apigateway create-resource --rest-api-id $API_ID --parent-id $BALANCE_ID --path-part "{tenantId}" --query 'id' --output text)

# Configure GET method
aws apigateway put-method --rest-api-id $API_ID --resource-id $TENANT_ID --http-method GET --authorization-type NONE --api-key-required

# Configure integration
aws apigateway put-integration --rest-api-id $API_ID --resource-id $TENANT_ID --http-method GET --type HTTP_PROXY --integration-http-method GET --uri "http://$ALB_DNS/api/v1/balance/{tenantId}"

# Deploy changes
aws apigateway create-deployment --rest-api-id $API_ID --stage-name v1
```

## Authentication & Authorization

### API Key Requirements
- All API endpoints require an API key
- API keys are managed through the tenant provisioning system
- Keys are associated with specific tenants for access control

### Headers Required
```
X-Api-Key: [API key from tenant provisioning]
X-Tenant-ID: [tenant identifier, e.g., "threedegrees"]
```

### Example Request
```bash
curl -H "X-Api-Key: YOUR_API_KEY" \
     -H "X-Tenant-ID: threedegrees" \
     "https://API_ID.execute-api.eu-west-1.amazonaws.com/v1/api/v1/balance/threedegrees"
```

## Route Mapping

| API Gateway Route | Centro ECS Endpoint | Purpose | Status |
|-------------------|-------------------|---------|---------|
| `GET /health` | `GET /health` | Health check | ✅ Configured |
| `GET /api/v1/balance/{tenantId}` | `GET /api/v1/balance/{tenantId}` | Get balances | ⚠️ Use workflow |
| `GET /api/v1/quotes` | Not implemented | Legacy | ❌ Remove |
| `POST /api/v1/payments` | Not implemented | Legacy | ❌ Remove |
| `GET /api/v1/payments/{id}` | Not implemented | Legacy | ❌ Remove |

## Next Steps

1. **Run the API Gateway configuration workflow** to add the balance endpoint
2. **Provision API keys** for tenant access using the tenancy scripts
3. **Test the endpoint** with proper authentication headers
4. **Remove unused routes** (quotes/payments) if not needed
5. **Add additional endpoints** as Centro API grows (quotes, payments, transfers, etc.)

## Troubleshooting

### Common Issues

**"API Gateway not found"**
- Ensure the environment name matches the deployed infrastructure
- Check CloudFormation stack status

**"ALB DNS not found"** 
- Verify the ALB is deployed and healthy
- Check CloudFormation exports

**"Integration failed"**
- Verify ECS service is running and healthy  
- Check ALB target group health
- Ensure security groups allow traffic

**"API key required"**
- Use the tenant provisioning scripts to create API keys
- Include X-Api-Key header in requests
- Verify API key is active and not expired

### Verification Commands

```bash
# Check API Gateway status
aws apigateway get-rest-apis --query "items[?name=='centro-dev-api']"

# Check ALB health
aws elbv2 describe-target-health --target-group-arn "TARGET_GROUP_ARN"

# Check ECS service status  
aws ecs describe-services --cluster centro-dev-cluster --services centro-service-dev

# Test health endpoint (no auth required)
curl "https://API_ID.execute-api.eu-west-1.amazonaws.com/v1/health"
```