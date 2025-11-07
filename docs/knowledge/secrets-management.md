# Secrets Management

## Overview
This document captures knowledge about how secrets are managed across the Centro platform, including both production AWS and local development environments.

## AWS Secrets Manager (Production)

### Tenant-Specific Secrets
- **Location Pattern**: `/centro/{environment}/{tenant-id}/secrets`
- **Common Secret Types**:
  - Database connection strings
  - API keys for third-party integrations
  - Encryption keys

### Application Secrets
- **Location Pattern**: `/centro/{environment}/app/secrets`
- **Contains**:
  - JWT signing keys
  - CloudWatch credentials
  - ECS task execution role secrets

### Provider Credentials
- **Location Pattern**: `centro/{environment}/{tenant-id}/{provider}`
- **Examples**:
  - `centro/prod/threedegrees/wise` - Wise API credentials
  - `centro/staging/creditunion/nium` - Nium client credentials
- **Format**: JSON-encoded strings containing provider-specific credentials

## LocalStack Secrets Management (Development)

### Local Development Setup
For local development, secrets are managed through LocalStack which emulates AWS Secrets Manager:

- **LocalStack Endpoint**: `http://localhost:4566`
- **Environment**: `dev` (to avoid conflicts with production)
- **Secret Naming**: `centro/dev/{tenant-id}/{provider}`

### Example LocalStack Secrets
```bash
# Wise API credentials for threedegrees tenant
Secret Name: centro/dev/threedegrees/wise
Secret Value: "4870ef4b-c995-46eb-9044-bd39b3c1d417"
ARN: arn:aws:secretsmanager:eu-west-1:000000000000:secret:centro/dev/threedegrees/wise-abcdef

# Test company credentials
Secret Name: centro/dev/testcompany/wise  
Secret Value: "test-wise-api-key-12345"
```

### Accessing LocalStack Secrets
```bash
# List all secrets in LocalStack
aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets --region eu-west-1

# Get specific secret value
aws --endpoint-url=http://localhost:4566 secretsmanager get-secret-value \
  --secret-id "centro/dev/threedegrees/wise" \
  --region eu-west-1
```

### Tenant Provisioning
The automated tenant provisioning script creates secrets from tenant configuration files:

```bash
# Provision tenant secrets
./scripts/provision-tenant-localstack.sh tenant_config.json create

# Delete tenant secrets  
./scripts/provision-tenant-localstack.sh tenant_config.json delete
```

See [Tenant Provisioning Guide](../development/tenant-provisioning.md) for details.

## Configuration Integration

### In Centro.Api
- Secrets are loaded via `ConfigurationExtensions.cs`
- Environment-specific overrides in `appsettings.{Environment}.json`
- See `AuthOptions.cs` and `DatabaseOptions.cs` for secret binding patterns
- LocalStack integration via `AwsOptions.cs` configuration

### Development vs Production
```csharp
// Development (LocalStack)
services.Configure<AwsOptions>(options => {
    options.ServiceURL = "http://localhost:4566";
    options.UseLocalStack = true;
});

// Production
services.Configure<AwsOptions>(options => {
    options.UseLocalStack = false;
    // Uses real AWS endpoints
});
```

### Mock Services
For rapid development without external dependencies:
- `MockSecretsManagerService.cs` - Returns test credentials
- `MockTenantRegistryService.cs` - Provides test tenant configuration
- Enabled when LocalStack is unavailable or disabled

### Best Practices
1. **Never log secret values** (use `SensitiveDataMasker.cs`)
2. **Rotate secrets regularly** through infrastructure project
3. **Use least-privilege IAM roles** for secret access
4. **Separate environments** - Use `dev` prefix for LocalStack
5. **Test with real secrets** in LocalStack before production deployment
6. **Automate provisioning** - Use scripts for consistent tenant setup

## Service Implementation

### SecretsManagerService
- **Production**: `SecretsManagerService.cs` - Real AWS integration
- **Development**: `MockSecretsManagerService.cs` - Mock responses
- **LocalStack**: Uses real AWS SDK with LocalStack endpoint

### WiseApiClient Integration
```csharp
// Secret retrieval in WiseApiClient
var secretArn = await _tenantRegistry.GetWiseSecretsArnAsync(tenantId);
var apiKey = await _secretsManager.GetSecretValueAsync(secretArn);
```

### Error Handling
- Secret not found → Tenant configuration issue
- Access denied → IAM permissions issue  
- Network errors → Service availability issue
- Invalid format → Secret value corruption

## Related Files
- `src/Centro.Api/Configuration/AuthOptions.cs`
- `src/Centro.Api/Configuration/AwsOptions.cs`
- `src/Centro.Api/Logging/SensitiveDataMasker.cs`
- `src/Centro.Api/Extensions/ConfigurationExtensions.cs`
- `src/Centro.Infrastructure/Services/SecretsManagerService.cs`
- `src/Centro.Infrastructure/Services/MockSecretsManagerService.cs`
- `scripts/provision-tenant-localstack.sh`

## Related Documentation
- [LocalStack Setup](../development/localstack-setup.md)
- [Tenant Provisioning](../development/tenant-provisioning.md)
- [Tenant Configuration](./tenant-configuration.md)