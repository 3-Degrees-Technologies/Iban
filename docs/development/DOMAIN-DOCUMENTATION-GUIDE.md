# Domain Documentation Guide

## Purpose

This guide defines when and how to create README.md files for business domains and technical boundaries in Centro's codebase. It implements **Domain-Driven Documentation** - placing knowledge anchors at the right level of abstraction.

## Philosophy

Documentation should follow Domain-Driven Design principles:
- **Business domains deserve special attention** (like DDD aggregates)
- **Provider boundaries matter** (like DDD integration contexts)  
- **Not every folder needs docs** (just like not every class is a domain object)
- **Knowledge lives with the code** it describes

---

## Decision Tree: When to Create README.md

### ✅ CREATE README.md for:

**Core Business Domains**
```
Centro.Core/Quotes/README.md           ✅ Quote pricing and routing logic
Centro.Core/Transfers/README.md        ✅ Transfer lifecycle management  
Centro.Core/Corridors/README.md        ✅ Currency pair optimization
Centro.Core/Compliance/README.md       ✅ Regulatory and audit logic
```

**Provider Implementations**
```
Centro.Providers.Wise.Quotes/README.md      ✅ Wise-specific quote integration
Centro.Providers.Nium.Transfers/README.md   ✅ Nium transfer API peculiarities
Centro.Providers.CurrencyCloud/README.md    ✅ CurrencyCloud implementation details
```

**Significant Technical Boundaries**
```
Centro.Api.Authentication/README.md     ✅ Multi-tenant auth strategy
Centro.Infrastructure.Metrics/README.md ✅ CloudWatch integration patterns
Centro.Infrastructure.Providers/README.md ✅ Provider abstraction layer
```

### ❌ DON'T CREATE README.md for:

**Simple Structural Folders**
```
Models/                    ❌ Just DTOs and entities
Controllers/               ❌ Basic API endpoints
Services/                  ❌ Simple service classes
Extensions/                ❌ Utility methods
Common/                    ❌ Shared utilities
Configuration/             ❌ Settings classes
```

**Basic Implementation Details**
```
Centro.Core.Quotes/Models/      ❌ Just data structures
Centro.Core.Quotes/Events/      ❌ Just event classes
Centro.Api/Middleware/          ❌ Standard middleware
Centro.Infrastructure/Health/   ❌ Basic health checks
```

---

## Agent Decision Rule

**Before creating any README.md, ask:**

*"If a new developer needs to modify this code, is there business domain knowledge or integration complexity that isn't obvious from reading the code itself?"*

- **Business Logic**: Quote pricing algorithms, transfer validation rules, compliance requirements
- **Provider Quirks**: Wise API timeouts, Nium currency restrictions, CurrencyCloud rate limits
- **Domain Rules**: Cross-border regulations, settlement windows, risk thresholds

**If yes** → Create README.md  
**If no** → Skip it

---

## README.md Content Template

### Required Sections

#### 1. Business Context (2-3 sentences)
```markdown
## Overview
This domain handles quote pricing and provider selection for Centro's corridor optimization engine. It implements the core "unbundling" strategy by analyzing multiple provider rates to find the optimal route for each currency pair and amount.
```

#### 2. Domain Objects (bullet list)
```markdown
## Domain Objects
- **Quote**: Represents a price quote from a provider with rate, fees, and execution timeline
- **Corridor**: A specific currency pair route (USD→GBP) with associated providers and constraints  
- **Route**: The selected optimal path through providers for a given corridor and amount
- **QuoteComparison**: Analysis engine that evaluates multiple provider quotes against business rules
```

#### 3. Business Rules (key validation and logic)
```markdown
## Business Rules
- Quotes expire after 30 seconds for volatile corridors (USD→EUR), 2 minutes for stable pairs
- Minimum transfer amounts vary by provider: Wise $1, Nium $10, CurrencyCloud $100
- Rate comparisons must account for both fees and FX spread to determine true cost
- Compliance checks run synchronously for amounts under $10k, async for larger transfers
```

#### 4. Provider Integration (if applicable)
```markdown
## Provider Integration
### Wise API
- Uses real-time rate endpoints with 30-second quotes
- Requires pre-funded balances for instant execution
- Supports 50+ currencies with transparent fee structure

### Key Differences
- **Wise**: Real-time rates, pre-funding required, transparent fees
- **Nium**: Quoted rates valid 2 minutes, credit-based, includes margin in rate
- **CurrencyCloud**: Tiered pricing, batch settlements, complex fee structure
```

#### 5. Testing Strategy (1-2 sentences)
```markdown
## Testing
- **Unit Tests**: 45 test cases covering rate calculations, fee computations, and quote expiry logic
- **Integration Tests**: Provider API contract validation with realistic response scenarios
- **Business Scenarios**: Cross-provider rate comparison, corridor optimization, compliance edge cases
```

#### 6. Future Considerations (optional)
```markdown
## Future Considerations
- **New Provider Integration**: Adapter pattern supports adding Remitly, Western Union APIs
- **Real-time Rates**: WebSocket support planned for high-frequency trading corridors
- **ML Optimization**: Rate prediction models to improve corridor selection accuracy
```

---

## Example Trigger Phrases

**✅ Indicates README needed:**
- "This handles quote pricing logic"
- "This contains the Wise API adapter"  
- "This validates IBAN formats according to SEPA rules"
- "This implements corridor optimization algorithms"
- "This manages multi-tenant authentication"

**❌ Indicates no README needed:**
- "This is just response DTOs"
- "This contains basic extension methods"
- "This is standard middleware configuration"
- "This has simple CRUD operations"

---

## Maintenance Guidelines

### When to Update README.md

**Always update when:**
- Adding new business rules or validation logic
- Integrating with a new provider
- Changing core domain concepts or terminology
- Adding significant new test scenarios

**Consider updating when:**
- Refactoring implementation (if business logic changes)
- Performance optimizations that affect behavior
- Bug fixes that reveal missing business context

### Quality Check

A good domain README should:
- Be understandable by a business analyst
- Help a new developer make changes confidently  
- Explain the "why" behind complex business logic
- Provide context that isn't obvious from code

---

## Related Documentation

- [API Design Guide](API-DESIGN-GUIDE.md) - Endpoint design principles
- [Provider API Navigation](../knowledge/provider-api-navigation.md) - External integration patterns
- [Agent Instructions](../../AGENTS.md) - AI agent development guidelines

---

*This guide implements Domain-Driven Documentation principles, ensuring knowledge anchors exist at business domain boundaries where understanding matters most.*