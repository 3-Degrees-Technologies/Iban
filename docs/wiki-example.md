# GitHub Wiki Structure Example

## Main Wiki Pages (in your GitHub Wiki)

### Home Page
```markdown
# Centro Platform Knowledge Base

Welcome to the Centro platform knowledge base. This wiki provides cross-project navigation and topic-based organization of our institutional knowledge.

## Quick Navigation
- [🔐 Security & Secrets](#security--secrets)
- [🏢 Tenant Management](#tenant-management)
- [📊 Monitoring & Logging](#monitoring--logging)
- [🚀 Deployment & Operations](#deployment--operations)
- [🏗️ Architecture](#architecture)

---

## Security & Secrets
- **[Secrets Management](https://github.com/your-org/Centro/blob/main/docs/knowledge/secrets-management.md)** - AWS Secrets Manager patterns (Centro)
- **[Infrastructure Secrets](https://github.com/your-org/Centro-Infrastructure/blob/main/docs/knowledge/secrets-setup.md)** - Secret provisioning and rotation (Infrastructure)

## Tenant Management
- **[Tenant Configuration](https://github.com/your-org/Centro/blob/main/docs/knowledge/tenant-configuration.md)** - Multi-tenant setup (Centro)
- **[Tenant Provisioning](https://github.com/your-org/Centro-TenantManager/blob/main/docs/runbooks/tenant-setup.md)** - New tenant onboarding (TenantManager)

## Monitoring & Logging
- **[Logging and Monitoring](https://github.com/your-org/Centro/blob/main/docs/knowledge/logging-and-monitoring.md)** - CloudWatch integration (Centro)
- **[Infrastructure Monitoring](https://github.com/your-org/Centro-Infrastructure/blob/main/docs/knowledge/monitoring-setup.md)** - Infrastructure metrics (Infrastructure)

## Deployment & Operations
- **[API Deployment](https://github.com/your-org/Centro/blob/main/docs/runbooks/deployment.md)** - Centro API deployment (Centro)
- **[Infrastructure Deployment](https://github.com/your-org/Centro-Infrastructure/blob/main/docs/runbooks/infrastructure-deployment.md)** - Infrastructure provisioning (Infrastructure)

## Architecture
- **[API Structure](https://github.com/your-org/Centro/blob/main/docs/architecture/api-structure.md)** - Centro API architecture (Centro)
- **[System Overview](https://github.com/your-org/Centro-Infrastructure/blob/main/docs/architecture/system-overview.md)** - Overall system design (Infrastructure)
```

### Topic-Specific Pages
Create dedicated wiki pages for major topics that aggregate knowledge from multiple repos:

#### Secrets-Management.md (Wiki Page)
```markdown
# Secrets Management

This page aggregates all knowledge about secrets management across the Centro platform.

## Overview
Secrets are managed using AWS Secrets Manager with specific patterns for different types of secrets.

## Application Secrets
- **[Centro API Secrets](https://github.com/your-org/Centro/blob/main/docs/knowledge/secrets-management.md)** - How the API consumes secrets
- **[Infrastructure Secrets Setup](https://github.com/your-org/Centro-Infrastructure/blob/main/docs/knowledge/secrets-provisioning.md)** - How secrets are provisioned

## Tenant Secrets
- **[Tenant Secret Patterns](https://github.com/your-org/Centro/blob/main/docs/knowledge/tenant-configuration.md#tenant-specific-secrets)** - Tenant-specific secret locations
- **[Tenant Secret Provisioning](https://github.com/your-org/Centro-TenantManager/blob/main/docs/runbooks/tenant-secrets.md)** - How tenant secrets are created

## Operations
- **[Secret Rotation](https://github.com/your-org/Centro-Infrastructure/blob/main/docs/runbooks/secret-rotation.md)** - How to rotate secrets
- **[Troubleshooting](https://github.com/your-org/Centro/blob/main/docs/runbooks/secret-troubleshooting.md)** - Common secret-related issues
```

## Benefits of This Approach

1. **AI Context**: Knowledge lives with code for AI assistants
2. **Human Navigation**: Wiki provides topic-based organization
3. **Version Control**: Documentation changes tracked with code
4. **Cross-Project Visibility**: Wiki shows relationships between projects
5. **Single Source of Truth**: Each piece of knowledge has one canonical location
6. **Easy Maintenance**: Update docs where the knowledge is most relevant

## Workflow
1. Write knowledge docs in the most relevant repo's `docs/` folder
2. Update wiki pages to link to new knowledge
3. Use GitHub's built-in linking to connect related topics
4. AI assistants can access files directly, humans navigate via wiki