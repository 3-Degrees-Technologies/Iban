# Podman Setup for Centro Development

## Overview

Centro development uses Podman as the container runtime instead of Docker. This guide covers Podman installation, configuration, and Centro-specific usage patterns.

## Why Podman?

Podman offers several advantages for Centro development:

- **Rootless containers** - Enhanced security by default
- **Daemonless architecture** - No background daemon required  
- **Docker compatibility** - Drop-in replacement for most Docker commands
- **Pod management** - Native support for Kubernetes-style pods
- **Better resource isolation** - Uses systemd for container management

## Installation

### Linux (Ubuntu/Debian)
```bash
# Ubuntu 20.04+
sudo apt-get update
sudo apt-get install podman

# For older Ubuntu versions, use official repositories
. /etc/os-release
echo "deb https://download.opensuse.org/repositories/devel:/kubic:/libcontainers:/stable/xUbuntu_${VERSION_ID}/ /" | sudo tee /etc/apt/sources.list.d/devel:kubic:libcontainers:stable.list
curl -L "https://download.opensuse.org/repositories/devel:/kubic:/libcontainers:/stable/xUbuntu_${VERSION_ID}/Release.key" | sudo apt-key add -
sudo apt-get update
sudo apt-get install podman
```

### Linux (RHEL/CentOS/Fedora)
```bash
# RHEL/CentOS 8+
sudo dnf install podman

# RHEL/CentOS 7
sudo yum install podman

# Fedora
sudo dnf install podman
```

### macOS
```bash
# Using Homebrew
brew install podman

# Initialize and start Podman machine
podman machine init
podman machine start
```

### Windows
```bash
# Using Chocolatey
choco install podman

# Or download from GitHub releases
# https://github.com/containers/podman/releases
```

## Configuration

### Registry Configuration
Configure container registries in `/etc/containers/registries.conf`:

```toml
[registries.search]
registries = ['docker.io', 'quay.io']

[registries.insecure]
registries = []

[registries.block]
registries = []
```

### Rootless Configuration
Enable rootless containers (recommended):

```bash
# Check if user namespaces are enabled
cat /proc/sys/user/max_user_namespaces

# If the value is 0, enable user namespaces
echo 'user.max_user_namespaces=15000' | sudo tee -a /etc/sysctl.conf
sudo sysctl -p

# Set up subuid and subgid
sudo usermod --add-subuids 10000-75535 $USER
sudo usermod --add-subgids 10000-75535 $USER

# Restart to apply changes
sudo reboot
```

## Centro-Specific Usage

### Basic Commands
Podman commands are nearly identical to Docker:

```bash
# Run containers
podman run -d --name centro-localstack docker.io/localstack/localstack:latest
podman run -d --name centro-postgres docker.io/postgres:15

# List containers
podman ps
podman ps -a

# Container logs
podman logs centro-localstack
podman logs -f centro-postgres  # Follow logs

# Stop/start containers
podman stop centro-localstack centro-postgres
podman start centro-localstack centro-postgres

# Remove containers
podman rm centro-localstack centro-postgres
```

### Network Management
```bash
# Create Centro network
podman network create centro-network

# List networks
podman network ls

# Inspect network
podman network inspect centro-network

# Remove network
podman network rm centro-network
```

### Volume Management
```bash
# Create named volumes
podman volume create localstack-data
podman volume create postgres-data

# List volumes
podman volume ls

# Inspect volume
podman volume inspect localstack-data

# Remove volumes
podman volume rm localstack-data postgres-data
```

### SELinux Context (Linux)
On SELinux-enabled systems, use `:Z` flag for volume mounts:

```bash
# Correct SELinux labeling
podman run -d \
  -v ./localstack/init:/etc/localstack/init/ready.d:Z \
  -v ./postgres/init:/docker-entrypoint-initdb.d:Z \
  docker.io/localstack/localstack:latest
```

## Centro Container Commands

### LocalStack Container
```bash
# Start LocalStack for Centro development
podman run -d \
  --name centro-localstack \
  --network centro-network \
  -p 4566:4566 \
  -e SERVICES=dynamodb,secretsmanager,s3,sqs,sns,iam,kms,sts,cloudwatch,logs,ecr,ecs,lambda,apigateway,route53,elbv2 \
  -e DEBUG=1 \
  -v ./localstack/init:/etc/localstack/init/ready.d:Z \
  docker.io/localstack/localstack:latest

# Check LocalStack health
curl http://localhost:4566/_localstack/health
```

### PostgreSQL Container
```bash
# Start PostgreSQL for Centro development
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

# Test PostgreSQL connection
psql -h localhost -p 5432 -U centro_dev -d centro_dev -c "SELECT version();"
```

## Systemd Integration

### Auto-start Containers
Create systemd user services for automatic container startup:

```bash
# Generate systemd unit files
podman generate systemd --name centro-localstack --files
podman generate systemd --name centro-postgres --files

# Move to systemd directory
mkdir -p ~/.config/systemd/user
mv container-*.service ~/.config/systemd/user/

# Enable services
systemctl --user daemon-reload
systemctl --user enable container-centro-localstack.service
systemctl --user enable container-centro-postgres.service

# Start services
systemctl --user start container-centro-localstack.service
systemctl --user start container-centro-postgres.service

# Enable linger (start on boot without login)
sudo loginctl enable-linger $USER
```

### Pod Management
Create a Centro pod for related containers:

```bash
# Create a pod
podman pod create --name centro-pod -p 4566:4566 -p 5432:5432

# Add containers to pod
podman run -d --pod centro-pod --name centro-localstack \
  -e SERVICES=... docker.io/localstack/localstack:latest

podman run -d --pod centro-pod --name centro-postgres \
  -e POSTGRES_DB=centro_dev ... docker.io/postgres:15

# Manage entire pod
podman pod stop centro-pod
podman pod start centro-pod
podman pod rm centro-pod
```

## Performance Optimization

### Resource Limits
Set appropriate resource limits for Centro containers:

```bash
# Limit memory and CPU usage
podman run -d \
  --name centro-localstack \
  --memory=2g \
  --cpus=2 \
  docker.io/localstack/localstack:latest
```

### Storage Driver
Configure efficient storage driver:

```bash
# Check current storage driver
podman info | grep -A5 "Storage Driver"

# Configure overlay2 (fastest)
# Edit /etc/containers/storage.conf
[storage]
driver = "overlay"
```

## Docker Compose Alternative

### Using podman-compose
Install podman-compose for Docker Compose compatibility:

```bash
# Install podman-compose
pip3 install podman-compose

# Use with existing docker-compose.yml
podman-compose up -d
podman-compose down
```

### Using Podman with docker-compose
Configure docker-compose to use Podman:

```bash
# Set Podman as Docker socket
export DOCKER_HOST=unix:///run/user/$UID/podman/podman.sock

# Or create Docker socket
sudo systemctl enable podman.socket
sudo systemctl start podman.socket

# Use docker-compose normally
docker-compose up -d
```

## Migration from Docker

### Command Mapping
Most Docker commands work with Podman:

| Docker Command | Podman Command | Notes |
|----------------|----------------|-------|
| `docker run` | `podman run` | Identical |
| `docker ps` | `podman ps` | Identical |
| `docker logs` | `podman logs` | Identical |
| `docker exec` | `podman exec` | Identical |
| `docker-compose` | `podman-compose` | Requires installation |

### Key Differences
- **No daemon** - Podman commands run directly
- **Rootless by default** - Enhanced security
- **Different storage locations** - `~/.local/share/containers/`
- **SELinux integration** - Better security on RHEL/CentOS

## Troubleshooting

### Common Issues

#### "short-name did not resolve"
```bash
# Error: short-name "postgres:15" did not resolve to an alias
# Solution: Use full registry path
podman run docker.io/postgres:15  # instead of postgres:15
```

#### Permission Denied on Volume Mounts
```bash
# Error: Permission denied
# Solution: Use :Z flag for SELinux
podman run -v ./data:/data:Z image_name
```

#### Rootless Networking Issues
```bash
# Error: Cannot bind to privileged ports
# Solution: Use port mapping or run as root
podman run -p 8080:80 image_name  # Map to unprivileged port
```

### Debugging Commands
```bash
# Check Podman version and configuration
podman version
podman info

# Check storage usage
podman system df

# Clean up unused resources
podman system prune -a

# Check user namespace configuration
podman unshare cat /proc/self/uid_map
```

## Best Practices for Centro

1. **Use rootless containers** - Default mode is more secure
2. **Create dedicated network** - Isolate Centro containers
3. **Use named volumes** - For data persistence
4. **Set resource limits** - Prevent resource exhaustion
5. **Use full image names** - Avoid registry resolution issues
6. **Enable systemd integration** - For automatic startup
7. **Regular cleanup** - Remove unused images and containers

```bash
# Recommended Centro startup script
#!/bin/bash
set -euo pipefail

# Create network if it doesn't exist
podman network exists centro-network || podman network create centro-network

# Start PostgreSQL
podman run -d --name centro-postgres \
  --network centro-network \
  --memory=1g \
  -p 5432:5432 \
  -e POSTGRES_DB=centro_dev \
  -e POSTGRES_USER=centro_dev \
  -e POSTGRES_PASSWORD=devpassword \
  -v ./postgres/init:/docker-entrypoint-initdb.d:Z \
  docker.io/postgres:15

# Start LocalStack
podman run -d --name centro-localstack \
  --network centro-network \
  --memory=2g \
  -p 4566:4566 \
  -e SERVICES=dynamodb,secretsmanager,s3,sqs,sns,iam,kms,sts,cloudwatch,logs \
  -v ./localstack/init:/etc/localstack/init/ready.d:Z \
  docker.io/localstack/localstack:latest

echo "Centro containers started successfully!"
```

## Related Documentation

- [LocalStack Setup](./localstack-setup.md)
- [Troubleshooting](./localstack-troubleshooting.md)
- [Development Setup](../DEVELOPMENT-SETUP.md)