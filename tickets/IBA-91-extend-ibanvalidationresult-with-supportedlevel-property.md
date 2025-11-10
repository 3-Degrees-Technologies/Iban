# Extend IbanValidationResult with SupportedLevel Property

**Objective**: Extend the existing IbanValidationResult class with a new SupportedLevel property that distinguishes between the validation level performed versus what is available for a given country.

## Context

This implements part of IBA-89's enhanced result structure design by adding validation level tracking.

## Acceptance Criteria

* Add ValidationLevel enum (Structural, AccountLevel, NotValidated)
* Add SupportedLevel property to IbanValidationResult
* Add PerformedLevel property to IbanValidationResult (what was actually validated)
* Update Success/Failure factory methods to accept validation level parameters
* Maintain backward compatibility with existing result consumers
* Add XML documentation for new properties

## Implementation Guidance

**ValidationLevel Enum:**

```csharp
public enum ValidationLevel
{
    NotValidated = 0,  // No validation performed
    Structural = 1,     // ISO 13616 format, checksum, length
    AccountLevel = 2    // Bank account validation (UK modulus, BBAN)
}
```

**IbanValidationResult Updates:**
Add two new properties:

* `ValidationLevel SupportedLevel` - What level is available for this country
* `ValidationLevel PerformedLevel` - What level was actually performed

**Factory Method Updates:**

* `Success(string countryCode, ValidationLevel performed, ValidationLevel supported)`
* `Failure(IbanValidationError error, string message, ValidationLevel supported)`

## Definition of Done

* ValidationLevel enum created
* IbanValidationResult extended with new properties
* Factory methods updated
* XML documentation complete
* Code compiles without warnings
* Ready for service integration (IBA-93)

**Task**: Extend the existing IbanValidationResult class with a new SupportedLevel property that distinguishes between the validation level performed versus what is available for a given country.

## Context

This implements part of IBA-89's enhanced result structure design by adding validation level tracking.

## Acceptance Criteria

* Add ValidationLevel enum (Structural, AccountLevel, NotValidated)
* Add SupportedLevel property to IbanValidationResult
* Add PerformedLevel property to IbanValidationResult (what was actually validated)
* Update Success/Failure factory methods to accept validation level parameters
* Maintain backward compatibility with existing result consumers
* Add XML documentation for new properties

## Implementation Guidance

**ValidationLevel Enum:**

```csharp
public enum ValidationLevel
{
    NotValidated = 0,  // No validation performed
    Structural = 1,     // ISO 13616 format, checksum, length
    AccountLevel = 2    // Bank account validation (UK modulus, BBAN)
}
```

**IbanValidationResult Updates:**
Add two new properties:

* `ValidationLevel SupportedLevel` - What level is available for this country
* `ValidationLevel PerformedLevel` - What level was actually performed

**Factory Method Updates:**

* `Success(string countryCode, ValidationLevel performed, ValidationLevel supported)`
* `Failure(IbanValidationError error, string message, ValidationLevel supported)`

## Definition of Done

* ValidationLevel enum created
* IbanValidationResult extended with new properties
* Factory methods updated
* XML documentation complete
* Code compiles without warnings
* Ready for service integration (IBA-93)

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
