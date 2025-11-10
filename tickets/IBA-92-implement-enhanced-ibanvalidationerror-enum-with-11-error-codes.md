# Implement Enhanced IbanValidationError Enum with 11 Error Codes

**Objective**: Create comprehensive IbanValidationError enum based on IBA-89 design specification with 11 standardized validation error codes.

## Context

This implements the enhanced error reporting from IBA-89's design. The error codes provide detailed, actionable feedback for each validation failure scenario.

## Acceptance Criteria

* Create IbanValidationError enum with 11 error codes
* Organize errors into logical categories (Format, Account, Input)
* Add XML documentation for each error code
* Ensure error codes are serialization-friendly (JSON/XML)
* Ready for integration in IBA-93

## Implementation Guidance

**Error Code Structure (from IBA-89):**

```csharp
public enum IbanValidationError
{
    // Format Validation Errors
    ERR_FORMAT_INVALID = 1001,        // Basic format violation
    ERR_FORMAT_LENGTH = 1002,          // Incorrect length for country
    ERR_FORMAT_CHECKSUM = 1003,        // MOD-97 checksum failed
    ERR_FORMAT_COUNTRY_INVALID = 1004, // Unknown country code
    
    // Account-Level Validation Errors
    ERR_ACCOUNT_MODULUS = 2001,        // UK modulus check failed
    ERR_ACCOUNT_BBAN = 2002,           // BBAN validation failed
    ERR_ACCOUNT_SORTCODE = 2003,       // Invalid UK sort code
    ERR_ACCOUNT_NUMBER = 2004,         // Invalid account number
    
    // Input Validation Errors
    ERR_INPUT_NULL = 3001,             // Null input
    ERR_INPUT_EMPTY = 3002,            // Empty string
    ERR_INPUT_WHITESPACE = 3003        // Whitespace-only
}
```

**Error Code Ranges:**

* 1000-1999: Format validation errors
* 2000-2999: Account-level validation errors
* 3000-3999: Input validation errors

**XML Documentation Requirements:**
Each error code should have:

* Clear description of what failed
* User-facing error message guidance
* Example scenarios triggering the error

## Definition of Done

* IbanValidationError enum created with all 11 codes
* XML documentation complete for each code
* Error codes organized by category (Format, Account, Input)
* Code compiles without warnings
* Ready for service integration (IBA-93)

**Task**: Create comprehensive error code enum based on IBA-89 design specification with 11 standardized validation error codes.

**Implementation Approach**: Use test-driven development approach following existing Centro patterns.

**Definition of Done**:

* Implementation completed using test-driven development
* Core functionality implemented
* Code review completed and approved

**Testing Approach**:

* Use Test-Driven Development (TDD) - unit tests are written AS PART of implementation
* DO NOT create separate integration test suites or comprehensive integration testing
* Integration tests are ONLY for Bruno API test tickets or specific test harness creation
* All testing should be integrated into the implementation process using TDD methodology

**Important Note for Implementation**
If any requirements are unclear or you need additional context, please ask clarifying questions rather than making assumptions. It's better to get confirmation on approach, scope, or technical details before implementation.

**Infrastructure Notice**
DO NOT attempt to access or modify databases directly. DO NOT attempt to change LocalStack configuration (centro-localstack:4566). For all database and infrastructure questions, contact Agent Black.

**Labels**
backend
