# Centro Development Setup

## Prerequisites

- **.NET 8 SDK** - For running the Centro API
- **Podman** - Container runtime (preferred over Docker)
- **AWS CLI** - For LocalStack interaction
- **jq** - JSON processing for scripts
- **PostgreSQL client** - For database testing

For detailed Podman setup, see [Podman Setup Guide](docs/development/podman-setup.md).

## 🎯 Quick Start

**IMPORTANT**: Use the proper git branching strategy! Work on your personal `local-{name}` branch, not on `main`.

```bash
# First time setup - create your personal branch
git checkout -b local-sam  # Replace 'sam' with your name
git push -u origin local-sam
```

The Centro API is ready to run with mock services - no external dependencies required!

```bash
# Start the API
cd src/Centro.Api
dotnet run

# Test balance endpoint
curl http://localhost:5139/api/balance/threedegrees
```

**Expected Response:**
```json
[
  {
    "currency": "USD",
    "amount": 1500.75,
    "reservedAmount": 100.00,
    "lastUpdated": "2025-07-05T17:15:04.6798109Z"
  },
  {
    "currency": "EUR",
    "amount": 2300.50,
    "reservedAmount": 50.00,
    "lastUpdated": "2025-07-05T17:15:04.679838Z"
  }
]
```

## 🏗️ Architecture Overview

### **Three-Tier Configuration System**

1. **Mock Mode** (Default) - No external dependencies
   - `MockWiseApiClient` - Returns test balance data
   - `MockTenantRegistryService` - Handles test tenant "threedegrees"
   - `MockSecretsManagerService` - Returns test API tokens
   - No database required

2. **LocalStack Mode** - Complete local AWS + PostgreSQL
   - PostgreSQL container (replaces RDS)
   - LocalStack with 16 AWS services
   - Real tenant provisioning and secrets management
   - Mock Wise API calls but real AWS service simulation
   - Set `AWS:UseLocalStack: true` in appsettings
   - See [LocalStack Setup Guide](docs/development/localstack-setup.md)

3. **Production Mode** - Real AWS services
   - Real RDS PostgreSQL
   - Real AWS services (DynamoDB, Secrets Manager, etc.)
   - Real Wise API
   - Set `AWS:UseLocalStack: false` in appsettings

### **API Endpoints**

- `GET /api/balance/{tenantId}` - Get balances for a tenant
- `GET /health` - Basic health check
- `GET /health/ready` - Readiness check with dependencies
- `GET /health/live` - Liveness check

## 🐳 Container Options

### **Option 1: Podman (Recommended) - Automated Setup**

Use the provided scripts for easy setup:

```bash
# Start complete environment (PostgreSQL + LocalStack)
./scripts/start-localstack-podman.sh

# Provision test tenant with real Wise API credentials
./scripts/provision-tenant-localstack.sh threedegrees-tenant-config.json create

# Run Bruno API test suite
./scripts/run-bruno-tests.sh

# Stop environment when done
./scripts/stop-localstack-podman.sh
```

### **Option 2: Manual Podman Setup**

If you prefer manual control over containers:

```bash
# Create network
podman network create centro-network

# Start PostgreSQL (RDS replacement)
podman run -d --name centro-postgres \
  --network centro-network \
  -p 5432:5432 \
  -e POSTGRES_DB=centro_dev \
  -e POSTGRES_USER=centro_dev \
  -e POSTGRES_PASSWORD=devpassword \
  -v ./postgres/init:/docker-entrypoint-initdb.d:Z \
  docker.io/postgres:15

# Start LocalStack (AWS services)
podman run -d --name centro-localstack \
  --network centro-network \
  -p 4566:4566 \
  -e SERVICES=dynamodb,secretsmanager,s3,sqs,sns,iam,kms,sts,cloudwatch,logs,ecr,ecs,lambda,apigateway,route53,elbv2 \
  -e DEBUG=1 \
  -v ./localstack/init:/etc/localstack/init/ready.d:Z \
  docker.io/localstack/localstack:latest

# Verify setup
curl http://localhost:4566/_localstack/health
```

For detailed setup and troubleshooting, see:
- [LocalStack Setup Guide](docs/development/localstack-setup.md)
- [Podman Setup Guide](docs/development/podman-setup.md)
- [Troubleshooting Guide](docs/development/localstack-troubleshooting.md)

### **Option 2: Docker Compose - Complete Stack**

If you prefer Docker Compose (requires docker-compose or podman-compose):

```bash
# Start PostgreSQL + LocalStack
docker-compose up -d

# Stop everything
docker-compose down
```

**Note**: Podman users may need to install `podman-compose` or use individual container commands.

### **Option 3: Individual Services**

```bash
# Just PostgreSQL
podman run -d --name centro-postgres \
  -p 5432:5432 \
  -e POSTGRES_DB=centro_dev \
  -e POSTGRES_USER=centro_dev \
  -e POSTGRES_PASSWORD=devpassword \
  postgres:15

# Just LocalStack
podman run -d --name centro-localstack \
  -p 4566:4566 \
  -e SERVICES=dynamodb,secretsmanager \
  localstack/localstack:latest
```

## 🏢 Tenant Management

### **Adding Tenants to LocalStack**

Use the tenant provisioning script to add tenants to your local environment:

```bash
# Provision a tenant
./scripts/provision-tenant-localstack.sh ~/Workspace/tenancy/tenant_config_3_degrees_fixed.json create

# Verify tenant setup
aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets --region eu-west-1
aws --endpoint-url=http://localhost:4566 dynamodb scan --table-name centro-dev-tenant-registry --region eu-west-1

# Test tenant endpoint
curl http://localhost:5139/api/balance/threedegrees
```

For detailed tenant management, see [Tenant Provisioning Guide](docs/development/tenant-provisioning.md).

## 🧪 Testing

### **Unit Tests**
```bash
# Run all tests
dotnet test

# Run specific project tests
cd tests/Centro.Providers.Tests
dotnet test
```

### **Bruno API Tests** 
Centro includes a comprehensive Bruno test suite for API testing:

```bash
# Run all Centro API tests (requires Local environment)
cd bruno
bru run "Centro API" -r --env Local

# Or use the convenience script
./scripts/run-bruno-tests.sh
```

**Bruno Test Coverage:**
- ✅ Get Balance - ThreeDegrees (200 OK) - Real Wise data
- ✅ Health Check (200 OK) - API health monitoring  
- ✅ Get Balance - Test Company (200 OK) - Uses threedegrees tenant
- ✅ Get Balance - Invalid Tenant (404 Not Found) - Error handling
- ✅ LocalStack Health Check (200 OK) - LocalStack connectivity
- ✅ Wise Direct API calls (200 OK) - Direct Wise sandbox integration

**Prerequisites for Bruno tests:**
- Centro API running on localhost:5139
- LocalStack running (for real data tests)
- Bruno CLI: `npm install -g @usebruno/cli`

### **Manual API Testing**
```bash
# Valid tenant
curl http://localhost:5139/api/balance/threedegrees

# Invalid tenant (error handling)
curl http://localhost:5139/api/balance/invalid-tenant

# Health checks
curl http://localhost:5139/health
curl http://localhost:5139/health/ready
curl http://localhost:5139/health/live
```

## 📁 Key Files

### **Core Implementation**
- `src/Centro.Core/Balance/` - Domain entities and interfaces
- `src/Centro.Infrastructure/Providers/Wise/` - Wise integration
- `src/Centro.Api/Controllers/BalanceController.cs` - REST API

### **Mock Services**
- `src/Centro.Infrastructure/Providers/Wise/MockWiseApiClient.cs`
- `src/Centro.Infrastructure/Providers/Wise/MockTenantRegistryService.cs`
- `src/Centro.Infrastructure/Services/MockSecretsManagerService.cs`

### **Configuration**
- `src/Centro.Api/appsettings.Development.json` - Development settings
- `src/Centro.Infrastructure/Extensions/WiseServiceExtensions.cs` - Service registration

### **LocalStack Setup**
- `docker-compose.yml` - Container orchestration
- `localstack/init/` - Initialization scripts for AWS services
- `scripts/provision-tenant-localstack.sh` - Tenant provisioning automation
- `docs/development/` - Comprehensive setup guides

## 🔧 Configuration

### **Development (Mock Mode)**
```json
{
  "AWS": {
    "UseLocalStack": true,
    "LocalStackEndpoint": "http://localhost:4566",
    "Region": "eu-west-1"
  }
}
```

### **Production**
```json
{
  "AWS": {
    "UseLocalStack": false,
    "Region": "eu-west-1"
  }
}
```

## 🚀 Next Steps

1. **Add More Providers**: Extend to CurrencyCloud, Nium, etc.
2. **Real AWS Integration**: Configure production AWS credentials
3. **Enhanced Testing**: Add integration tests with LocalStack
4. **Monitoring**: Add metrics and logging enhancements

## 🛠️ Troubleshooting

### **Common Issues**

#### **API Won't Start**
```bash
cd src/Centro.Api
dotnet build
# Check for compilation errors
```

#### **Container Issues**
```bash
# Check container status
podman ps -a
podman logs centro-localstack

# Restart containers
podman restart centro-localstack centro-postgres
```

#### **LocalStack Connection Issues**
```bash
# Test LocalStack health
curl http://localhost:4566/_localstack/health

# Check AWS CLI configuration
aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets --region eu-west-1
```

#### **Tenant Not Found**
```bash
# Verify tenant is provisioned
aws --endpoint-url=http://localhost:4566 dynamodb get-item \
  --table-name "centro-dev-tenant-registry" \
  --key '{"TenantId":{"S":"threedegrees"}}' \
  --region eu-west-1

# Re-provision if needed
./scripts/provision-tenant-localstack.sh tenant_config.json create
```

#### **Port Conflicts**
- API uses port 5000 (configurable)
- LocalStack uses port 4566
- PostgreSQL uses port 5432
- Change ports in configuration if needed

#### **Podman-specific Issues**
- Use full registry paths: `docker.io/postgres:15`
- Add `:Z` flag for SELinux: `-v ./data:/data:Z`
- See [Troubleshooting Guide](docs/development/localstack-troubleshooting.md)

### **Getting Help**

1. Check the comprehensive guides in `docs/development/`
2. Verify all prerequisites are installed
3. Test with mock mode first (always works)
4. Check container logs for specific errors

## ✅ What's Working

- ✅ Complete balance retrieval flow with real Wise sandbox API
- ✅ Multi-tenant architecture with real tenant provisioning
- ✅ Error handling and proper HTTP status codes (404 for missing tenants)
- ✅ Health checks with correlation ID tracking
- ✅ Mock services for offline development
- ✅ LocalStack with full AWS service emulation (eu-west-1 region)
- ✅ Podman containerization with automated setup scripts
- ✅ Automated tenant provisioning script with real credentials
- ✅ Comprehensive unit and integration tests (95+ tests passing)
- ✅ Bruno API test suite (8 tests covering all endpoints)
- ✅ API endpoints functional with structured logging
- ✅ Real-time metrics and request tracking middleware
- ✅ Comprehensive documentation with troubleshooting guides

## 📚 Documentation

- **[Git Branching Strategy](docs/development/git-branching-strategy.md)** - MUST READ: Environment-based workflow
- [LocalStack Setup Guide](docs/development/localstack-setup.md) - Complete LocalStack configuration
- [Tenant Provisioning Guide](docs/development/tenant-provisioning.md) - Adding and managing tenants
- [Podman Setup Guide](docs/development/podman-setup.md) - Container runtime configuration
- [Troubleshooting Guide](docs/development/localstack-troubleshooting.md) - Common issues and solutions
- [Secrets Management](docs/knowledge/secrets-management.md) - Provider credential handling
- [Tenant Configuration](docs/knowledge/tenant-configuration.md) - Multi-tenancy implementation