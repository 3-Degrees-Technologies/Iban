# Tenant Provisioning Guide

## Overview

This guide covers how to provision tenants in your LocalStack development environment using the automated provisioning script. Tenant provisioning creates the necessary secrets, registry entries, and configurations for a tenant to use Centro services.

## Prerequisites

- LocalStack running (see [LocalStack Setup](./localstack-setup.md))
- AWS CLI configured for LocalStack
- jq installed
- Tenant configuration file

## Tenant Configuration File Format

Tenant configurations are stored in JSON files with the following structure:

```json
{
  "tenant": {
    "id": "threedegrees",
    "name": "3 Degrees Technologies Inc",
    "display_name": "3 Degrees Technologies Inc",
    "tier": "community",
    "compliance_regions": ["US", "EU", "UK"],
    "created_at": "2025-07-03T06:30:00Z",
    "status": "provisioning",
    "contact": {
      "primary_email": "hello@3degrees.co",
      "technical_email": "sam@3degrees.co",
      "phone": ""
    }
  },
  "provider_credentials": [
    {
      "provider": "wise",
      "enabled": true,
      "api_key": "4870ef4b-c995-46eb-9044-bd39b3c1d417",
      "profile_id": "esteban@3degrees.co",
      "environment": "production",
      "webhook_key": "mvfcu_wise_webhook_123...",
      "permissions": ["quotes", "payments", "balances"],
      "limits": {
        "max_payment_amount_usd": 10000,
        "daily_volume_limit_usd": 100000
      }
    },
    {
      "provider": "nium",
      "enabled": false,
      "client_id": "",
      "client_secret": "",
      "environment": "sandbox",
      "permissions": []
    }
  ],
  "api_keys": [
    {
      "name": "member_portal",
      "description": "Member online banking",
      "rate_limit_tier": "standard",
      "compliance": {
        "ip_restrictions": []
      }
    }
  ],
  "features": {
    "balance_monitoring": true,
    "automated_reconciliation": false,
    "transaction_reporting": true,
    "webhook_notifications": {
      "enabled": true,
      "endpoints": [
        {
          "url": "https://admin.3degrees.co/webhooks/payments",
          "events": ["payment.status_changed", "payment.failed"],
          "secret": "3degrees_webhook_secret_456..."
        }
      ]
    }
  },
  "compliance": {
    "data_retention_days": 2555,
    "audit_trail_enabled": true,
    "data_residency": "US",
    "reporting": {
      "daily_summary": false,
      "monthly_regulatory": true
    },
    "kyc_aml_provider": "internal"
  }
}
```

### Key Fields

- **tenant.id**: Unique identifier for the tenant (used in URLs and database keys)
- **tenant.name**: Human-readable tenant name
- **provider_credentials**: Array of payment provider configurations
  - **provider**: Provider name (wise, nium, currencycloud, etc.)
  - **enabled**: Whether this provider is active for the tenant
  - **api_key**: Provider API key
  - **profile_id**: Provider profile/account identifier

## Provisioning Script Usage

The provisioning script is located at `scripts/provision-tenant-localstack.sh`.

### Basic Usage

```bash
# Provision a tenant
./scripts/provision-tenant-localstack.sh <tenant_config_file> [create|delete]

# Examples
./scripts/provision-tenant-localstack.sh ~/Workspace/tenancy/tenant_config_3_degrees_fixed.json create
./scripts/provision-tenant-localstack.sh test-tenant-config.json delete
```

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `LOCALSTACK_ENDPOINT` | `http://localhost:4566` | LocalStack endpoint URL |
| `AWS_REGION` | `eu-west-1` | AWS region for resources |

### Script Parameters

- **tenant_config_file**: Path to tenant configuration JSON file
- **mode**: `create` (default) or `delete`

## Provisioning Process

When you run the script with `create` mode, it performs these steps:

### 1. Validation
- Checks LocalStack connectivity
- Validates tenant configuration file
- Extracts tenant ID and name

### 2. Secrets Creation
For each enabled provider:
- Creates secret in AWS Secrets Manager
- Secret name format: `centro/dev/{tenant_id}/{provider}`
- Stores provider credentials (API keys, etc.)

Example secret:
```bash
# Secret name: centro/dev/threedegrees/wise
# Secret value: "4870ef4b-c995-46eb-9044-bd39b3c1d417"
```

### 3. DynamoDB Registry Entry
Creates entry in `centro-dev-tenant-registry` table:
```json
{
  "TenantId": "threedegrees",
  "TenantName": "3 Degrees Technologies Inc",
  "Environment": "dev",
  "WiseProfileId": "esteban@3degrees.co",
  "WiseSecretsArn": "arn:aws:secretsmanager:eu-west-1:000000000000:secret:centro/dev/threedegrees/wise-abcdef",
  "CreatedAt": "2025-07-05T18:52:19Z"
}
```

### 4. Testing
The script automatically tests the provisioned configuration:
- Retrieves secrets to verify access
- Queries DynamoDB registry to verify tenant lookup

## Example Provisioning Session

```bash
$ ./scripts/provision-tenant-localstack.sh tenant_config_3_degrees_fixed.json create

[2025-07-05 19:55:53] === LocalStack Tenant Provisioning ===
[2025-07-05 19:55:53] Mode: create
[2025-07-05 19:55:53] Tenant ID: threedegrees
[2025-07-05 19:55:53] Tenant Name: 3 Degrees Technologies Inc
[2025-07-05 19:55:53] Config File: tenant_config_3_degrees_fixed.json
[2025-07-05 19:55:53] LocalStack: http://localhost:4566
[2025-07-05 19:55:53] Region: eu-west-1
[2025-07-05 19:55:53] Checking LocalStack connectivity...
[2025-07-05 19:55:53] ✅ LocalStack is running
[2025-07-05 19:55:53] === CREATING TENANT ===
[2025-07-05 19:55:53] Creating provider secrets for tenant 'threedegrees'...
[2025-07-05 19:55:53] Creating secret: centro/dev/threedegrees/wise
[2025-07-05 19:55:53]   Wise API Key: 4870ef4b-c995-46eb-9044-bd39b3c1d417
[2025-07-05 19:55:53]   Wise Profile ID: esteban@3degrees.co
[2025-07-05 19:55:53] ✅ Created secret: centro/dev/threedegrees/wise
[2025-07-05 19:55:54]   ARN: arn:aws:secretsmanager:eu-west-1:000000000000:secret:centro/dev/threedegrees/wise-qglNDt
[2025-07-05 19:55:54] === SECRETS CREATED ===
[2025-07-05 19:55:54] ✅ centro/dev/threedegrees/wise
[2025-07-05 19:55:54] Creating DynamoDB registry entry for tenant 'threedegrees'...
[2025-07-05 19:55:54]   Wise Profile ID: esteban@3degrees.co
[2025-07-05 19:55:54]   Wise Secrets ARN: arn:aws:secretsmanager:eu-west-1:000000000000:secret:centro/dev/threedegrees/wise-abcdef
[2025-07-05 19:55:55] ✅ Created DynamoDB registry entry for tenant: threedegrees
[2025-07-05 19:55:55] ✅ Tenant creation completed successfully!
[2025-07-05 19:55:55] === TESTING CONFIGURATION ===
[2025-07-05 19:55:55] Testing tenant configuration...
[2025-07-05 19:55:55] Testing secret retrieval...
[2025-07-05 19:55:55] ✅ Secret retrieval test passed
[2025-07-05 19:55:55]   Secret: centro/dev/threedegrees/wise
[2025-07-05 19:55:55]   Value: "4870ef4b-c995-46eb-...
[2025-07-05 19:55:55] Testing DynamoDB registry lookup...
[2025-07-05 19:55:56] ✅ DynamoDB registry test passed
[2025-07-05 19:55:56]   Tenant: threedegrees
[2025-07-05 19:55:56]   Wise Profile: esteban@3degrees.co
[2025-07-05 19:55:56] === NEXT STEPS ===
[2025-07-05 19:55:56] 1. Restart your Centro API: cd src/Centro.Api && dotnet run
[2025-07-05 19:55:56] 2. Test the real Wise integration:
[2025-07-05 19:55:56]    curl http://localhost:5139/api/balance/threedegrees
[2025-07-05 19:55:56] 3. Check logs for real Wise API calls vs mock responses
[2025-07-05 19:55:56] === DONE ===
```

## Manual Verification

After provisioning, you can manually verify the setup:

### Check Secrets
```bash
# List all secrets
aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets --region eu-west-1

# Get specific secret
aws --endpoint-url=http://localhost:4566 secretsmanager get-secret-value \
  --secret-id "centro/dev/threedegrees/wise" \
  --region eu-west-1
```

### Check DynamoDB Registry
```bash
# Scan tenant registry table
aws --endpoint-url=http://localhost:4566 dynamodb scan \
  --table-name centro-dev-tenant-registry \
  --region eu-west-1

# Get specific tenant
aws --endpoint-url=http://localhost:4566 dynamodb get-item \
  --table-name "centro-dev-tenant-registry" \
  --key '{"TenantId":{"S":"threedegrees"}}' \
  --region eu-west-1
```

## Tenant Deletion

To remove a tenant from LocalStack:

```bash
./scripts/provision-tenant-localstack.sh tenant_config_file.json delete
```

This will:
- Delete all secrets for the tenant
- Remove the DynamoDB registry entry
- Require confirmation before proceeding

## Testing with Centro API

After provisioning a tenant:

1. **Start the Centro API**:
   ```bash
   cd src/Centro.Api
   dotnet run
   ```

2. **Test tenant endpoints**:
   ```bash
   # Test balance endpoint
   curl http://localhost:5139/api/balance/threedegrees
   
   # Test health with tenant context
   curl http://localhost:5139/health
   ```

3. **Check logs** for:
   - Successful tenant resolution
   - Provider API calls (vs mock responses)
   - Secret retrieval from LocalStack

## Common Configuration Patterns

### Multiple Providers
```json
"provider_credentials": [
  {
    "provider": "wise",
    "enabled": true,
    "api_key": "wise-key-123",
    "profile_id": "wise-profile-456"
  },
  {
    "provider": "nium", 
    "enabled": true,
    "client_id": "nium-client-789",
    "client_secret": "nium-secret-abc"
  }
]
```

### Test vs Production Credentials
```json
{
  "provider": "wise",
  "enabled": true,
  "api_key": "test-key-for-dev",
  "profile_id": "test-profile",
  "environment": "sandbox"
}
```

## Best Practices

1. **Use descriptive tenant IDs** - they appear in URLs and logs
2. **Keep test credentials separate** - don't use production API keys in LocalStack
3. **Enable only needed providers** - reduces setup complexity
4. **Verify after provisioning** - always test the tenant works as expected
5. **Clean up test tenants** - delete temporary tenants to keep LocalStack clean

## Related Documentation

- [LocalStack Setup](./localstack-setup.md)
- [Secrets Management](../knowledge/secrets-management.md)
- [Tenant Configuration](../knowledge/tenant-configuration.md)
- [Troubleshooting](./localstack-troubleshooting.md)