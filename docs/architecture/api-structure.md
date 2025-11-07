# API Architecture

## Overview
Centro.Api follows a layered architecture with clear separation of concerns.

## Project Structure

### Centro.Api (Presentation Layer)
- **Controllers**: API endpoints and request handling
- **Middleware**: Cross-cutting concerns (logging, security, metrics)
- **Configuration**: Application settings and options
- **Models**: DTOs and response models

### Centro.Core (Domain Layer)
- **Interfaces**: Service contracts and abstractions
- **Domain Logic**: Business rules and entities

### Centro.Infrastructure (Infrastructure Layer)
- **External Services**: CloudWatch, AWS integrations
- **Data Access**: Database connections and repositories
- **Infrastructure Concerns**: Metrics, health checks

## Key Patterns

### Configuration Pattern
- Options classes for each configuration section
- Extension methods for service registration
- Environment-specific overrides

### Middleware Pipeline
```
Request → Security Headers → Correlation ID → Metrics → Logging → Error Handling → Controllers
```

### Health Checks
- Custom health check implementations
- Integration with ECS health checks
- Detailed health status reporting

### Error Handling
- Global error handling middleware
- Structured error responses
- Correlation ID tracking for debugging

## API Standards
- RESTful endpoints
- Consistent response models (`PaginatedResponse.cs`, `ErrorResponse.cs`)
- OpenAPI/Swagger documentation
- Correlation ID headers for request tracking

## Related Files
- `src/Centro.Api/Program.cs` - Application startup
- `src/Centro.Api/Extensions/ServiceCollectionExtensions.cs` - DI configuration
- `src/Centro.Api/Models/Common/` - Common response models