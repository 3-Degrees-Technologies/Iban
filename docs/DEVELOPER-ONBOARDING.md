# Developer Onboarding Guide

Welcome to Centro! This guide will get you from zero to productive contributor in about 30 minutes.

## 🎯 What is Centro?

Centro is 3 Degrees Technologies' **cross-border payments platform** - we're the "network of networks" that **unbundles** existing FX aggregators.

**The Problem**: Traditional FX aggregators force you to accept bundled mediocrity - they might be great for USD→EUR but terrible for USD→CNY.

**Our Solution**: Cherry-pick the best-in-class provider for each specific corridor, giving clients access to optimal rates and reliability across all their payment routes.

**Key Insight**: Clients maintain direct relationships with FX providers (Wise, Nium, Currency Cloud) while Centro provides intelligent routing and provider abstraction.

## 🏗️ Architecture at a Glance

We use **Hexagonal Architecture** to keep business logic clean and providers swappable:

```
Centro.Api (REST endpoints, middleware)
    ↓
Centro.Core (routing logic, corridor optimization) 
    ↓
Centro.Infrastructure (Wise adapter, Nium adapter, etc.)
```

**Money never touches Centro** - it flows directly between client and provider. We're the intelligent routing layer.

## 🚀 Quick Start (5 minutes)

### Prerequisites
- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **That's it!** No containers, databases, or AWS accounts needed

### Get Centro Running
1. **Clone and run immediately**:
   ```bash
   git clone [repo-url]
   cd Centro
   cd src/Centro.Api
   dotnet run
   ```

2. **Test it works**:
   ```bash
   curl http://localhost:5000/api/balance/threedegrees
   ```

3. **You should see mock balance data** - congratulations, you're running Centro!

The API runs in **Mock Mode** by default - no external dependencies required.

## 📚 Essential Reading (15 minutes)

Read these in order to understand the system:

1. **[DEVELOPMENT-SETUP.md](../DEVELOPMENT-SETUP.md)** - Technical setup and three-tier architecture
2. **[Agent.md](../Agent.md)** - Business context, architecture philosophy, and team context
3. **[docs/architecture/api-structure.md](architecture/api-structure.md)** - Code structure and patterns

## 🔧 Development Environment Options

### Option 1: Mock Mode (Recommended for new developers)
- **What**: Uses mock services, no external dependencies
- **When**: Learning the codebase, developing core features  
- **Prerequisites**: Just .NET 8 SDK
- **Setup**: `dotnet run` (that's it!)

### Option 2: LocalStack + Postgres (Full local AWS)
- **What**: Local AWS services + database via containers
- **When**: Testing AWS integrations, database features
- **Prerequisites**: 
  - .NET 8 SDK
  - **Docker** OR **Podman** (container runtime)
  - LocalStack runs as a container (not a NuGet package)
- **Setup**: 
  ```bash
  ./scripts/start-localstack-podman.sh
  # OR
  docker-compose up -d
  ```

### Option 3: Production Mode
- **What**: Real AWS services and database
- **When**: Production deployments only
- **Prerequisites**: AWS account and credentials
- **Setup**: Configure real AWS credentials

## 🧭 Key Concepts

### Corridors
- **Definition**: Specific currency routes (USD→GBP, EUR→CNY)
- **Why Important**: Each corridor has different optimal providers
- **Example**: Wise might be best for USD→EUR, but Nium best for USD→SGD

### Providers
- **Definition**: FX services like Wise, Nium, Currency Cloud
- **Integration**: Each has its own adapter in `Centro.Infrastructure/Providers/`
- **Client Model**: Clients maintain direct relationships, provide API keys to Centro

### Routing Intelligence
- **Location**: `Centro.Core/` (this is our secret sauce)
- **Purpose**: Determines optimal provider per corridor based on rates, reliability, etc.
- **Future**: This is where corridor optimization algorithms will live

## 📁 Codebase Navigation

### Core Business Logic (`Centro.Core/`)
```
Balance/
├── Balance.cs              # Domain entity
├── IBalanceService.cs      # Core interface
Health/
├── IHealthService.cs       # Health check interface
Metrics/
├── IMetricsService.cs      # Metrics interface
```

### External Integrations (`Centro.Infrastructure/`)
```
Providers/Wise/
├── Models/                 # Wise API models  
├── WiseApiClient.cs        # Real Wise integration
├── MockWiseApiClient.cs    # Mock for development
├── WiseBalanceAdapter.cs   # Converts Wise format to Centro format
Services/
├── DynamoDbService.cs      # AWS DynamoDB integration
├── SecretsManagerService.cs # AWS Secrets Manager
```

### API Layer (`Centro.Api/`)
```
Controllers/
├── BalanceController.cs    # REST endpoints
Middleware/
├── ErrorHandlingMiddleware.cs
├── CorrelationIdMiddleware.cs
├── RequestMetricsMiddleware.cs
```

## 🧪 Testing Strategy

We follow **TDD Pragmatist** approach - tests should add real value, not just coverage metrics.

### Test Priority (High to Low)
1. **Core business logic** - routing algorithms, corridor optimization
2. **Error handling** - payment systems must be robust
3. **Integration points** - provider APIs, external services
4. **Security-sensitive code** - anything touching credentials or financial data

### Running Tests
```bash
# All tests
dotnet test

# Specific project
cd tests/Centro.Core.Tests
dotnet test

# With coverage (when it matters)
dotnet test --collect:"XPlat Code Coverage"
```

## 🔐 Security Mindset

This is **high-stakes fintech** - security is paramount:

- **Never log secrets** - use `SensitiveDataMasker.cs` patterns
- **Validate all inputs** - especially anything from provider APIs
- **Error handling** - graceful failures, no sensitive data in responses
- **Audit trails** - structured logging for all important operations

## 📋 Common Development Tasks

### Adding a New FX Provider

1. **Create adapter** in `Centro.Infrastructure/Providers/NewProvider/`
2. **Implement interfaces** from `Centro.Core/`
3. **Add mock version** for development
4. **Write tests** focusing on error scenarios
5. **Register services** in `Extensions/ServiceCollectionExtensions.cs`

### Adding API Endpoints

1. **Define domain interfaces** in `Centro.Core/`
2. **Implement business logic** (keep it in Core, not Controllers)
3. **Add controller** in `Centro.Api/Controllers/`
4. **Add integration tests** in `Centro.Api.Tests/Integration/`

### LocalStack Development

```bash
# Start LocalStack + Postgres
./scripts/start-localstack-podman.sh

# Configure for LocalStack mode
# Edit appsettings.Development.json:
{
  "AWS": {
    "UseLocalStack": true,
    "LocalStackEndpoint": "http://localhost:4566"
  }
}

# Stop everything
./scripts/stop-localstack-podman.sh
```

## 🚨 Common Gotchas

1. **Port conflicts**: API uses 5000, LocalStack uses 4566, Postgres uses 5432
2. **Mock vs Real mode**: Check `AWS:UseLocalStack` setting if things behave unexpectedly  
3. **Container networking**: Use container names (`centro-postgres`) not localhost when services talk to each other
4. **Secrets**: Never commit real API keys - use LocalStack or mocks for development

## 📖 Next Steps

After completing this onboarding:

1. **Take a ticket** from the backlog - start with something marked "good first issue"
2. **Read provider documentation** - understand Wise, Nium APIs we're integrating
3. **Explore the tests** - they show expected behavior and edge cases
4. **Join standups** - understand current sprint priorities
5. **Ask questions** - better to ask than assume!

## 💡 Pro Tips

- **Start with Mock Mode** - understand the flow before adding complexity
- **Use structured logging** - follow existing patterns in `Logging/` 
- **Check health endpoints** - `/health/ready` shows if dependencies are healthy
- **Read error logs** - structured logging makes debugging much easier
- **Follow existing patterns** - consistency matters more than personal preference

## 🆘 Getting Help

- **Slack**: #centro-dev channel
- **Code questions**: Pair with Sam or Esteban
- **Architecture questions**: Refer to `docs/architecture/`
- **LocalStack issues**: Check `LOCALSTACK-COMPATIBILITY.md`

Welcome to the team! 🎉

---

*Last updated: July 2025*