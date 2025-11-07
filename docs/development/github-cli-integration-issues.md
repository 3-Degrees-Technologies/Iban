# GitHub CLI Integration Issues

## Problem
The `gh` CLI cannot create pull requests programmatically due to SSH key configuration issues with the 3-Degrees-Technologies organization.

## Current Workaround
Manual PR creation via GitHub web interface using generated URLs.

## Root Cause Analysis
1. **SSH Key Mismatch**: The repository uses `git@github.com-3degrees:3-Degrees-Technologies/Centro.git` but `gh` CLI expects standard `git@github.com`
2. **Authentication Context**: `gh` CLI is authenticated to personal account (`appmancer`) but needs organization-specific SSH key access
3. **Host Configuration**: SSH config maps `github.com-3degrees` to use specific SSH key for 3 Degrees organization

## Error Messages Observed
```bash
# gh CLI errors
error connecting to github.com-3degrees
check your internet connection or https://githubstatus.com

# API errors  
{
  "message": "Bad credentials",
  "documentation_url": "https://docs.github.com/rest",
  "status": "401"
}
```

## Current SSH Configuration
```bash
$ git remote -v
origin	git@github.com-3degrees:3-Degrees-Technologies/Centro.git (fetch)
origin	git@github.com-3degrees:3-Degrees-Technologies/Centro.git (push)

$ gh auth status
✓ Logged in to github.com account appmancer
- Git operations protocol: ssh
- Token scopes: 'gist', 'read:org', 'repo'
```

## Solutions to Investigate

### Option 1: Fix gh CLI Configuration
```bash
# Investigate gh CLI host configuration
gh auth login --hostname github.com-3degrees
# OR
GH_HOST=github.com-3degrees gh pr create ...
```

### Option 2: Custom GitHub API Tool
Create a custom tool that:
- Uses the correct SSH key configuration
- Leverages existing git authentication
- Handles organization-specific repositories
- Provides same functionality as `gh pr create`

```bash
# Proposed tool usage
./scripts/create-pr.sh --base dev --head local-sam --title "..." --body "..."
```

### Option 3: Git-Based PR Creation
Use git push with special refs to create PRs:
```bash
# Some git hosting services support this
git push origin local-sam:refs/for/dev
```

### Option 4: GitHub API with Personal Access Token
Configure organization-specific PAT for API access:
```bash
# Use PAT with proper organization permissions
curl -H "Authorization: token $GITHUB_TOKEN_3DEGREES" \
  https://api.github.com/repos/3-Degrees-Technologies/Centro/pulls
```

## Immediate Action Items
- [ ] Research gh CLI host configuration for organization repositories
- [ ] Investigate SSH key sharing between git and gh CLI
- [ ] Create custom PR creation script if needed
- [ ] Document proper authentication setup for team

## Impact
- **Development Velocity**: Manual PR creation slows down workflow automation
- **CI/CD Integration**: Reduces effectiveness of automated development processes
- **Team Productivity**: Forces manual steps in otherwise automated workflows

## Priority
**Medium-High** - This affects the core development workflow and should be resolved to maintain automation benefits.

## Notes
- Current workaround (manual PR creation) is functional but not ideal
- SSH key configuration works perfectly for git operations
- Issue is specifically with gh CLI organization authentication
- May need custom tooling to bridge this gap

## Test Cases for Solution
When fixed, the following should work seamlessly:
```bash
# Should create PR without manual intervention
gh pr create --base dev --head local-sam --title "Feature" --body "Description"

# Should list PRs
gh pr list

# Should merge PRs  
gh pr merge 123 --squash
```