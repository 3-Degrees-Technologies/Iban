# LocalStack Development Setup

## Quick Start Options

### Option 1: Using Docker Compose
```bash
docker-compose up -d
```

### Option 2: Using Podman
```bash
# Start LocalStack with Podman
podman run -d --name centro-localstack \
  -p 4566:4566 \
  -e SERVICES=dynamodb,secretsmanager \
  -e DEBUG=1 \
  -v $(pwd)/localstack/init:/etc/localstack/init/ready.d:Z \
  docker.io/localstack/localstack:latest

# Or without volume mount (manual setup required)
podman run -d --name centro-localstack \
  -p 4566:4566 \
  -e SERVICES=dynamodb,secretsmanager \
  -e DEBUG=1 \
  docker.io/localstack/localstack:latest
```

### Option 3: Mock Services Only (No LocalStack)
The API is already configured to use mock services in development mode. Just run:
```bash
cd src/Centro.Api
dotnet run
```

## Verify Services

```bash
# Check DynamoDB tables
aws --endpoint-url=http://localhost:4566 dynamodb list-tables --region eu-west-1

# Check Secrets Manager
aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets --region eu-west-1
```

## Test the API

```bash
# Test the balance endpoint
curl http://localhost:5000/api/balance/threedegrees
```

## Manual LocalStack Setup (if auto-init fails)

If the initialization scripts don't run automatically, set up manually:

```bash
# Create DynamoDB table
aws --endpoint-url=http://localhost:4566 dynamodb create-table \
  --table-name centro-dev-tenant-registry \
  --attribute-definitions AttributeName=TenantId,AttributeType=S \
  --key-schema AttributeName=TenantId,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST \
  --region eu-west-1

# Add test tenant
aws --endpoint-url=http://localhost:4566 dynamodb put-item \
  --table-name centro-dev-tenant-registry \
  --item '{"TenantId":{"S":"threedegrees"},"WiseProfileId":{"S":"12345678"},"WiseSecretsArn":{"S":"arn:aws:secretsmanager:eu-west-1:000000000000:secret:centro/threedegrees/wise-api-token-abcdef"}}' \
  --region eu-west-1

# Create secret
aws --endpoint-url=http://localhost:4566 secretsmanager create-secret \
  --name "centro/threedegrees/wise-api-token" \
  --secret-string "test-wise-api-token-12345" \
  --region eu-west-1
```

## Configuration Modes

The API supports three modes:

1. **Mock Mode** (default): Uses in-memory mock services
2. **LocalStack Mode**: Set `AWS:UseLocalStack: true` in appsettings
3. **Production Mode**: Set `AWS:UseLocalStack: false` for real AWS

## Stopping Services

```bash
# Docker Compose
docker-compose down

# Podman
podman stop centro-localstack && podman rm centro-localstack
```

## Troubleshooting

- **Port conflicts**: Make sure port 4566 is available
- **AWS CLI**: Install AWS CLI for testing: `pip install awscli-local`
- **Container logs**: 
  - Docker: `docker-compose logs localstack`
  - Podman: `podman logs centro-localstack`
- **Mock mode**: If LocalStack fails, the API works with mock services by default