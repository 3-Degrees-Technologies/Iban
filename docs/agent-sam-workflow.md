# Centro Repository - AI Agent Workflow Documentation

## 🤖 Agent-Sam's Complete PR Workflow

### Overview
Agent-Sam creates feature branches, implements changes, and uses automated CI/CD feedback to iterate until code is ready for human review.

---

## 📋 Complete Workflow Steps

### 1. **Ticket Assignment & Setup**
```bash
# Agent-Sam receives ticket CEN-200
git checkout dev
git pull origin dev
git checkout -b feature/CEN-200
```

### 2. **Development & Local Testing**
```bash
# Implement feature
# Run local tests
dotnet test
dotnet build
# Verify changes work locally
```

### 3. **PR Creation**
```bash
# Push feature branch
git add .
git commit -m "CEN-200: Implement user authentication feature"
git push origin feature/CEN-200

# Create pull request
gh pr create \
  --title "CEN-200: Add user authentication" \
  --body "Implements JWT authentication with role-based access control" \
  --base dev \
  --head feature/CEN-200
```

### 4. **Automated CI Triggers**
- **Trigger**: PR creation automatically starts `ci.yml` workflow
- **Duration**: 2-5 minutes for complete feedback
- **Jobs**: 6 parallel jobs (code quality, tests, security, docker, AI review, summary)

### 5. **MCP Monitoring Loop**
```javascript
// Agent-Sam monitors via Cotejar MCP server
const checkPRStatus = async () => {
  const feedback = await mcp.call('get_pr_feedback', { 
    owner: 'your-org', 
    repo: 'Centro', 
    pr_number: 123 
  });
  
  return feedback;
};
```

### 6. **Feedback Interpretation**
```javascript
// Example feedback structure Agent-Sam receives:
{
  summary: { 
    totalIssues: 5, 
    blockingIssues: 2 
  },
  categories: {
    "security": [
      { 
        message: "Dependency vulnerability found in Newtonsoft.Json", 
        status: "failed",
        url: "https://github.com/advisories/GHSA-..." 
      }
    ],
    "quality": [
      { 
        message: "Unit test failed: UserAuthTest.ShouldAuthenticateValidUser", 
        status: "failed",
        file: "tests/UserAuthTest.cs:45" 
      }
    ],
    "style": [
      { 
        message: "Code formatting issues in UserController.cs", 
        status: "failed",
        tool: "dotnet format" 
      }
    ]
  },
  actions: {
    overall: "failure",  // pending | success | failure
    completedJobs: 4,    // Out of 6 total
    pendingJobs: 2,
    runs: [...]
  }
}
```

### 7. **Completion Detection Logic**
```javascript
const isPRReady = (feedback) => {
  // CI must be complete (not pending)
  const ciComplete = feedback.actions.overall !== 'pending';
  
  // No blocking issues preventing merge
  const noBlockingIssues = feedback.summary.blockingIssues === 0;
  
  // All CI checks passed
  const allChecksPassed = feedback.actions.overall === 'success';
  
  return ciComplete && noBlockingIssues && allChecksPassed;
};
```

### 8. **Fix & Iteration Cycle**
```bash
# Agent-Sam addresses feedback issues
# Example fixes based on feedback:

# Fix formatting
dotnet format

# Fix failing tests
# Edit tests/UserAuthTest.cs

# Update dependencies (if security issues)
# Edit Centro.csproj

# Commit and push updates
git add .
git commit -m "Fix: Address CI feedback - format code, fix tests, update dependencies"
git push origin feature/CEN-200
# This automatically triggers CI again (synchronize event)
```

### 9. **Monitoring Until Complete**
```javascript
// Agent-Sam polling logic
const waitForCICompletion = async (prNumber) => {
  while (true) {
    const feedback = await mcp.call('get_pr_feedback', { 
      owner: 'your-org', 
      repo: 'Centro', 
      pr_number: prNumber 
    });
    
    if (feedback.actions.overall === 'pending') {
      console.log('⏳ CI still running, waiting 30 seconds...');
      await sleep(30000);
      continue;
    }
    
    if (feedback.summary.blockingIssues > 0) {
      console.log('🔧 Issues to fix:', feedback.categories);
      return 'needs_fixes';
    }
    
    if (feedback.actions.overall === 'success') {
      console.log('✅ All checks passed! Ready for human review.');
      return 'ready_for_review';
    }
    
    // CI failed but may need manual review
    console.log('❌ CI failed with non-blocking issues');
    return 'manual_review_needed';
  }
};
```

---

## 🎯 Success Criteria

### **Agent-Sam knows the PR is ready when:**
1. **`feedback.actions.overall === 'success'`** - All CI jobs completed successfully
2. **`feedback.summary.blockingIssues === 0`** - No merge-preventing issues
3. **PR receives automated comment**: "🚀 Ready for human review!"

### **Human reviewer is notified when:**
- All automated checks pass
- AI code review is complete
- No security vulnerabilities
- All tests pass
- Code formatting is correct
- Docker build succeeds

---

## 🔄 Workflow States

| State | Description | Agent-Sam Action |
|-------|-------------|------------------|
| `pending` | CI jobs still running | Wait and poll MCP |
| `failure` with blocking issues | Must fix issues | Address feedback and push |
| `failure` with non-blocking | May need manual review | Evaluate and potentially fix |
| `success` | All checks passed | Notify human reviewer |

---

## 📊 MCP Server Tools Used

- **`get_pr_feedback`** - Complete feedback aggregation
- **`get_blocking_issues`** - Only merge-preventing issues  
- **`get_actions_status`** - GitHub Actions status
- **`get_failed_workflows`** - Specific failure details

---

## 🚨 Error Handling

### **Common Issues & Resolutions:**
- **Formatting failures**: Run `dotnet format`
- **Test failures**: Review test output, fix logic
- **Security vulnerabilities**: Update dependencies
- **Build failures**: Check compilation errors
- **Docker issues**: Verify Dockerfile syntax

### **When to Escalate:**
- Infrastructure failures (AWS permissions, etc.)
- Unclear CI feedback messages
- Repeated failures after multiple fix attempts

---

This workflow ensures Agent-Sam gets immediate, actionable feedback and iterates quickly until code meets all quality standards before human review.