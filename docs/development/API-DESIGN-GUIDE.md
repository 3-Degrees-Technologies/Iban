# Centro API Design Guide

## Purpose

This guide provides strategic principles for designing robust, maintainable APIs at Centro. It focuses on architectural decisions that prevent real-world problems, not basic REST conventions.

## Core Philosophy

**Design for failure, not success.** External providers fail, networks are unreliable, and clients have varying needs. Build APIs that gracefully handle the real world.

---

## Strategic Design Principles

### 1. One Endpoint, One Job

If your endpoint has more than 3-4 optional parameters, split it into focused endpoints.

**❌ Avoid:**
```
GET /quotes?from=USD&to=GBP&amount=100&speed=fast&provider=wise&includeHistory=true&includeFees=true
```

**✅ Prefer:**
```
GET /quotes                    # Core quote functionality
GET /quotes/123/fees          # Fee breakdown details
GET /quotes/123/history       # Historical rate data
```

**Why:** Single-purpose endpoints are easier to test, cache, document, and maintain.

### 2. Never Block on External Dependencies

Long-running or external operations must be asynchronous with status polling.

**❌ Avoid:**
```
POST /transfers  # Waits 30+ seconds for provider response
```

**✅ Prefer:**
```
POST /transfers     → 202 Accepted + Location: /transfers/123/status
GET /transfers/123/status → "processing" | "completed" | "failed"
```

**Why:** Synchronous calls to external providers will timeout and create poor user experience.

### 3. Design for Failure, Not Success

Assume external providers will fail. Design error responses that help clients recover.

**✅ Example:**
```json
{
  "error": "PROVIDER_UNAVAILABLE",
  "message": "Wise API temporarily unavailable",
  "retryAfter": 30,
  "alternatives": [
    {"provider": "nium", "estimatedTime": "2-3 minutes"}
  ],
  "traceId": "centro-req-abc123"
}
```

**Why:** In fintech, resilience and client recovery options are critical business features.

### 4. Avoid Chatty APIs - Batch Related Data

If clients always need related data, include it. Don't force multiple round trips.

**❌ Avoid:**
```
GET /orders/123        # Basic order
GET /customers/456     # Customer details
GET /orders/123/fees   # Fee breakdown
```

**✅ Prefer:**
```
GET /orders/123?include=customer,fees
```

**Why:** Network latency compounds with multiple calls, especially for mobile clients.

### 5. Make State Transitions Explicit

Don't hide business logic behind generic UPDATE endpoints.

**❌ Avoid:**
```
PUT /orders/123 {"status": "cancelled"}
```

**✅ Prefer:**
```
POST /orders/123/cancel
POST /orders/123/approve
POST /orders/123/execute
```

**Why:** Explicit operations make business logic clear, enable proper validation, and improve auditability.

### 6. Optimize for the 80% Use Case

Design primary endpoints for the most common client needs, not edge cases.

**✅ Example:**
```
GET /corridors/usd-gbp           # Optimized for common pairs
GET /corridors?from=USD&to=GBP   # Flexible for edge cases
```

**Why:** Common operations should be fast and simple; complexity should be opt-in.

### 7. Fail Fast with Validation

Validate everything possible synchronously before starting async processes.

**✅ Flow:**
```
POST /transfers
  → 400: "Invalid IBAN format" (immediate)
  → 202: Transfer accepted (after full validation)
```

**Why:** Catching errors early prevents wasted processing and improves client experience.

### 8. Don't Expose Internal Implementation

APIs should reflect business concepts, not database schema or internal services.

**❌ Avoid:**
```
GET /provider-rate-cache-entries
GET /customer-payment-method-associations
```

**✅ Prefer:**
```
GET /rates/live
GET /customers/123/payment-methods
```

**Why:** Business-focused APIs remain stable as internal implementation changes.

### 9. Version Breaking Changes, Extend Non-Breaking

Add optional fields freely. New required fields or removed fields need versioning.

**✅ Non-breaking:**
```json
{
  "orderId": 123,
  "amount": 1000,
  "estimatedArrival": "2024-01-16T10:00:00Z"  // New optional field
}
```

**❌ Breaking (needs v2):**
```json
{
  "orderId": 123,
  "amount": 1000,
  "compliance": { "required": true }  // New required field
  // "currency": "USD"  // Removed field
}
```

**Why:** Non-breaking changes allow gradual client adoption without forced upgrades.

### 10. Design for Monitoring and Debugging

Include correlation IDs, meaningful error codes, and enough context for troubleshooting.

**✅ Example:**
```json
{
  "traceId": "centro-req-abc123",
  "providerId": "wise-tx-xyz789",
  "corridor": "USD-GBP",
  "amount": 1000,
  "error": "RATE_EXPIRED",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Why:** Production debugging requires context; correlation IDs enable end-to-end tracing.

---

## Essential REST Conventions

### Resource Naming
- Use **nouns** for resources: `/customers`, `/orders`
- Use **plural** nouns for collections: `/orders` not `/order`
- Use **HTTP verbs** for actions: `GET`, `POST`, `PUT`, `DELETE`
- Keep URLs **hierarchical**: `/customers/123/orders/456`

### HTTP Status Codes
- **200**: Success with response body
- **201**: Created (include `Location` header)
- **204**: Success, no response body
- **400**: Client error (validation failure)
- **401**: Unauthorized (authentication required)
- **404**: Resource not found
- **500**: Server error

### Response Consistency
- Use **camelCase** for JSON properties
- Include **pagination** for collections: `limit`, `offset`, `total`
- Support **filtering** and **sorting**: `?status=active&sort=created_date`
- Provide **HATEOAS links** for related resources

---

## Decision Framework

Before implementing any endpoint, ask:

1. **Resilience**: Could this hang if a provider is slow?
2. **Efficiency**: Will clients need 3+ calls to complete their task?  
3. **Clarity**: Am I exposing business concepts or database tables?
4. **Debugging**: How will I troubleshoot this in production when it fails?
5. **Scope**: Does this endpoint try to do too many things?

---

## Centro-Specific Patterns

### Multi-tenant Isolation
```
X-Tenant-ID: threedegrees
X-API-Key: centro-api-key-123
```

### Correlation Tracking
```
X-Correlation-ID: centro-req-abc123
```

### Provider Integration
```json
{
  "result": "success",
  "providerId": "wise-tx-xyz789",
  "providerRef": "T12345",
  "centerMetadata": {
    "corridor": "USD-GBP",
    "route": "optimal"
  }
}
```

### Error Response Standard
```json
{
  "error": {
    "code": "INVALID_CURRENCY", 
    "message": "Currency 'XYZ' is not supported for this corridor",
    "traceId": "centro-req-abc123",
    "timestamp": "2024-01-15T10:30:00Z",
    "details": {
      "supportedCurrencies": ["USD", "GBP", "EUR"]
    }
  }
}
```

---

## Related Documentation

- [API Architecture](../architecture/api-structure.md) - Technical structure and patterns
- [Provider API Navigation](../knowledge/provider-api-navigation.md) - External integration patterns
- [Logging and Monitoring](../knowledge/logging-and-monitoring.md) - Observability standards

---

*This guide focuses on strategic decisions that prevent API design problems under real-world usage. For basic REST conventions, see the existing codebase patterns in `src/Centro.Api/Models/Common/`.*