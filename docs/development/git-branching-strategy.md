# Git Branching Strategy for Centro

## Overview

Centro uses a **environment-based branching strategy** that mirrors our deployment pipeline and supports multiple developers working safely in isolation.

## Branch Structure

### Environment Branches
- `main` - Production-ready code, deployed to production automatically
- `test` - Staging environment, deployed to test automatically  
- `dev` - Development environment, deployed to dev automatically
- `local-{developer}` - Individual developer branches (e.g., `local-sam`, `local-esteban`)

### Flow Direction with Automatic Deployments
```
local-sam → dev (auto-deploy to dev environment)
              ↓  
            test (auto-deploy to test environment)
              ↓
            main (auto-deploy to production environment)
```

**🚀 CI/CD Integration**: Each push to `dev`, `test`, or `main` automatically triggers deployment to the corresponding AWS environment. See [CI/CD Integration Guide](cicd-integration.md) for details.

## Branch Purposes

### `main` Branch
- **Purpose**: Production-ready code only
- **Protection**: 🔒 Protected branch - no direct commits
- **Deployment**: ✅ Automatically deployed to production AWS environment
- **Access**: Merge only via pull requests from `test`

### `test` Branch
- **Purpose**: Pre-production testing and validation
- **Protection**: 🔒 Protected branch - no direct commits  
- **Deployment**: ✅ Automatically deployed to staging/test AWS environment
- **Access**: Merge only via pull requests from `dev`

### `dev` Branch
- **Purpose**: Integration testing of multiple features
- **Protection**: ⚠️ Semi-protected - merge via pull requests preferred
- **Deployment**: ✅ Automatically deployed to development AWS environment
- **Access**: Merge via pull requests from `local-*` branches

### `local-{developer}` Branches
- **Purpose**: Individual developer work and experimentation
- **Protection**: ❌ No protection - developer has full control
- **Deployment**: ❌ No automatic deployment - local development only
- **Access**: Developer works directly on this branch

## Workflow

### 1. Daily Development (Developer)
```bash
# Work on your personal branch
git checkout local-sam
git pull origin local-sam

# Make changes, commit frequently
git add .
git commit -m "Add corridor optimization logic"
git push origin local-sam
```

### 2. Feature Ready for Integration
```bash
# Create pull request: local-sam → dev
gh pr create --base dev --head local-sam \
  --title "Add corridor optimization for USD-GBP routes" \
  --body "Implements routing logic for high-volume corridors"
```

### 3. Development Testing Complete
```bash
# Create pull request: dev → test
gh pr create --base test --head dev \
  --title "Release v1.2.0 - Corridor optimization features" \
  --body "Ready for staging validation"
```

### 4. Production Release
```bash
# Create pull request: test → main
gh pr create --base main --head test \
  --title "Production Release v1.2.0" \
  --body "Final release with corridor optimization"
```

## Branch Policies

### Main Branch Protection
- ✅ Require pull request reviews (2 reviewers)
- ✅ Require status checks to pass
- ✅ Require up-to-date branches
- ✅ Include administrators in restrictions
- ✅ No force pushes allowed

### Test Branch Protection
- ✅ Require pull request reviews (1 reviewer)
- ✅ Require status checks to pass
- ✅ Allow administrators to bypass

### Dev Branch Guidelines
- ⚠️ Pull requests preferred but not required
- ✅ Status checks should pass
- ✅ Merge commits allowed for integration

### Local Branch Freedom
- ❌ No restrictions
- ✅ Force pushes allowed (developer discretion)
- ✅ Direct commits encouraged for rapid iteration

## Development Patterns

### TDD Workflow on Local Branch
```bash
# On local-sam branch
git checkout local-sam

# Red - Write failing test
git add tests/
git commit -m "Add failing test for corridor optimization"

# Green - Implement minimal solution
git add src/
git commit -m "Implement basic corridor optimization"

# Refactor - Clean up implementation
git add src/
git commit -m "Refactor corridor logic for clarity"

# Push frequently
git push origin local-sam
```

### Hotfix Workflow
For critical production fixes:
```bash
# Create hotfix branch from main
git checkout main
git pull origin main
git checkout -b hotfix/critical-security-fix

# Fix the issue
git add .
git commit -m "Fix security vulnerability in auth middleware"

# Push and create urgent PR to main
git push origin hotfix/critical-security-fix
gh pr create --base main --head hotfix/critical-security-fix \
  --title "URGENT: Fix auth security vulnerability"
```

### Sync Pattern
Keep your local branch updated:
```bash
# Daily sync with dev branch
git checkout local-sam
git fetch origin
git merge origin/dev  # Incorporate team changes

# Or rebase for cleaner history (use with caution)
git rebase origin/dev
```

## Integration Strategy

### Code Review Requirements
- **Main**: 2 senior developers (Sam + Esteban)
- **Test**: 1 senior developer
- **Dev**: Peer review (optional but encouraged)
- **Local**: No review required

### CI/CD Integration
Each branch triggers appropriate environment deployment:
- `main` → Production deployment (centro-production-cluster)
- `test` → Staging deployment (centro-test-cluster)
- `dev` → Development deployment (centro-dev-cluster)  
- `local-*` → No automatic deployment

**📚 Detailed CI/CD Documentation**: See [CI/CD Integration Guide](cicd-integration.md) for complete automation details.

### Merge Strategies
- **Main**: Squash merge (clean history)
- **Test**: Merge commit (preserve integration points)
- **Dev**: Merge commit (preserve feature context)
- **Local**: Any strategy (developer choice)

## Migration Steps

### Setting Up Your Local Branch
```bash
# Create your personal branch from current main
git checkout main
git pull origin main
git checkout -b local-sam
git push -u origin local-sam

# Configure as your default working branch
git config branch.local-sam.description "Sam's local development branch"
```

### Existing Work Migration
If you have uncommitted work on main:
```bash
# Stash current work
git stash

# Switch to your local branch
git checkout local-sam

# Apply your changes
git stash pop

# Commit to your branch
git add .
git commit -m "Migrate existing work to local-sam branch"
git push origin local-sam
```

## Best Practices

### Commit Messages
Use conventional commits format:
```
type(scope): description

feat(balance): add Wise provider integration
fix(auth): resolve token validation issue
docs(api): update endpoint documentation
test(quote): add corridor optimization tests
```

### Branch Naming
- `local-{firstname}` - Personal development branches
- `feature/{feature-name}` - Temporary feature branches (optional)
- `hotfix/{issue-description}` - Critical production fixes
- `release/{version}` - Release preparation (if needed)

### Keep Branches Clean
```bash
# Regularly clean up merged branches
git branch --merged dev | grep -v dev | xargs git branch -d

# Prune remote tracking branches
git remote prune origin
```

### Security Considerations
- **Never commit secrets** to any branch
- **Use environment-specific configuration** files
- **Keep API keys in secure stores** (AWS Secrets Manager)
- **Review changes carefully** before merging to protected branches

## Emergency Procedures

### Broken Main Branch
```bash
# Immediate rollback using previous commit
git checkout main
git reset --hard HEAD~1
git push --force-with-lease origin main
```

### Corrupted Local Branch
```bash
# Reset to known good state
git checkout local-sam
git reset --hard origin/dev  # Or origin/main
git push --force-with-lease origin local-sam
```

### Lost Work Recovery
```bash
# Find lost commits using reflog
git reflog
git checkout -b recovery-branch commit-hash
```

## Tools and Automation

### Recommended Git Aliases
```bash
git config --global alias.sw 'switch'
git config --global alias.br 'branch'
git config --global alias.co 'checkout'
git config --global alias.st 'status'
git config --global alias.pr '!gh pr create'
```

### VS Code Integration
Configure `.vscode/settings.json`:
```json
{
  "git.defaultCloneDirectory": "~/Workspace",
  "git.autofetch": true,
  "git.confirmSync": false,
  "gitflow.feature.base": "dev",
  "gitflow.release.base": "dev"
}
```

This branching strategy ensures:
- ✅ **Safe experimentation** on personal branches
- ✅ **Controlled integration** through environment progression
- ✅ **Production stability** with protected main branch
- ✅ **Team coordination** without stepping on each other
- ✅ **Rollback capability** at every environment level