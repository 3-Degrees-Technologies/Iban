# Centro Code Quality Checklist for AI Development

## Pre-Development Analysis (MANDATORY)

**BEFORE writing any code, run these commands to understand current quality state:**

```bash
# 1. Full build with analyzer feedback
dotnet build --verbosity normal

# 2. Count current warnings by type
dotnet build --no-restore 2>&1 | grep "warning" | cut -d: -f4 | sort | uniq -c | sort -nr

# 3. Check for security-specific warnings
dotnet build --no-restore 2>&1 | grep -E "CA3[0-9]{3}|S2068|S4423"

# 4. Auto-fix formatting (safe operation)
dotnet format

# 5. Verify tests still pass after formatting
dotnet test --no-build
```

## Code Quality Rules for Financial Systems

### Security (Zero Tolerance)
- **CA3001**: SQL injection vulnerabilities
- **CA3002**: XSS vulnerabilities  
- **CA3003**: File path injection vulnerabilities
- **CA3004**: Information disclosure vulnerabilities
- **CA3012**: Regex injection vulnerabilities
- **S2068**: Hardcoded credentials/passwords/API keys
- **S4423**: Weak SSL/TLS protocols
- **S5122**: Insecure CORS configuration

### Reliability (High Priority)
- **CA2007**: ConfigureAwait(false) - disabled for ASP.NET Core
- **CA2201**: Reserved exception types
- **CA2208**: ArgumentException instantiation
- **S112**: Generic exceptions should not be thrown
- **S1481**: Unused local variables
- **S1854**: Unused assignments

### Maintainability (Medium Priority)
- **SA1518**: File should end with newline
- **SA1028**: No trailing whitespace
- **SA1516**: Elements separated by blank lines
- **SA1413**: Trailing commas in multi-line initializers
- **S138**: Functions should not have too many lines
- **S1541**: Methods should not be too complex

## AI Development Workflow

### Step 1: Pre-Code Analysis
```bash
# Get baseline quality metrics
echo "=== Current Quality State ==="
dotnet build --no-restore 2>&1 | grep -c "warning"
echo "Total warnings before changes"

# Show security warnings specifically
echo "=== Security Issues ==="
dotnet build --no-restore 2>&1 | grep -E "CA3[0-9]{3}|S2068|S4423"
```

### Step 2: During Development
- **Fix warnings in files you modify** - don't ignore analyzer feedback
- **Use `dotnet format` frequently** - auto-fixes many style issues
- **Pay attention to security warnings** - financial systems require zero tolerance
- **Test after each change** - `dotnet test --no-build`

### Step 3: Pre-Commit Validation
```bash
# Final quality check
echo "=== Final Quality Check ==="
dotnet build --verbosity normal

# Count remaining warnings
dotnet build --no-restore 2>&1 | grep -c "warning"
echo "Total warnings after changes (should be same or fewer)"

# Ensure no new security issues
dotnet build --no-restore 2>&1 | grep -E "CA3[0-9]{3}|S2068|S4423" && echo "SECURITY ISSUES FOUND!" || echo "No security issues detected"

# Format and test
dotnet format --verify-no-changes
dotnet test --no-build
```

## Financial Code Standards

### Currency and Precision
- Use `decimal` for all financial calculations
- Specify culture for string formatting: `ToString("C", CultureInfo.InvariantCulture)`
- Validate currency codes against ISO 4217

### API Security
- Always validate tenant isolation via `X-Tenant-ID`
- Log all financial operations with correlation IDs
- Sanitize all inputs from external provider APIs
- Use structured logging for audit trails

### Error Handling
- No silent failures in financial operations
- Include correlation IDs in error responses
- Log errors at appropriate levels (Warning for business logic, Error for system failures)
- Return meaningful error codes to clients

## Common Analyzer Fixes

### StyleCop Quick Fixes
```csharp
// BAD: No trailing newline, missing blank lines
public class Balance {
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

// GOOD: Proper formatting
public class Balance
{
    public decimal Amount { get; set; }

    public string Currency { get; set; }
}
```

### Security Quick Fixes
```csharp
// BAD: Hardcoded credential
private const string ApiKey = "sk_live_abc123";

// GOOD: Configuration-based
private readonly string _apiKey = configuration["Wise:ApiKey"];

// BAD: SQL injection risk
var query = $"SELECT * FROM users WHERE id = {userId}";

// GOOD: Parameterized query
var query = "SELECT * FROM users WHERE id = @userId";
```

## Session Handoff Commands

**For new AI sessions, run these commands to understand quality state:**

```bash
# Quality overview
echo "=== Centro Code Quality Status ==="
dotnet build --no-restore 2>&1 | grep "warning" | wc -l
echo "Current warning count"

# Security scan
echo "=== Security Scan ==="
dotnet build --no-restore 2>&1 | grep -E "CA3[0-9]{3}|S2068|S4423" || echo "No security warnings found"

# Style issues count
echo "=== Style Issues ==="
dotnet build --no-restore 2>&1 | grep "SA1" | wc -l
echo "StyleCop warnings"

# Most common issues
echo "=== Top Warning Types ==="
dotnet build --no-restore 2>&1 | grep "warning" | cut -d: -f4 | cut -d' ' -f2 | sort | uniq -c | sort -nr | head -5
```

## Quality Gates for Centro

1. **Zero security warnings** (CA3xxx, S2068, S4423, S5122)
2. **Fix style warnings in modified files** (SA1xxx)
3. **No unused code in new implementations** (S1481, S1854)
4. **All tests pass** after quality fixes
5. **Files end with newlines** (SA1518)
6. **No trailing whitespace** (SA1028)

Remember: **Quality is not optional in financial systems.** The analyzers catch real issues that could affect security, reliability, and maintainability in a high-stakes fintech environment.