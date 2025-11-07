# SecretsManagerService

## Overview
The `SecretsManagerService` provides a clean abstraction over AWS Secrets Manager, designed to retrieve API keys and other sensitive configuration data. It intelligently handles both JSON-formatted secrets and plain text secrets, automatically extracting API keys from common JSON patterns.

## Architecture

### Purpose
- Abstracts AWS Secrets Manager complexity
- Provides mockable interface for testing
- Handles both JSON and plain text secrets
- Extracts API keys from common JSON patterns
- Supports tenant-specific secret retrieval

### Location
- **Interface**: `Centro.Infrastructure.Services.ISecretsManagerService`
- **Implementation**: `Centro.Infrastructure.Services.SecretsManagerService`
- **Registration**: `Centro.Infrastructure.Extensions.WiseServiceExtensions`

## Core Functionality

### Primary Method: `GetSecretAsync`
```csharp
Task<string> GetSecretAsync(string secretArn, CancellationToken cancellationToken = default)
```

**Parameters:**
- `secretArn`: Full ARN or name of the secret in AWS Secrets Manager

**Returns:**
- String value of the secret (API key, token, etc.)
- Throws exception if secret not found or inaccessible

### Smart Secret Extraction

The service automatically handles different secret formats:

#### JSON Secrets (Preferred)
```json
{
  "api_key": "wise_live_abc123...",
  "environment": "production",
  "created_date": "2024-01-15"
}
```
**Result**: Returns `"wise_live_abc123..."` (extracts the `api_key` value)

#### Plain Text Secrets
```
wise_live_abc123...
```
**Result**: Returns the entire string as-is

#### Supported JSON Key Patterns
The service looks for API keys in this order:
1. `api_key`
2. `apiKey`
3. `token`
4. `access_token`
5. `key`

If none found, returns the entire JSON string.

## Usage Examples

### Basic Usage in WiseApiClient
```csharp
public class WiseApiClient : IWiseApiClient
{
    private readonly ISecretsManagerService _secretsManager;
    
    public async Task<WiseBalanceResponse[]> GetBalancesAsync(string secretsArn, string profileId, CancellationToken cancellationToken = default)
    {
        // Get API token from Secrets Manager
        var apiToken = await _secretsManager.GetSecretAsync(secretsArn, cancellationToken);
        
        // Use token in API request
        using var request = new HttpRequestMessage(HttpMethod.Get, $"v4/profiles/{profileId}/balances");
        request.Headers.Add("Authorization", $"Bearer {apiToken}");
        
        // Make API call...
    }
}
```

### Testing with Mocks
```csharp
[Test]
public async Task GetBalancesAsync_ValidSecret_ReturnsBalances()
{
    // Arrange
    var mockToken = "mock-api-token";
    _mockSecretsManager.Setup(x => x.GetSecretAsync("mock-arn", It.IsAny<CancellationToken>()))
        .ReturnsAsync(mockToken);
    
    // Act & Assert
    var result = await _wiseClient.GetBalancesAsync("mock-arn", "profile-id");
    Assert.NotNull(result);
}
```

### Real-World Secret ARN Patterns
```csharp
// Tenant-specific secrets
var secretArn = "arn:aws:secretsmanager:eu-west-1:123456789:secret:centro/prod/threedegrees/wise-api-key";

// Environment-specific secrets  
var secretArn = "arn:aws:secretsmanager:eu-west-1:123456789:secret:centro/dev/wise-api-key";

// Simple secret names (resolved to full ARN by AWS)
var secretName = "centro-prod-wise-api-key";
```

## Error Handling

### Exception Types
- **`ResourceNotFoundException`**: Secret doesn't exist
- **`DecryptionFailureException`**: KMS decryption failed
- **`InvalidRequestException`**: Malformed secret ARN
- **`AccessDeniedException`**: Insufficient IAM permissions
- **`InvalidOperationException`**: Secret contains no string value

### Logging
- **Debug**: Secret retrieval requests (ARN logged, value never logged)
- **Error**: Exceptions with ARN context (never logs secret values)
- **Security**: Never logs actual secret values in any log level

## Security Considerations

### Secret Value Protection
- **Never logged**: Secret values are never written to logs
- **Memory safety**: Secrets stored as strings (consider `SecureString` for future)
- **Scope limitation**: Secrets only exist in method scope

### IAM Permissions
The service requires specific IAM permissions:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "secretsmanager:GetSecretValue"
      ],
      "Resource": [
        "arn:aws:secretsmanager:*:*:secret:centro/*",
        "arn:aws:secretsmanager:*:*:secret:wise/*"
      ]
    }
  ]
}
```

### KMS Integration
- Secrets Manager automatically handles KMS decryption
- Requires access to the KMS key used for secret encryption
- Default AWS managed key: `aws/secretsmanager`

## Current Limitations

### 1. Read-Only Operations
- Only supports `GetSecretValue`
- No support for creating, updating, or deleting secrets
- No support for secret rotation

### 2. Simple Key Extraction
- Limited to predefined key patterns
- No custom key path specification
- No support for nested JSON structures

### 3. No Caching
- Fetches secret on every call
- No built-in caching mechanism
- Relies on AWS SDK caching (if any)

### 4. Single Secret Per Call
- No batch secret retrieval
- No support for secret bundles
- Each secret requires separate API call

## Future Enhancements

### Phase 1: Caching
```csharp
// Add memory caching with configurable TTL
public class CachedSecretsManagerService : ISecretsManagerService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);
}
```

### Phase 2: Custom Key Extraction
```csharp
Task<string> GetSecretAsync(string secretArn, string? jsonPath = null, CancellationToken cancellationToken = default);

// Usage:
var apiKey = await _secretsManager.GetSecretAsync("arn:...", "$.credentials.api_key");
```

### Phase 3: Batch Operations
```csharp
Task<Dictionary<string, string>> GetSecretsAsync(IEnumerable<string> secretArns, CancellationToken cancellationToken = default);
```

### Phase 4: Write Operations
```csharp
Task CreateSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default);
Task UpdateSecretAsync(string secretArn, string secretValue, CancellationToken cancellationToken = default);
```

### Phase 5: Advanced Features
- Secret rotation support
- Version management
- Binary secret support
- Cross-region replication
- Automatic secret refresh

## Configuration

### Service Registration
```csharp
// In Program.cs or Startup.cs
services.AddWiseServices(configuration);

// Or manually:
services.AddAWSService<IAmazonSecretsManager>();
services.AddScoped<ISecretsManagerService, SecretsManagerService>();
```

### AWS Configuration
Requires standard AWS configuration:
- AWS credentials (IAM role, profile, or environment variables)
- Region configuration
- KMS permissions for secret decryption

## Testing Strategy

### Unit Tests
- Mock `ISecretsManagerService` interface
- Test business logic without AWS dependencies
- Use mock secret values like `"mock-api-token"`

### Integration Tests
- Use real Secrets Manager with test secrets
- Test actual AWS integration and KMS decryption
- Verify JSON parsing and key extraction
- Test error scenarios (missing secrets, permission errors)

### Test Secret Setup
```json
{
  "SecretName": "centro-test-wise-api-key",
  "SecretValue": {
    "api_key": "wise_test_abc123...",
    "environment": "test",
    "created_by": "automation"
  }
}
```

## Secret Naming Conventions

### Recommended Patterns
```
# Tenant-specific secrets
centro/{environment}/{tenant_id}/{provider}-api-key

# Application-wide secrets  
centro/{environment}/{provider}-api-key

# Examples
centro/dev/threedegrees/wise-api-key
centro/prod/wise-api-key
centro/staging/currencycloud-api-key
```

### Benefits of Consistent Naming
- Easy IAM permission management
- Clear ownership and purpose
- Environment isolation
- Automated secret rotation

## Related Files
- `src/Centro.Infrastructure/Services/SecretsManagerService.cs` - Implementation
- `src/Centro.Infrastructure/Providers/Wise/WiseApiClient.cs` - Primary consumer
- `tests/Centro.Providers.Tests/Wise/WiseBalanceAdapterTests.cs` - Usage examples