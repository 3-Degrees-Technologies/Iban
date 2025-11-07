# DynamoDbService

## Overview
The `DynamoDbService` provides a clean abstraction over AWS DynamoDB operations, specifically designed for simple key-value lookups. It converts DynamoDB's complex `AttributeValue` format to simple JSON strings, making it easy to work with tenant configuration data.

## Architecture

### Purpose
- Abstracts DynamoDB complexity for common operations
- Provides mockable interface for testing
- Handles tenant registry lookup pattern
- Converts DynamoDB types to standard JSON

### Location
- **Interface**: `Centro.Infrastructure.Services.IDynamoDbService`
- **Implementation**: `Centro.Infrastructure.Services.DynamoDbService`
- **Registration**: `Centro.Infrastructure.Extensions.WiseServiceExtensions`

## Core Functionality

### Primary Method: `GetItemAsync`
```csharp
Task<string?> GetItemAsync(string tableName, string keyName, string keyValue, CancellationToken cancellationToken = default)
```

**Parameters:**
- `tableName`: DynamoDB table name (e.g., `"centro-dev-tenant-registry"`)
- `keyName`: Primary key attribute name (e.g., `"tenant_id"`)
- `keyValue`: Primary key value (e.g., `"threedegrees"`)

**Returns:**
- JSON string representation of the entire DynamoDB item
- `null` if item not found
- Throws exception on errors

### Data Type Conversion

The service automatically converts DynamoDB's `AttributeValue` format to standard JSON:

**DynamoDB Format:**
```json
{
  "tenant_id": { "S": "threedegrees" },
  "wise_secrets_arn": { "S": "arn:aws:secretsmanager:..." },
  "wise_profile_id": { "S": "12345" },
  "active": { "BOOL": true },
  "balance_limit": { "N": "1000.50" }
}
```

**Converted JSON:**
```json
{
  "tenant_id": "threedegrees",
  "wise_secrets_arn": "arn:aws:secretsmanager:...",
  "wise_profile_id": "12345",
  "active": true,
  "balance_limit": 1000.50
}
```

### Supported DynamoDB Types
- **String** (`S`) → `string`
- **Number** (`N`) → `decimal`
- **Boolean** (`BOOL`) → `bool`
- **String Set** (`SS`) → `List<string>`
- **Number Set** (`NS`) → `List<decimal>`
- **List** (`L`) → `List<object>` (recursive conversion)
- **Map** (`M`) → `Dictionary<string, object>` (recursive conversion)
- **Null** (`NULL`) → `null`

## Usage Examples

### Basic Usage
```csharp
public class TenantRegistryService
{
    private readonly IDynamoDbService _dynamoDb;
    
    public async Task<TenantConfiguration> GetTenantConfigurationAsync(string tenantId)
    {
        // Get raw JSON from DynamoDB
        var tenantRecord = await _dynamoDb.GetItemAsync(
            "centro-dev-tenant-registry", 
            "tenant_id", 
            tenantId
        );
        
        if (tenantRecord == null)
            throw new ArgumentException($"Tenant {tenantId} not found");
            
        // Deserialize to strongly-typed object
        var config = JsonSerializer.Deserialize<TenantConfigurationRecord>(tenantRecord);
        return MapToTenantConfiguration(config);
    }
}
```

### Testing with Mocks
```csharp
[Test]
public async Task GetTenantConfiguration_ValidTenant_ReturnsConfig()
{
    // Arrange
    var mockJson = """{"tenant_id": "test", "wise_secrets_arn": "mock-arn"}""";
    _mockDynamoDb.Setup(x => x.GetItemAsync("table", "tenant_id", "test"))
        .ReturnsAsync(mockJson);
    
    // Act & Assert
    var result = await _tenantService.GetTenantConfigurationAsync("test");
    Assert.NotNull(result);
}
```

## Error Handling

### Exception Types
- **`ArgumentException`**: Invalid parameters
- **`AmazonDynamoDBException`**: AWS service errors
- **`JsonException`**: Data conversion errors
- **`InvalidOperationException`**: Unexpected data format

### Logging
- **Debug**: Request details and successful operations
- **Warning**: Item not found scenarios
- **Error**: Exceptions with full context

## Current Limitations

### 1. Read-Only Operations
- Only supports `GetItem` operations
- No support for `PutItem`, `UpdateItem`, or `DeleteItem`
- No batch operations

### 2. Simple Primary Keys Only
- Assumes single-attribute primary key
- No support for composite keys (partition key + sort key)
- No support for Global Secondary Index queries

### 3. No Advanced Querying
- No `Query` or `Scan` operations
- No filtering or projection expressions
- No pagination support

### 4. Limited Error Recovery
- No automatic retries
- No exponential backoff
- Relies on AWS SDK default retry policies

## Future Enhancements

### Phase 1: Write Operations
```csharp
Task PutItemAsync(string tableName, string jsonItem, CancellationToken cancellationToken = default);
Task UpdateItemAsync(string tableName, string keyName, string keyValue, Dictionary<string, object> updates, CancellationToken cancellationToken = default);
Task DeleteItemAsync(string tableName, string keyName, string keyValue, CancellationToken cancellationToken = default);
```

### Phase 2: Composite Keys
```csharp
Task<string?> GetItemAsync(string tableName, Dictionary<string, string> keys, CancellationToken cancellationToken = default);
```

### Phase 3: Query Operations
```csharp
Task<List<string>> QueryAsync(string tableName, string partitionKey, string partitionValue, CancellationToken cancellationToken = default);
Task<List<string>> ScanAsync(string tableName, Dictionary<string, object>? filters = null, CancellationToken cancellationToken = default);
```

### Phase 4: Advanced Features
- Batch operations (`BatchGetItem`, `BatchWriteItem`)
- Transaction support (`TransactGetItems`, `TransactWriteItems`)
- Conditional operations
- Projection expressions
- Pagination tokens

## Configuration

### Service Registration
```csharp
// In Program.cs or Startup.cs
services.AddWiseServices(configuration);

// Or manually:
services.AddAWSService<IAmazonDynamoDB>();
services.AddScoped<IDynamoDbService, DynamoDbService>();
```

### AWS Configuration
Requires standard AWS configuration:
- AWS credentials (IAM role, profile, or environment variables)
- Region configuration
- Appropriate DynamoDB permissions

### Required IAM Permissions
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "dynamodb:GetItem"
      ],
      "Resource": "arn:aws:dynamodb:*:*:table/centro-*-tenant-registry"
    }
  ]
}
```

## Testing Strategy

### Unit Tests
- Mock `IDynamoDbService` interface
- Test business logic without AWS dependencies
- Fast, reliable, environment-independent

### Integration Tests
- Use real DynamoDB with test data
- Test actual AWS integration
- Verify data type conversion accuracy
- Test error scenarios (network failures, missing items)

### Test Data Setup
- Use "threedegrees" as guaranteed test tenant
- Ensure test tenant exists in all environments
- Include various data types in test records

## Related Files
- `src/Centro.Infrastructure/Services/DynamoDbService.cs` - Implementation
- `src/Centro.Infrastructure/Providers/Wise/TenantRegistryService.cs` - Primary consumer
- `tests/Centro.Providers.Tests/Wise/WiseBalanceAdapterTests.cs` - Usage examples