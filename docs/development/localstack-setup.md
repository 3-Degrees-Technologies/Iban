# LocalStack Development Setup

## Overview

LocalStack provides a local development environment that emulates AWS services, allowing you to develop and test Centro without connecting to real AWS infrastructure. This guide covers setting up LocalStack with Podman for Centro development.

## Prerequisites

- **Podman** (Container runtime - we use Podman instead of Docker)
- **AWS CLI** (for interacting with LocalStack services)
- **jq** (for JSON processing in scripts)

### Installing Prerequisites

```bash
# Install Podman (varies by OS)
# Ubuntu/Debian:
sudo apt-get install podman

# macOS:
brew install podman

# Install AWS CLI
pip install awscli

# Install jq
sudo apt-get install jq  # Ubuntu/Debian
brew install jq          # macOS
```

## Services Provided

LocalStack provides these AWS services for Centro development:

- **DynamoDB** - Tenant registry and configuration storage
- **Secrets Manager** - Provider API keys and credentials
- **S3** - File storage and backups
- **SQS/SNS** - Message queuing and notifications
- **CloudWatch** - Metrics and logging
- **IAM/STS** - Identity and access management
- **ECS** - Container orchestration (for testing deployments)

## Container Setup

### Starting LocalStack and PostgreSQL

**Quick Start (Recommended):**
```bash
# Automated setup with scripts
./scripts/start-localstack-podman.sh

# Provision test tenant
./scripts/provision-tenant-localstack.sh threedegrees-tenant-config.json create

# Verify setup
curl http://localhost:4566/_localstack/health
curl http://localhost:5139/api/balance/threedegrees
```

**Manual Setup:**
Centro uses two main containers for local development:

1. **LocalStack** - AWS services emulation
2. **PostgreSQL** - Database (replaces AWS RDS)

#### Method 1: Individual Container Commands (Recommended for Podman)

```bash
# Create network
podman network create centro-network

# Start PostgreSQL
podman run -d \
  --name centro-postgres \
  --network centro-network \
  -p 5432:5432 \
  -e POSTGRES_DB=centro_dev \
  -e POSTGRES_USER=centro_dev \
  -e POSTGRES_PASSWORD=devpassword \
  -e POSTGRES_INITDB_ARGS="--encoding=UTF-8 --lc-collate=C --lc-ctype=C" \
  -v ./postgres/init:/docker-entrypoint-initdb.d:Z \
  docker.io/postgres:15

# Start LocalStack
podman run -d \
  --name centro-localstack \
  --network centro-network \
  -p 4566:4566 \
  -e SERVICES=dynamodb,secretsmanager,s3,sqs,sns,iam,kms,sts,cloudwatch,logs,ecr,ecs,lambda,apigateway,route53,elbv2 \
  -e DEBUG=1 \
  -v ./localstack/init:/etc/localstack/init/ready.d:Z \
  docker.io/localstack/localstack:latest
```

#### Method 2: Using Docker Compose (if available)

```bash
# If you have docker-compose or podman-compose
podman-compose up -d  # or docker-compose up -d
```

### Verifying Setup

```bash
# Check containers are running
podman ps

# Test LocalStack health
curl http://localhost:4566/_localstack/health

# Test PostgreSQL
psql -h localhost -p 5432 -U centro_dev -d centro_dev -c "SELECT version();"
```

Expected LocalStack health response:
```json
{
  "services": {
    "dynamodb": "running",
    "secretsmanager": "running",
    "s3": "running",
    ...
  }
}
```

## AWS CLI Configuration

Configure AWS CLI to work with LocalStack:

```bash
# Configure AWS CLI for LocalStack
aws configure set aws_access_key_id test
aws configure set aws_secret_access_key test
aws configure set region eu-west-1

# Test configuration
aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets
```

## Environment Configuration

Set these environment variables for Centro development:

```bash
export LOCALSTACK_ENDPOINT=http://localhost:4566
export AWS_REGION=eu-west-1
export DATABASE_CONNECTION_STRING="Server=localhost;Port=5432;Database=centro_dev;User Id=centro_dev;Password=devpassword;"
```

Add to your shell profile (`.bashrc`, `.zshrc`, etc.) for persistence.

## Container Management

### Starting Containers
```bash
podman start centro-localstack centro-postgres
```

### Stopping Containers
```bash
podman stop centro-localstack centro-postgres
```

### Viewing Logs
```bash
# LocalStack logs
podman logs centro-localstack

# PostgreSQL logs  
podman logs centro-postgres

# Follow logs in real-time
podman logs -f centro-localstack
```

### Cleaning Up
```bash
# Remove containers
podman rm centro-localstack centro-postgres

# Remove network
podman network rm centro-network

# Remove images (optional)
podman rmi docker.io/localstack/localstack:latest docker.io/postgres:15
```

## Data Persistence

### LocalStack Data
By default, LocalStack data is ephemeral. For persistence across container restarts:

```bash
# Create a volume for LocalStack data
podman volume create localstack-data

# Start LocalStack with persistent volume
podman run -d \
  --name centro-localstack \
  --network centro-network \
  -p 4566:4566 \
  -e SERVICES=... \
  -v localstack-data:/tmp/localstack/data \
  -v ./localstack/init:/etc/localstack/init/ready.d:Z \
  docker.io/localstack/localstack:latest
```

### PostgreSQL Data
PostgreSQL data is automatically persisted in a Docker volume named `postgres_data`.

## Integration with Centro API

Once LocalStack is running, configure Centro API to use local services:

```json
// appsettings.Development.json
{
  "Aws": {
    "ServiceURL": "http://localhost:4566",
    "Profile": "default",
    "Region": "eu-west-1"
  },
  "Database": {
    "ConnectionString": "Server=localhost;Port=5432;Database=centro_dev;User Id=centro_dev;Password=devpassword;"
  }
}
```

## Initialization Scripts

LocalStack automatically runs initialization scripts in `localstack/init/` on startup:

- `01-setup-dynamodb.sh` - Creates DynamoDB tables
- `02-setup-secrets.sh` - Creates initial secrets
- `03-setup-s3.sh` - Sets up S3 buckets
- `04-setup-sqs.sh` - Configures message queues
- `05-setup-event-tables.sh` - Event store setup

These scripts ensure your local environment matches production structure.

## Next Steps

1. [Tenant Provisioning](./tenant-provisioning.md) - Add tenants to your local environment
2. [Troubleshooting](./localstack-troubleshooting.md) - Common issues and solutions
3. [Development Workflow](../DEVELOPMENT-SETUP.md) - Full development setup guide

## Related Documentation

- [Secrets Management](../knowledge/secrets-management.md)
- [Tenant Configuration](../knowledge/tenant-configuration.md)
- [DynamoDB Service](../knowledge/dynamodb-service.md)