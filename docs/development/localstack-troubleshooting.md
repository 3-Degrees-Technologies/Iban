# LocalStack Troubleshooting Guide

## Overview

This guide covers common issues you might encounter when setting up and using LocalStack with Centro, along with their solutions.

## Container Issues

### LocalStack Container Won't Start

#### Error: Data Volume Busy
```
ERROR: 'rm -rf "/tmp/localstack"': exit code 1; output: b"rm: cannot remove '/tmp/localstack/data': Device or resource busy\n"
ERROR: the LocalStack runtime exited unexpectedly: [Errno 16] Device or resource busy: 'data'
```

**Solution**: Remove the data volume mount or use a named volume:
```bash
# Remove existing container
podman rm centro-localstack

# Start without data volume (ephemeral)
podman run -d --name centro-localstack --network centro-network \
  -p 4566:4566 \
  -e SERVICES=dynamodb,secretsmanager,s3,sqs,sns,iam,kms,sts,cloudwatch,logs \
  -v ./localstack/init:/etc/localstack/init/ready.d:Z \
  docker.io/localstack/localstack:latest

# OR use named volume for persistence
podman volume create localstack-data
podman run -d --name centro-localstack --network centro-network \
  -p 4566:4566 \
  -e SERVICES=... \
  -v localstack-data:/tmp/localstack/data \
  docker.io/localstack/localstack:latest
```

#### Error: Image Not Found
```
Error: short-name "postgres:15" did not resolve to an alias and no unqualified-search registries are defined
```

**Solution**: Use fully qualified image names:
```bash
# Use full registry path
podman run -d --name centro-postgres \
  docker.io/postgres:15  # instead of just postgres:15
```

#### Error: Port Already in Use
```
Error: cannot listen on the TCP port: listen tcp4 0.0.0.0:4566: bind: address already in use
```

**Solution**: Check for existing containers or processes:
```bash
# Check what's using the port
sudo lsof -i :4566
podman ps -a | grep 4566

# Stop conflicting containers
podman stop centro-localstack
podman rm centro-localstack

# Or use a different port
podman run -d -p 4567:4566 ...  # Map to port 4567 instead
```

### PostgreSQL Container Issues

#### Error: Database Connection Refused
```
psql: error: connection to server at "localhost", port 5432 failed: Connection refused
```

**Solutions**:
1. **Check container status**:
   ```bash
   podman ps | grep postgres
   podman logs centro-postgres
   ```

2. **Wait for PostgreSQL to fully start**:
   ```bash
   # PostgreSQL takes time to initialize
   sleep 10
   podman logs centro-postgres | grep "ready to accept connections"
   ```

3. **Check network connectivity**:
   ```bash
   # Test from another container
   podman run --rm --network centro-network postgres:15 \
     psql -h centro-postgres -U centro_dev -d centro_dev -c "SELECT 1;"
   ```

## LocalStack Service Issues

### Services Not Available

#### Check Service Status
```bash
# Check which services are running
curl http://localhost:4566/_localstack/health | jq
```

Expected response:
```json
{
  "services": {
    "dynamodb": "running",
    "secretsmanager": "running",
    "s3": "running"
  }
}
```

#### Service Shows as "disabled"
If a service shows as "disabled", check the SERVICES environment variable:
```bash
# Check current services
podman inspect centro-localstack | grep -A5 -B5 SERVICES

# Restart with correct services
podman stop centro-localstack
podman rm centro-localstack
podman run -d --name centro-localstack \
  -e SERVICES=dynamodb,secretsmanager,s3,sqs,sns,iam,kms,sts,cloudwatch,logs \
  ...
```

### AWS CLI Connection Issues

#### Error: Could not connect to endpoint
```
Could not connect to the endpoint URL: "https://dynamodb.eu-west-1.amazonaws.com/"
```

**Solution**: Ensure you're using the LocalStack endpoint:
```bash
# Always use --endpoint-url with LocalStack
aws --endpoint-url=http://localhost:4566 dynamodb list-tables --region eu-west-1

# Set as environment variable
export AWS_ENDPOINT_URL=http://localhost:4566
```

#### Error: Credentials not found
```
Unable to locate credentials. You can configure credentials by running "aws configure".
```

**Solution**: Configure dummy credentials for LocalStack:
```bash
aws configure set aws_access_key_id test
aws configure set aws_secret_access_key test
aws configure set region eu-west-1

# Or use environment variables
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=eu-west-1
```

## Tenant Provisioning Issues

### Script Permission Denied
```bash
./scripts/provision-tenant-localstack.sh: Permission denied
```

**Solution**: Make script executable:
```bash
chmod +x scripts/provision-tenant-localstack.sh
```

### JSON Parsing Errors
```
jq: error (at <stdin>:0): Invalid numeric literal at line 1, column 4
```

**Solution**: Check tenant config file format:
```bash
# Validate JSON syntax
jq '.' tenant_config_file.json

# Check for required fields
jq '.tenant.id' tenant_config_file.json
jq '.provider_credentials[] | select(.enabled == true)' tenant_config_file.json
```

### Secret Creation Fails
```
An error occurred (AccessDenied) when calling the CreateSecret operation
```

**Solutions**:
1. **Check LocalStack is running**:
   ```bash
   curl http://localhost:4566/_localstack/health
   ```

2. **Verify AWS CLI configuration**:
   ```bash
   aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets --region eu-west-1
   ```

3. **Check LocalStack logs**:
   ```bash
   podman logs centro-localstack | tail -20
   ```

### DynamoDB Table Not Found
```
ResourceNotFoundException: Requested resource not found
```

**Solution**: Ensure initialization scripts ran:
```bash
# Check if table exists
aws --endpoint-url=http://localhost:4566 dynamodb list-tables --region eu-west-1

# Check LocalStack logs for initialization
podman logs centro-localstack | grep -i dynamodb

# Manually create table if needed
aws --endpoint-url=http://localhost:4566 dynamodb create-table \
  --table-name centro-dev-tenant-registry \
  --attribute-definitions AttributeName=TenantId,AttributeType=S \
  --key-schema AttributeName=TenantId,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST \
  --region eu-west-1
```

## Network Issues

### Container Communication Problems
If containers can't communicate:

1. **Check network exists**:
   ```bash
   podman network ls | grep centro-network
   ```

2. **Verify containers are on same network**:
   ```bash
   podman inspect centro-localstack | grep NetworkMode
   podman inspect centro-postgres | grep NetworkMode
   ```

3. **Test connectivity between containers**:
   ```bash
   # From LocalStack to PostgreSQL
   podman exec centro-localstack ping centro-postgres
   
   # From PostgreSQL to LocalStack
   podman exec centro-postgres curl http://centro-localstack:4566/_localstack/health
   ```

### Host to Container Issues
If you can't connect from host to containers:

1. **Check port mappings**:
   ```bash
   podman port centro-localstack
   podman port centro-postgres
   ```

2. **Test from host**:
   ```bash
   curl http://localhost:4566/_localstack/health
   psql -h localhost -p 5432 -U centro_dev -d centro_dev -c "SELECT 1;"
   ```

3. **Check firewall settings** (Linux):
   ```bash
   sudo ufw status
   sudo iptables -L | grep 4566
   ```

## Performance Issues

### Slow LocalStack Startup
LocalStack can take 30-60 seconds to fully start:

```bash
# Wait for services to be ready
timeout 60 bash -c 'until curl -s http://localhost:4566/_localstack/health > /dev/null; do sleep 2; done'

# Check startup progress
podman logs -f centro-localstack
```

### High Memory Usage
LocalStack can use significant memory:

```bash
# Check container resource usage
podman stats centro-localstack

# Reduce services if needed
-e SERVICES=dynamodb,secretsmanager  # Only essential services
```

## Data Persistence Issues

### Data Lost After Container Restart
By default, LocalStack data is ephemeral:

```bash
# Use named volume for persistence
podman volume create localstack-data
podman run -d --name centro-localstack \
  -v localstack-data:/tmp/localstack/data \
  ...
```

### Volume Mount Permission Issues (SELinux)
On systems with SELinux:

```bash
# Use :Z flag for SELinux labeling
-v ./localstack/init:/etc/localstack/init/ready.d:Z
```

## Integration Issues

### Centro API Can't Connect to LocalStack
Check Centro API configuration:

```json
// appsettings.Development.json
{
  "Aws": {
    "ServiceURL": "http://localhost:4566",
    "Region": "eu-west-1"
  }
}
```

### Wise API Mock vs Real Calls
If you're seeing unexpected mock responses:

1. **Check tenant configuration**:
   ```bash
   aws --endpoint-url=http://localhost:4566 secretsmanager get-secret-value \
     --secret-id "centro/dev/threedegrees/wise" --region eu-west-1
   ```

2. **Verify API key format**:
   ```json
   // Should be a JSON string
   "\"4870ef4b-c995-46eb-9044-bd39b3c1d417\""
   ```

3. **Check Centro logs** for tenant resolution and provider selection

## Debugging Commands

### Useful Debug Commands
```bash
# Container status
podman ps -a

# Service health
curl http://localhost:4566/_localstack/health | jq

# Container logs
podman logs centro-localstack --tail 50
podman logs centro-postgres --tail 50

# Network inspection
podman network inspect centro-network

# Resource usage
podman stats

# List all secrets
aws --endpoint-url=http://localhost:4566 secretsmanager list-secrets --region eu-west-1

# List DynamoDB tables
aws --endpoint-url=http://localhost:4566 dynamodb list-tables --region eu-west-1

# Check tenant registry
aws --endpoint-url=http://localhost:4566 dynamodb scan \
  --table-name centro-dev-tenant-registry --region eu-west-1
```

### Environment Check Script
Create a quick health check script:

```bash
#!/bin/bash
echo "=== Centro LocalStack Health Check ==="
echo "Containers:"
podman ps | grep centro

echo -e "\nLocalStack health:"
curl -s http://localhost:4566/_localstack/health | jq -r '.services | to_entries[] | "\(.key): \(.value)"'

echo -e "\nPostgreSQL connection:"
psql -h localhost -p 5432 -U centro_dev -d centro_dev -c "SELECT 'Connected successfully';" 2>/dev/null || echo "Failed"

echo -e "\nTenant registry:"
aws --endpoint-url=http://localhost:4566 dynamodb describe-table \
  --table-name centro-dev-tenant-registry --region eu-west-1 \
  --query 'Table.TableStatus' --output text 2>/dev/null || echo "Table not found"
```

## Getting Help

If you're still experiencing issues:

1. **Check LocalStack logs** for error details
2. **Verify all prerequisites** are installed
3. **Try with minimal configuration** first
4. **Check LocalStack documentation** for version-specific issues
5. **Test with Docker** to rule out Podman-specific issues

## Related Documentation

- [LocalStack Setup](./localstack-setup.md)
- [Tenant Provisioning](./tenant-provisioning.md)
- [Development Setup](../DEVELOPMENT-SETUP.md)