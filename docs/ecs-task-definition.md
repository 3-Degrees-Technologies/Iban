# ECS Task Definition for Centro

This task definition template is a representation of how the Centro application should be containerized and deployed to ECS. It serves as:

1. Documentation of the application's runtime requirements
2. A validation point to ensure proper configuration
3. A reference for developers to understand the application's container environment

## Platform Integration

The actual deployment of Centro uses the CloudFormation templates in the [aws-infra](https://github.com/3-Degrees-Technologies/aws-infra) repository, which defines:

- ECS Cluster
- CloudWatch Log Groups
- IAM Roles
- Load Balancer configuration
- Auto-scaling policies

These platform resources are referenced in this task definition using placeholders that will be substituted during the deployment process.

## CloudWatch Logs

CloudWatch log groups are defined and managed by the platform (aws-infra repository). The task definition in this repository merely references those log groups using the standardized naming pattern:

`/ecs/${PROJECT_NAME}-${ENVIRONMENT}`

## Multi-Region Support

The task definition supports deployment to multiple regions by using the `${AWS_REGION}` placeholder. This ensures that region-specific resources (ECR repositories, CloudWatch log groups) are correctly referenced regardless of deployment region.

## Template Structure

This task definition follows the ECS Task Definition specification and includes:

- CPU and memory allocations
- Container definitions
- Port mappings
- Health check configuration
- Environment variables
- Logging configuration

## Container Health Check

The health check is configured to verify the application's `/health` endpoint, ensuring that the container is properly functioning before receiving traffic.

## Relationship with Platform Infrastructure

The task definition in this repository is primarily for application-specific configuration and testing purposes. The actual deployment uses the CloudFormation template in the aws-infra repository.