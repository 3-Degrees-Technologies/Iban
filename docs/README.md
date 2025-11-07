# Centro Documentation

## Overview
This documentation structure captures institutional knowledge for both human team members and AI assistants working on the Centro platform.

## Structure

### 📚 Knowledge
Captures insights and learnings about the platform:
- **[Secrets Management](knowledge/secrets-management.md)** - AWS Secrets Manager patterns and tenant secret locations
- **[Tenant Configuration](knowledge/tenant-configuration.md)** - Multi-tenant setup and configuration patterns
- **[Logging and Monitoring](knowledge/logging-and-monitoring.md)** - Structured logging, CloudWatch integration, and monitoring setup
- **[PricedRoute Domain Knowledge](knowledge/pricedroute-domain-knowledge.md)** - Deep dive into the PricedRoute value object, usage patterns, and provider integration examples

### 🔧 Runbooks
Step-by-step operational procedures:
- **[Deployment](runbooks/deployment.md)** - Deployment procedures and rollback steps

### 🏗️ Architecture
System design and architectural decisions:
- **[API Structure](architecture/api-structure.md)** - Layered architecture and key patterns

## Usage Guidelines

### For Human Team Members
- Browse knowledge docs to understand system patterns
- Follow runbooks for operational tasks
- Update docs when you discover new insights or patterns

### For AI Assistants
- Reference these docs for context about the Centro platform
- Use the knowledge captured here to make informed decisions
- Follow established patterns documented in architecture section

## Contributing
When you discover new knowledge or patterns:
1. Create or update relevant documentation
2. Link to specific code files and line numbers where applicable
3. Include examples and best practices
4. Update this README if adding new categories

## Integration with GitHub Wiki
This documentation is designed to work with a central GitHub Wiki that provides cross-project navigation and topic-based organization.