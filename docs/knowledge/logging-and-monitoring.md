# Logging and Monitoring

## Overview
Centro implements structured logging with CloudWatch integration and comprehensive request/response monitoring.

## Logging Architecture

### Structured Logging
- Custom `StructuredLoggingProvider.cs` for consistent log formatting
- `StructuredLoggingEnricher.cs` adds correlation IDs and context
- Sensitive data masking via `SensitiveDataMasker.cs`

### Log Context
- `LogContext.cs` manages request-scoped logging context
- Correlation IDs tracked across requests via `CorrelationIdMiddleware.cs`
- Request/response logging via `RequestResponseLoggingMiddleware.cs`

## CloudWatch Integration
- Logs sent to CloudWatch Logs
- Metrics sent to CloudWatch Metrics via `CloudWatchMetricsService.cs`
- Custom metrics for business logic and performance monitoring

## Middleware Stack
1. `SecurityHeadersMiddleware.cs` - Security headers
2. `CorrelationIdMiddleware.cs` - Request correlation
3. `RequestMetricsMiddleware.cs` - Performance metrics
4. `RequestResponseLoggingMiddleware.cs` - Request/response logging
5. `ErrorHandlingMiddleware.cs` - Global error handling

## Configuration
- Logging configured via `LoggingOptions.cs`
- Environment-specific settings in `appsettings.{Environment}.json`
- CloudWatch settings in configuration

## Best Practices
- Always use structured logging with context
- Mask sensitive data before logging
- Include correlation IDs in all log entries
- Monitor both technical and business metrics

## Related Files
- `src/Centro.Api/Logging/` - All logging components
- `src/Centro.Api/Middleware/` - Request processing middleware
- `src/Centro.Infrastructure/Metrics/CloudWatchMetricsService.cs`