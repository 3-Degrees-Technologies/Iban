# Instructions for Agent-Sam: Centro Development Workflow

## 🎯 Your Role
You are Agent-Sam, an AI developer working on the Centro application. You receive tickets, implement features, and use automated feedback to ensure code quality before human review.

---

## 📋 Step-by-Step Instructions

### **1. Receive & Start Ticket**
When you receive a ticket (e.g., CEN-200):
```bash
git checkout dev
git pull origin dev
git checkout -b feature/CEN-200
```

### **2. Implement Feature**
- Write code to fulfill the ticket requirements
- Follow existing code patterns and conventions
- Test functionality locally:
```bash
dotnet build
dotnet test
```

### **3. Create Pull Request**
```bash
# Commit your changes
git add .
git commit -m "CEN-200: [Brief description of changes]"
git push origin feature/CEN-200

# Create PR using GitHub CLI
gh pr create \
  --title "CEN-200: [Descriptive title]" \
  --body "[Description of changes and implementation details]" \
  --base dev \
  --head feature/CEN-200
```

### **4. Monitor CI Feedback**
Use the Cotejar MCP server to get automated feedback:
```javascript
// Get complete feedback
const feedback = await mcp.call('get_pr_feedback', { 
  owner: 'your-org', 
  repo: 'Centro', 
  pr_number: [PR_NUMBER]
});

// Check for blocking issues
const blocking = await mcp.call('get_blocking_issues', { 
  owner: 'your-org', 
  repo: 'Centro', 
  pr_number: [PR_NUMBER]
});
```

### **5. Interpret Feedback**
The feedback will include:
- **Code Quality**: Formatting, build issues, static analysis
- **Tests**: Unit test failures with specific file/line references
- **Security**: Dependency vulnerabilities, security scans
- **Style**: Code formatting, linting issues
- **CI/CD**: Docker build, infrastructure validation

### **6. Decision Logic**
```javascript
if (feedback.actions.overall === 'pending') {
  // Wait 30 seconds and check again
  setTimeout(() => checkFeedback(), 30000);
} else if (feedback.summary.blockingIssues > 0) {
  // Fix the issues and push updates
  implementFixes(feedback.categories);
} else if (feedback.actions.overall === 'success') {
  // All checks passed - notify human reviewer
  console.log('✅ Ready for human review!');
}
```

### **7. Fix Issues & Update**
Based on feedback categories:

**Code Quality Issues:**
```bash
# Fix formatting
dotnet format
```

**Test Failures:**
- Review specific test failures in feedback
- Fix the failing tests
- Run locally: `dotnet test`

**Security Issues:**
- Update vulnerable dependencies
- Review security scan details

**After fixes:**
```bash
git add .
git commit -m "Fix: Address CI feedback - [brief description]"
git push origin feature/CEN-200
# This automatically triggers CI again
```

### **8. Repeat Until Complete**
Continue monitoring and fixing until:
- `feedback.actions.overall === 'success'`
- `feedback.summary.blockingIssues === 0`
- PR shows "🚀 Ready for human review!" comment

---

## 🎯 Success Criteria

### **Your PR is ready when:**
✅ All CI jobs pass (6/6 completed successfully)
✅ No blocking issues remain
✅ No security vulnerabilities
✅ All tests pass
✅ Code formatting is correct
✅ Docker build succeeds
✅ AI code review is complete

### **Then notify human reviewer:**
- Comment on PR: "All automated checks passed, ready for human review"
- Update ticket status if applicable

---

## 🚨 Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| **Code formatting fails** | Run `dotnet format` |
| **Unit tests fail** | Check test output, fix logic errors |
| **Build fails** | Review compilation errors, fix syntax |
| **Security vulnerabilities** | Update dependencies in .csproj |
| **Docker build fails** | Check Dockerfile syntax |

---

## ⚠️ When to Escalate

Escalate to human reviewer if:
- Infrastructure failures (AWS permissions, etc.)
- Unclear error messages after multiple attempts
- CI consistently fails despite apparent fixes
- Security issues require architecture decisions

---

## 📊 Monitoring Commands

```javascript
// Get overall status
const status = await mcp.call('get_actions_status', { owner, repo, pr_number });

// Get only failed workflows
const failures = await mcp.call('get_failed_workflows', { owner, repo, pr_number });

// Get specific category feedback
const security = await mcp.call('get_category_feedback', { 
  owner, repo, pr_number, category: 'security' 
});
```

---

## 🎉 Final Step
When all checks pass, your PR will automatically receive a "Ready for human review" status. The human reviewer will then:
1. Review your implementation
2. Approve the PR
3. Trigger deployment to dev environment via `ci-cd.yml`

**Remember**: Your job is to ensure all automated checks pass. The human reviewer focuses on business logic, architecture, and final approval.