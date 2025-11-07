# Environment Configuration Guide

## Environment Hierarchy

Centro uses a clear four-tier environment system designed for different stages of development and deployment:

### 1. **Local** (`ASPNETCORE_ENVIRONMENT=Local`)
- **Purpose**: Local laptop development with LocalStack for AWS service mocking
- **Configuration**: `appsettings.local.json`
- **Database**: Local PostgreSQL container (`localhost:5432`)
- **AWS Services**: LocalStack (`localhost:4566`) - S3, DynamoDB, SQS, Secrets Manager
- **Provider APIs**: Mock mode enabled for faster development iteration
- **Table Names**: `centro-local-*` prefix to avoid conflicts
- **CORS**: Allows `localhost:3000` origins for local frontend development
- **HTTPS**: Disabled for simplified local setup
- **Secrets**: Local configuration files (not AWS Secrets Manager)
- **Use Case**: Day-to-day feature development, debugging, unit testing

### 2. **Dev** (`ASPNETCORE_ENVIRONMENT=Dev`)
- **Purpose**: AWS development environment for integration testing
- **Configuration**: `appsettings.dev.json`
- **Database**: AWS RDS PostgreSQL (connection via environment variables)
- **AWS Services**: Real AWS services in `eu-west-1` region
- **Provider APIs**: Sandbox APIs (Wise sandbox, Nium test environment)
- **Table Names**: `centro-dev-*` prefix
- **Domain**: `api.dev.centro.3degrees.xyz`
- **Secrets**: AWS Secrets Manager with dev-prefixed secrets
- **Use Case**: Integration testing, API contract validation, provider testing

### 3. **Staging** (`ASPNETCORE_ENVIRONMENT=Staging`)
- **Purpose**: Pre-production testing with production-like configuration
- **Configuration**: `appsettings.staging.json`
- **AWS Services**: Real AWS services (separate staging account recommended)
- **Provider APIs**: Production APIs with test credentials where available
- **Table Names**: `centro-staging-*` prefix
- **Secrets**: AWS Secrets Manager with staging-prefixed secrets
- **Use Case**: End-to-end testing, performance testing, security validation

### 4. **Production** (`ASPNETCORE_ENVIRONMENT=Production`)
- **Purpose**: Live production system handling real transactions
- **Configuration**: `appsettings.production.json`
- **AWS Services**: Production AWS services with full monitoring
- **Provider APIs**: Live production APIs with real client credentials
- **Table Names**: `centro-prod-*` prefix
- **Secrets**: AWS Secrets Manager with production secrets
- **Monitoring**: Full CloudWatch metrics, alerting, and logging
- **Use Case**: Production traffic, real money transfers

## Deprecated Environments

### ~~Development~~ (DEPRECATED)
- **Status**: ⚠️ **Being phased out** - do not use for new development
- **Issue**: Contains localhost references unsuitable for AWS deployment
- **Migration**: Use `Local` for local development, `Dev` for AWS development

## Environment Capabilities

Each environment has different capabilities enabled through the `EnvironmentCapabilities` system:

| Capability | Local | Dev | Staging | Production |
|------------|-------|-----|---------|------------|
| **Mock Providers** | ✅ Yes | ❌ No | ❌ No | ❌ No |
| **LocalStack** | ✅ Yes | ❌ No | ❌ No | ❌ No |
| **Real AWS** | ❌ No | ✅ Yes | ✅ Yes | ✅ Yes |
| **Sandbox APIs** | N/A | ✅ Yes | ✅ Partial | ❌ No |
| **Production APIs** | ❌ No | ❌ No | ✅ Limited | ✅ Yes |
| **Debug Logging** | ✅ Yes | ✅ Yes | ⚠️ Limited | ❌ No |

## Key Configuration Differences

| Environment | AWS Services | LocalStack | Database | Provider APIs | CORS Origins | Secrets |
|-------------|--------------|------------|----------|---------------|--------------|---------|
| **Local** | LocalStack | ✅ Yes | Local PostgreSQL | Mock/Disabled | `localhost:*` | Local files |
| **Dev** | Real AWS | ❌ No | AWS RDS | Sandbox APIs | `*.dev.3degrees.io` | AWS Secrets Manager |
| **Staging** | Real AWS | ❌ No | AWS RDS | Mixed APIs | `*.staging.3degrees.io` | AWS Secrets Manager |
| **Production** | Real AWS | ❌ No | AWS RDS | Production APIs | `*.3degrees.io` | AWS Secrets Manager |

## Migration Guide

### For Local Development
1. **Before**: Set `ASPNETCORE_ENVIRONMENT=Development`
2. **After**: Set `ASPNETCORE_ENVIRONMENT=Local`
3. **Configuration**: Now uses `appsettings.local.json` with proper LocalStack setup
4. **Benefits**: Faster startup, no AWS credentials needed, isolated testing

### For AWS Deployment
1. **Before**: Used `Development` environment with localhost references (broken)
2. **After**: Use `Dev`, `Staging`, or `Production` environments
3. **Configuration**: Environment-specific files with correct AWS service URLs
4. **ECS Task Definition**: Ensure `ASPNETCORE_ENVIRONMENT` matches target environment

## Development Workflow

### Starting Local Development
```bash
# 1. Start LocalStack and PostgreSQL
./scripts/start-localstack-podman.sh

# 2. Set environment (in IDE or terminal)
export ASPNETCORE_ENVIRONMENT=Local

# 3. Run the application
dotnet run --project src/Centro.Api
```

### Testing Against Dev Environment
```bash
# 1. Ensure AWS credentials are configured
aws configure list

# 2. Set environment
export ASPNETCORE_ENVIRONMENT=Dev

# 3. Run with dev configuration
dotnet run --project src/Centro.Api
```

## Environment Variables

### Local Environment
```bash
ASPNETCORE_ENVIRONMENT=Local
# No AWS credentials needed - uses LocalStack
```

### Dev/Staging/Production
```bash
ASPNETCORE_ENVIRONMENT=Dev|Staging|Production
AWS_REGION=eu-west-1
# AWS credentials via IAM roles or environment variables
```

## Troubleshooting

### Common Issues

**Issue**: "LocalStack connection refused"
- **Solution**: Ensure LocalStack is running via `./scripts/start-localstack-podman.sh`
- **Check**: `curl http://localhost:4566/health`

**Issue**: "Database connection failed in Local environment"
- **Solution**: Check PostgreSQL container is running in docker-compose
- **Check**: `docker ps | grep postgres`

**Issue**: "AWS services not found in Dev environment"  
- **Solution**: Verify AWS credentials and region configuration
- **Check**: `aws sts get-caller-identity`

**Issue**: "Wrong DynamoDB table names"
- **Solution**: Each environment uses prefixed table names (`centro-{env}-*`)
- **Check**: Table name configuration in appsettings files

### Configuration File Validation
```bash
# Validate JSON syntax
cat src/Centro.Api/appsettings.local.json | jq .

# Check environment-specific settings
dotnet run --project src/Centro.Api --configuration Debug -- --help
```

## Launch Settings Configuration

The `launchSettings.json` now defaults to `Local` environment for seamless local development:

```json
{
  "profiles": {
    "Centro.Api": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Local"
      },
      "applicationUrl": "http://localhost:5000",
      "launchBrowser": false
    }
  }
}
```

## Security Considerations

### Local Environment
- **No production secrets** - uses mock credentials and LocalStack
- **Open CORS policy** - allows any localhost origin for development
- **Debug logging enabled** - includes sensitive data for troubleshooting

### Dev Environment  
- **Sandbox credentials only** - no access to production provider APIs
- **Restricted CORS** - only dev domain origins allowed
- **AWS IAM roles** - minimal permissions for development resources

### Staging/Production
- **Production secrets** - real provider API credentials via AWS Secrets Manager
- **Strict CORS policy** - only authorized domain origins
- **Audit logging** - all API calls logged for compliance
- **IAM least privilege** - minimal required permissions only

## File Naming Convention

Environment configuration files follow lowercase naming:
- `appsettings.local.json` (not `Local`)
- `appsettings.dev.json` (not `Dev`) 
- `appsettings.staging.json`
- `appsettings.production.json`

This ensures consistency across platforms and deployment systems.