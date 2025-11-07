# Tenant Configuration

## Overview
Knowledge about how tenants are configured and managed in the Centro platform, including both production and local development environments.

## Tenant Identification
- Tenants are identified by unique tenant IDs (e.g., "threedegrees", "creditunion")
- Configuration is environment-specific (dev, staging, production)
- Tenant IDs appear in URLs, database keys, and logging context

## Configuration Sources

### DynamoDB Tenant Registry
The primary source of tenant configuration is stored in DynamoDB:

#### Production Table: `centro-{environment}-tenant-registry`
#### LocalStack Table: `centro-dev-tenant-registry`

**Example Registry Entry:**
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

### Database Configuration
- Tenant-specific database connections stored in Secrets Manager
- Connection strings follow pattern: `Server=...;Database=centro_{tenant_id};...`
- See `DatabaseOptions.cs` for configuration binding

### Provider Configuration
Each tenant can have multiple payment provider configurations:

```json
{
  "provider_credentials": [
    {
      "provider": "wise",
      "enabled": true,
      "api_key": "4870ef4b-c995-46eb-9044-bd39b3c1d417",
      "profile_id": "esteban@3degrees.co",
      "environment": "production"
    },
    {
      "provider": "nium", 
      "enabled": false,
      "client_id": "",
      "client_secret": ""
    }
  ]
}
```

## Local Development (LocalStack)

### Tenant Provisioning
Tenants are provisioned in LocalStack using configuration files:

```bash
# Provision tenant from config file
./scripts/provision-tenant-localstack.sh ~/Workspace/tenancy/tenant_config_3_degrees_fixed.json create

# Verify tenant registry
aws --endpoint-url=http://localhost:4566 dynamodb scan \
  --table-name centro-dev-tenant-registry --region eu-west-1
```

### Example Tenant Config File
```json
{
  "tenant": {
    "id": "threedegrees",
    "name": "3 Degrees Technologies Inc",
    "tier": "community",
    "status": "provisioning"
  },
  "provider_credentials": [
    {
      "provider": "wise",
      "enabled": true,
      "api_key": "4870ef4b-c995-46eb-9044-bd39b3c1d417",
      "profile_id": "esteban@3degrees.co"
    }
  ],
  "features": {
    "balance_monitoring": true,
    "transaction_reporting": true
  }
}
```

### Mock vs Real Configuration
- **Mock Mode**: Uses `MockTenantRegistryService.cs` with hardcoded "threedegrees" tenant
- **LocalStack Mode**: Uses real DynamoDB with `TenantRegistryService.cs`
- **Production Mode**: Uses real AWS DynamoDB

## Multi-Tenancy Implementation

### Tenant Resolution
- Tenant context established via `TenancyOptions.cs`
- URL-based tenant resolution: `/api/balance/{tenantId}`
- Middleware handles tenant resolution and validation

### Service Integration
```csharp
// Example: Wise balance retrieval
public async Task<IEnumerable<Balance>> GetBalancesAsync(string tenantId)
{
    // 1. Lookup tenant in registry
    var tenantConfig = await _tenantRegistry.GetTenantConfigurationAsync(tenantId);
    
    // 2. Get provider credentials from secrets
    var secretArn = tenantConfig.WiseSecretsArn;
    var apiKey = await _secretsManager.GetSecretValueAsync(secretArn);
    
    // 3. Call provider API
    return await _wiseClient.GetBalancesAsync(apiKey, tenantConfig.WiseProfileId);
}
```

### Health Checks
- Health checks are tenant-aware via `HealthService.cs`
- Verify tenant registry connectivity
- Validate provider credential access

## Metrics and Monitoring
- CloudWatch metrics are tenant-segmented
- Custom metrics via `CloudWatchMetricsService.cs`
- Tenant-specific dashboards and alarms
- Request correlation with tenant context

## Testing and Validation

### Tenant Lookup Testing
```bash
# Test tenant exists in registry
aws --endpoint-url=http://localhost:4566 dynamodb get-item \
  --table-name "centro-dev-tenant-registry" \
  --key '{"TenantId":{"S":"threedegrees"}}' \
  --region eu-west-1

# Test API endpoint
curl http://localhost:5139/api/balance/threedegrees
```

### Error Scenarios
- **Tenant Not Found**: Registry lookup fails → 404 response
- **Invalid Configuration**: Missing provider config → 500 error
- **Provider Unavailable**: API call fails → 503 error

## Deployment Considerations
- ECS task definitions may be tenant-specific
- See `src/Centro.Infrastructure/Ecs/task-definition.json`
- Environment variables passed to containers include tenant context
- Tenant data isolation and compliance requirements

## Configuration Management

### Environment-Specific Settings
```json
// appsettings.Development.json (LocalStack)
{
  "Tenancy": {
    "DefaultTenantId": "threedegrees",
    "RegistryTableName": "centro-dev-tenant-registry"
  },
  "Aws": {
    "UseLocalStack": true,
    "ServiceURL": "http://localhost:4566"
  }
}

// appsettings.Production.json
{
  "Tenancy": {
    "RegistryTableName": "centro-prod-tenant-registry"
  },
  "Aws": {
    "UseLocalStack": false
  }
}
```

### Feature Flags
Tenants can have different feature configurations:
- Balance monitoring
- Automated reconciliation  
- Transaction reporting
- Webhook notifications
- Compliance requirements

## Related Files
- `src/Centro.Api/Configuration/TenancyOptions.cs`
- `src/Centro.Api/Configuration/DatabaseOptions.cs`
- `src/Centro.Infrastructure/Metrics/CloudWatchMetricsService.cs`
- `src/Centro.Infrastructure/Health/HealthService.cs`
- `src/Centro.Infrastructure/Providers/Wise/MockTenantRegistryService.cs`
- `scripts/provision-tenant-localstack.sh`

## Related Documentation
- [LocalStack Setup](../development/localstack-setup.md)
- [Tenant Provisioning](../development/tenant-provisioning.md)
- [Secrets Management](./secrets-management.md)
- [DynamoDB Service](./dynamodb-service.md)