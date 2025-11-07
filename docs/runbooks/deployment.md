# Deployment Runbook

## Overview
Step-by-step procedures for deploying Centro applications.

## Prerequisites
- AWS CLI configured with appropriate permissions
- Docker installed and configured
- Access to ECR repository
- ECS cluster running

## Deployment Process

### 1. Build and Test
```bash
# Run tests
dotnet test

# Build application
dotnet build --configuration Release
```

### 2. Docker Build
```bash
# Build Docker image
docker build -t centro-api .

# Tag for ECR
docker tag centro-api:latest {account}.dkr.ecr.{region}.amazonaws.com/centro-api:latest
```

### 3. Push to ECR
```bash
# Login to ECR
aws ecr get-login-password --region {region} | docker login --username AWS --password-stdin {account}.dkr.ecr.{region}.amazonaws.com

# Push image
docker push {account}.dkr.ecr.{region}.amazonaws.com/centro-api:latest
```

### 4. Update ECS Service
- GitHub Actions workflow handles this automatically
- See `.github/workflows/deploy-to-ecs.yml`
- Manual deployment via AWS CLI if needed

## Health Checks
- Application health endpoint: `/health`
- ECS service health checks configured in task definition
- CloudWatch alarms monitor application metrics

## Rollback Procedure
1. Identify last known good image tag
2. Update ECS service with previous image
3. Monitor health checks and metrics
4. Verify application functionality

## Related Files
- `.github/workflows/deploy-to-ecs.yml`
- `Dockerfile`
- `src/Centro.Infrastructure/Ecs/task-definition.json`