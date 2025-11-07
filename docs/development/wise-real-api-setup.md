# Real Wise API Integration with LocalStack

This guide shows how to configure Centro to hit the **real Wise sandbox API** while using LocalStack for AWS services (DynamoDB, Secrets Manager).

## Problem We're Solving

By default, Centro runs in "Mock Mode" which returns fake data. To test real Wise API integration during development, we need:

1. **Real Wise sandbox credentials** stored in LocalStack Secrets Manager
2. **Real tenant configuration** in LocalStack DynamoDB  
3. **Centro configured** to use real WiseApiClient instead of mocks

## Prerequisites

- Centro project cloned
- LocalStack running (`docker-compose up -d` or podman scripts)
- AWS CLI configured for LocalStack
- Real Wise sandbox account with API token

## Step 1: Start LocalStack

```bash
# Option 1: Docker Compose
docker-compose up -d

# Option 2: Podman script  
./scripts/start-localstack-podman.sh

# Verify LocalStack is running
curl http://localhost:4566/health
```

## Step 2: Configure Real Wise API (Not Mocks)

Edit `src/Centro.Api/appsettings.Development.json`:

```json
{
  "AWS": {
    "UseLocalStack": true,
    "LocalStackEndpoint": "http://localhost:4566",
    "Region": "eu-west-1"
  },
  "Providers": {
    "Wise": {
      "BaseUrl": "https://api.sandbox.transferwise.tech",
      "UseMock": false
    }
  }
}
```

**Key Changes:**
- `AWS:UseLocalStack: true` - Use LocalStack for AWS services (DynamoDB, Secrets Manager)
- `Providers:Wise:UseMock: false` - Use **real** WiseApiClient, not mocks

## Step 3: Provision Tenant with Real Credentials

Use the automated script with real Wise credentials:

```bash
./scripts/provision-tenant-localstack.sh ~/Workspace/tenancy/tenant_config_3_degrees_fixed.json create
```

This script:
1. **Reads real credentials** from `tenant_config_3_degrees_fixed.json`:
   - API Key: `4870ef4b-c995-46eb-9044-bd39b3c1d417`
   - Profile ID: `esteban@3degrees.co`
2. **Stores in LocalStack Secrets Manager**: `centro/dev/threedegrees/wise`
3. **Creates DynamoDB registry entry** with proper ARNs and profile mapping
4. **Tests the configuration** to ensure everything works

## Step 4: Restart Centro API

```bash
cd src/Centro.Api
dotnet run
```

The API will now:
- Use **LocalStack** for AWS services (secrets, DynamoDB) 
- Use **real WiseApiClient** to hit Wise sandbox
- Load **real credentials** from LocalStack

## Step 5: Test Real Wise Integration

```bash
# Test the balance endpoint (should hit real Wise API)
curl http://localhost:5139/api/balance/threedegrees
```

**Expected Behavior:**
- API logs show calls to `https://api.sandbox.transferwise.tech`
- Response contains **real balance data** from Wise sandbox
- No more mock data with hardcoded amounts

## Verification

### Check LocalStack Secrets
```bash
# List secrets
aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets --region eu-west-1

# Get Wise secret value
aws --endpoint-url=http://localhost:4566 secretsmanager get-secret-value \
  --secret-id "centro/dev/threedegrees/wise" \
  --region eu-west-1
```

### Check DynamoDB Registry  
```bash
# Get tenant registry entry
aws --endpoint-url=http://localhost:4566 dynamodb get-item \
  --table-name "centro-dev-tenant-registry" \
  --key '{"TenantId":{"S":"threedegrees"}}' \
  --region eu-west-1
```

### Check API Logs
Look for these log entries indicating real API calls:
```
[Debug] Calling Wise API: v4/profiles/esteban@3degrees.co/balances?types=STANDARD
[Debug] Successfully retrieved X balances from Wise API
```

## Configuration Summary

| Component | Configuration | Purpose |
|-----------|---------------|---------|
| **AWS Services** | `UseLocalStack: true` | DynamoDB + Secrets Manager via LocalStack |
| **Wise API** | `UseMock: false` | Real Wise sandbox API calls |
| **Credentials** | LocalStack Secrets Manager | Real API token stored locally |
| **Tenant Registry** | LocalStack DynamoDB | Real profile ID mapping |

## Troubleshooting

### "Mock data still showing"
- Restart the API after configuration changes
- Verify `Providers:Wise:UseMock: false` in appsettings
- Check API logs for Wise API calls

### "401 Unauthorized from Wise"  
- Verify API token is correct in tenant config
- Check Wise sandbox account status
- Ensure profile ID matches your Wise account

### "Tenant not found"
- Run the provisioning script to create tenant in LocalStack
- Verify DynamoDB registry entry exists
- Check tenant ID spelling matches exactly

### "Cannot connect to LocalStack"
- Ensure LocalStack is running on port 4566
- Check `LocalStackEndpoint` configuration
- Verify AWS CLI can connect: `aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets`

## Clean Up

To remove the tenant configuration:

```bash
./scripts/provision-tenant-localstack.sh ~/Workspace/tenancy/tenant_config_3_degrees_fixed.json delete
```

## Next Steps

1. **Add more providers**: Extend script to support Nium, CurrencyCloud
2. **Production deployment**: Use same tenant config with real AWS
3. **Integration tests**: Automate testing against real APIs
4. **Monitoring**: Add metrics for real API response times

---

*This setup gives you the best of both worlds: real API testing with local AWS services for fast development iteration.*