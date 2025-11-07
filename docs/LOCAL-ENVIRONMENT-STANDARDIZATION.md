# Local Environment Standardization Plan

## Overview
The local environment currently uses inconsistent naming that mixes "dev" and "local" references, causing confusion. This document outlines the plan to make local a first-class environment with proper `centro-local-*` naming conventions.

## Naming Convention Rules

### 1. Environment Name Casing Standards
**All environments use lowercase to match AWS conventions:**
- **ASP.NET Core environments**: lowercase (`local`, `dev`, `staging`, `production`)
  - Used in: `ASPNETCORE_ENVIRONMENT`, file names like `appsettings.local.json`
- **AWS/Infrastructure resources**: lowercase-with-hyphens (`local`, `dev`, `staging`, `production`)  
  - Used in: Resource names like `centro-local-tenant-registry`
- **Configuration values**: lowercase (`local`, `dev`, `staging`, `production`)
  - Used in: Connection strings, environment-specific values

### 2. Resource Naming Pattern
All resources should follow: `{project}-{environment}-{feature}`
- Project: `centro`
- Environment: `local`, `dev`, `staging`, `production` (lowercase)
- Feature: descriptive name (lowercase-with-hyphens)

**Note:** This means renaming appsettings files and updating ASPNETCORE_ENVIRONMENT values to use lowercase.

## Current Issues Identified

### 1. Database Configuration
**Files:** `src/Centro.Api/appsettings.Local.json`, `docker-compose.yml`, `postgres/init/01-create-database.sql`, `scripts/start-localstack-podman.sh`

**Current Issues:**
- Database name: `centro_dev` → Should be `centro_local`
- Username: `centro_dev` → Should be `centro_local`
- Password: `devpassword` → Should be `localpassword`

**Files to Change:**
- `src/Centro.Api/appsettings.Local.json:3` - Connection string
- `docker-compose.yml:15,16,17,19` - Postgres environment variables
- `postgres/init/01-create-database.sql:3,5,6,7,8,9,10,11,12,13` - Database setup
- `scripts/start-localstack-podman.sh:25,26,27,31,37,39` - Container setup

### 2. AWS Resource Names (LocalStack)
**Files:** LocalStack initialization scripts, provisioning scripts, appsettings

**Current Issues:**
- DynamoDB tables: `centro-dev-*` → Should be `centro-local-*`
- S3 buckets: `centro-dev-*` → Should be `centro-local-*`
- SQS queues: `centro-dev-*` → Should be `centro-local-*`
- Secrets Manager: `centro/dev/*` → Should be `centro/local/*`

**Files to Change:**
- `localstack/init/01-setup-dynamodb.sh:6,8,10` - DynamoDB table names
- `localstack/init/03-setup-s3.sh:4,5,6` - S3 bucket names
- `localstack/init/04-setup-sqs.sh:6,10,14,18,22,26` - SQS queue names
- `localstack/init/05-setup-event-tables.sh:6,14,22` - Event table names
- `scripts/provision-tenant-localstack.sh:25,33,55,75,80,94` - Resource references
- `bruno/LocalStack/Get Tenant Registry.bru:8` - Bruno test query
- `bruno/LocalStack/List Secrets.bru:4,5` - Documentation

### 3. Application Configuration
**Files:** `src/Centro.Api/appsettings.Local.json`

**Current Issues:**
- JWT/Auth configuration: Should be removed entirely (moved to API key auth only)
- Cloud Watch Log Group: `/centro/api/local` (this one is correct)  
- Tenant Registry Table: Should reference `centro-local-tenant-registry`

**Files to Change:**
- `src/Centro.Api/appsettings.Local.json:14-19` - Remove entire Auth section (lines 14-19)
- `src/Centro.Api/appsettings.Local.json:26` - Tenant registry table name

**Note:** Other environments (Dev, Staging, Production) still use JWT auth, but Local environment uses API key auth only.

### 4. Infrastructure Configuration
**Files:** Various configuration and default values

**Current Issues:**
- Default tenant registry table name in code defaults to `centro-dev-tenant-registry`
- Environment variable parsing defaults to "dev"

**Files to Change:**
- `src/Centro.Infrastructure/Providers/Wise/TenantRegistryService.cs:33` - Default table name
- `src/Centro.Infrastructure/Providers/Wise/TenantRegistryService.cs:31` - Environment fallback

### 5. Remove Fragile Environment Patterns
**Files:** Application startup and configuration

**Current Issues:**
- Code uses `IsDevelopment()` which hardcodes "Development" assumptions
- Hardcoded environment name checks in logging configuration  
- Business logic tied to framework environment names instead of actual requirements

**Better Pattern:**
- `IsLowerEnvironment()` - Swagger enabled, debug logging, no metrics recording
- `IsHigherEnvironment()` - Production security, full monitoring, customer-specific configs
- Define based on actual feature requirements, not environment names

**Files to Change:**
- `src/Centro.Api/Program.cs:52` - Replace `IsDevelopment()` check
- `src/Centro.Api/Extensions/LoggingExtensions.cs:*` - Replace hardcoded "Development" checks
- Create environment configuration classes that define capabilities

### 6. Scripts and Documentation
**Files:** Various scripts that reference dev environment

**Current Issues:**
- Several scripts have hardcoded dev references that should be configurable or local-specific

**Files to Review/Update:**
- `scripts/provision-tenant-localstack.sh` - Environment references
- Documentation in Bruno tests referring to "dev" setup

### 6. Scripts and Documentation
**Files to Review/Update:**
- `scripts/provision-tenant-localstack.sh` - Environment references
- Documentation in Bruno tests referring to "dev" setup

## Implementation Plan

### Phase 1: Environment Name Standardization (High Priority)
1. **Rename appsettings files to lowercase**
   - `appsettings.Local.json` → `appsettings.local.json`
   - `appsettings.Dev.json` → `appsettings.dev.json` 
   - `appsettings.Staging.json` → `appsettings.staging.json`
   - `appsettings.Production.json` → `appsettings.production.json`

2. **Update ASPNETCORE_ENVIRONMENT values**
   - `src/Centro.Api/Properties/launchSettings.json` - Change "Local" to "local"
   - `src/Centro.Infrastructure/Ecs/task-definition.json` - Change "Dev" to "dev"
   - Any deployment scripts or configs

### Phase 2: Core Infrastructure (High Priority)
### Phase 2: Core Infrastructure (High Priority)
1. **Database Changes**
   - Update database name, username, password
   - Update all connection strings
   - Update Docker Compose and init scripts

2. **LocalStack Resources**
   - Update DynamoDB table names
   - Update S3 bucket names  
   - Update SQS queue names
   - Update Secrets Manager paths

### Phase 3: Remove Fragile Environment Patterns (High Priority)
1. **Create Environment Configuration**
   - Define `IsLowerEnvironment()` and `IsHigherEnvironment()` logic
   - Replace `IsDevelopment()` usage with capability-based checks
   - Update logging configuration to use capability checks

2. **Update Application Startup**
   - Replace hardcoded environment name checks
   - Enable Swagger based on environment capabilities, not names

### Phase 4: Application Configuration (High Priority)
1. **App Settings**
   - Remove Auth/JWT configuration section (API key auth only)
   - Fix tenant registry table reference
   - Ensure all local-specific configurations

2. **Default Values**
   - Update code defaults to use local naming
   - Fix environment detection logic

### Phase 4: Scripts and Tooling (Medium Priority)
1. **Provisioning Scripts**
   - Update resource name generation
   - Ensure environment-aware naming

2. **Bruno Tests**
   - Update test queries and documentation
   - Ensure tests reference correct local resources

### Phase 5: Validation (High Priority)
1. **End-to-End Testing**
   - Verify all Bruno tests pass
   - Verify tenant provisioning works
   - Verify balance API works with local resources

## Migration Strategy

1. **Create backup** of current working state
2. **Implement changes incrementally** by phase
3. **Test after each phase** to ensure no breakage
4. **Update documentation** as changes are made

## Success Criteria

- [ ] All LocalStack resources use `centro-local-*` naming
- [ ] Database uses `centro_local` naming consistently
- [ ] Application configuration references correct local resources
- [ ] Bruno tests pass against local environment
- [ ] Tenant provisioning works with new naming
- [ ] Balance API successfully calls Wise sandbox API
- [ ] No remaining references to "dev" in local environment configs

## Files Summary

**Critical Files (Must Change):**
- Rename `src/Centro.Api/appsettings.Local.json` → `appsettings.local.json`
- Rename `src/Centro.Api/appsettings.Dev.json` → `appsettings.dev.json`
- Rename `src/Centro.Api/appsettings.Staging.json` → `appsettings.staging.json`
- Rename `src/Centro.Api/appsettings.Production.json` → `appsettings.production.json`
- `src/Centro.Api/Properties/launchSettings.json` - ASPNETCORE_ENVIRONMENT values
- `src/Centro.Infrastructure/Ecs/task-definition.json` - Environment variable
- `docker-compose.yml` 
- `postgres/init/01-create-database.sql`
- `scripts/start-localstack-podman.sh`
- All LocalStack init scripts (`localstack/init/*.sh`)
- `scripts/provision-tenant-localstack.sh`
- `src/Centro.Infrastructure/Providers/Wise/TenantRegistryService.cs`

**Supporting Files (Should Change):**
- Bruno test files referencing local resources
- Bruno environment files (may need lowercase names)
- Documentation files

**Total Estimated Files to Modify:** ~18 files
**Estimated Complexity:** Medium-High (file renames + find-replace operations with verification)