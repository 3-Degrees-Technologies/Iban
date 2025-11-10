# IBAN Validation Result Structure - Architecture Specification

**Ticket:** IBA-89  
**Author:** Agent-Blue  
**Date:** 2025-11-10  
**Status:** Architecture Design (No Implementation)

---

## Executive Summary

This document specifies the unified result structure for IBAN validation that handles both structural validation (ISO 13616) and account-level validation (UK modulus checking + BBAN validation). The design extends the existing `ValidationResult` implementation with minimal changes, following the principle of simplicity from the development guide.

**Decision:** **Option A - Unified Result Class (RECOMMENDED)** with minor enhancements to the existing implementation.

---

## Table of Contents

1. [Context & Requirements](#context--requirements)
2. [Current Implementation Analysis](#current-implementation-analysis)
3. [Architecture Decision](#architecture-decision)
4. [Enhanced Result Structure](#enhanced-result-structure)
5. [Complete Error Code Schema](#complete-error-code-schema)
6. [Integration Specifications](#integration-specifications)
7. [Usage Examples](#usage-examples)
8. [Testing Strategy](#testing-strategy)
9. [Backward Compatibility](#backward-compatibility)
10. [Implementation Roadmap](#implementation-roadmap)

---

## Context & Requirements

### Background

- **IBA-85**: Implemented structural IBAN validation using IbanNet library
  - Format validation (country code, length, characters)
  - MOD-97-10 checksum validation (ISO/IEC 7064:2003)
  
- **IBA-87**: Implemented account-level validation
  - UK: ModulusChecker.NetCore (Modulus-11/10 checking)
  - 9 Countries: IbanNet.Extensions.Bban (FR, IT, ES, PT, NO + 4 more)
  - Graceful fallback to structural validation for unsupported countries

### Validation Levels

**Structural Validation** (ISO 13616:2020):
- Country code validity (ISO 3166-1 alpha-2)
- MOD-97-10 checksum verification
- Length validation per country specification
- Character structure validation

**Account-Level Validation** (Enhanced):
- **UK (GB)**: Modulus-11/10 checking via ModulusChecker.NetCore
- **9 Countries**: BBAN validation via IbanNet.Extensions.Bban
  - France (FR), Italy (IT), Spain (ES), Portugal (PT), Norway (NO)
  - Plus 4 additional countries supported by IbanNet.Extensions.Bban
- **Other Countries**: Graceful fallback to structural validation only

### Design Goals

1. **Clarity**: API consumers understand what validation was performed and why it failed
2. **Actionability**: Error codes and messages enable proper error handling
3. **Performance**: Use value types (readonly record struct) for efficiency
4. **Simplicity**: Minimal changes to existing implementation
5. **Extensibility**: Support future validation levels without breaking changes

---

## Current Implementation Analysis

### Existing ValidationResult Structure

Location: `src/Iban.Core/ValidationResult.cs`

```csharp
public readonly record struct ValidationResult
{
    public required bool IsValid { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Country { get; init; }
    public ValidationLevel Level { get; init; }
    
    public static ValidationResult Success(string? country = null, 
        ValidationLevel level = ValidationLevel.Structural);
    
    public static ValidationResult Failed(string errorCode, string errorMessage, 
        string? country = null, ValidationLevel level = ValidationLevel.Structural);
}

public enum ValidationLevel
{
    Structural,
    AccountLevel
}
```

### Strengths

✅ **Already uses readonly record struct** - Optimal performance (value type)  
✅ **Factory methods** - Clean API for creating results  
✅ **ErrorCode + ErrorMessage** - Supports both machine and human-readable errors  
✅ **ValidationLevel** - Distinguishes between validation types  
✅ **Country property** - Captures country code when parseable

### Gaps

❌ **No SupportedValidationLevel** - Can't distinguish "validation performed" from "validation available"  
❌ **Error codes not standardized** - Need consistent schema (ERR_FORMAT_*, ERR_ACCOUNT_*, ERR_INPUT_*)  
❌ **No metadata for account validation capabilities** - API consumers can't know what validation is available for a country

---

## Architecture Decision

### Option A: Unified Result Class (RECOMMENDED) ✅

**Rationale:**
- ✅ Extends existing implementation with minimal changes
- ✅ Follows development guide: "solve your issue in the simplest possible way" (dev.md)
- ✅ Maintains existing API surface (backward compatible)
- ✅ Uses value type (readonly record struct) for performance
- ✅ Simple, clear API - easy to consume

**Changes Required:**
1. Add `SupportedValidationLevel` property (optional)
2. Standardize error codes with ERR_* schema
3. Add static error code constants
4. Update factory methods to use standardized error codes

### Option B: Result Pattern with Success/Failure Types ❌

**Rejected Because:**
- ❌ Adds complexity without clear benefit
- ❌ Requires discriminated union pattern (not idiomatic in C#)
- ❌ Forces API consumers to handle pattern matching
- ❌ Breaking change to existing API

### Option C: Builder Pattern ❌

**Rejected Because:**
- ❌ Overkill for current needs
- ❌ Adds verbosity without value
- ❌ Against development guide principle of simplicity

---

## Enhanced Result Structure

### Proposed Changes to ValidationResult

```csharp
namespace Iban.Core;

/// <summary>
/// Represents the result of an IBAN validation operation.
/// </summary>
public readonly record struct ValidationResult
{
    /// <summary>
    /// Gets whether the IBAN is valid.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Gets the error code if validation failed; otherwise, null.
    /// Error codes follow the schema: ERR_[CATEGORY]_[SPECIFIC]
    /// - ERR_FORMAT_* : Structural validation errors (format, checksum, length)
    /// - ERR_ACCOUNT_* : Account-level validation errors (modulus, BBAN)
    /// - ERR_INPUT_* : Input processing errors (null, unparseable)
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets the error message if validation failed; otherwise, null.
    /// Provides human-readable description suitable for UI display.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the two-letter ISO country code if the IBAN was structurally 
    /// valid enough to parse; otherwise, null.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Gets the level of validation that was actually performed.
    /// </summary>
    public ValidationLevel Level { get; init; }

    /// <summary>
    /// Gets the highest level of validation supported for this IBAN's country.
    /// If null, the country could not be determined from the input.
    /// </summary>
    /// <remarks>
    /// This helps API consumers understand validation capabilities:
    /// - If Level == SupportedLevel: Full validation was performed
    /// - If Level &lt; SupportedLevel: Validation stopped early due to errors
    /// - If SupportedLevel == Structural: Country has no account-level validation
    /// </remarks>
    public ValidationLevel? SupportedLevel { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="country">The two-letter ISO country code.</param>
    /// <param name="level">The validation level performed.</param>
    /// <param name="supportedLevel">The highest validation level supported for this country.</param>
    public static ValidationResult Success(
        string? country = null, 
        ValidationLevel level = ValidationLevel.Structural,
        ValidationLevel? supportedLevel = null) => new()
    {
        IsValid = true,
        Country = country,
        Level = level,
        SupportedLevel = supportedLevel ?? level
    };

    /// <summary>
    /// Creates a failed validation result with an error code and message.
    /// </summary>
    /// <param name="errorCode">The structured error code (ERR_FORMAT_*, ERR_ACCOUNT_*, ERR_INPUT_*).</param>
    /// <param name="errorMessage">The human-readable error message.</param>
    /// <param name="country">The two-letter ISO country code if parseable.</param>
    /// <param name="level">The validation level at which the failure occurred.</param>
    /// <param name="supportedLevel">The highest validation level supported for this country.</param>
    public static ValidationResult Failed(
        string errorCode, 
        string errorMessage, 
        string? country = null, 
        ValidationLevel level = ValidationLevel.Structural,
        ValidationLevel? supportedLevel = null) => new()
    {
        IsValid = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        Country = country,
        Level = level,
        SupportedLevel = supportedLevel
    };
}

/// <summary>
/// Indicates the level of validation performed or supported for an IBAN.
/// </summary>
public enum ValidationLevel
{
    /// <summary>
    /// No validation was performed (e.g., due to null/empty input).
    /// </summary>
    NotValidated = 0,

    /// <summary>
    /// Basic structural validation only (format, length, check digits per ISO 13616).
    /// All IBANs support this level.
    /// </summary>
    Structural = 1,

    /// <summary>
    /// Account-level validation performed (modulus checking, BBAN validation).
    /// Only supported for specific countries (UK + 9 BBAN countries).
    /// </summary>
    AccountLevel = 2
}
```

### Design Rationale

**New Property: `SupportedLevel`**
- **Why**: Distinguishes "what validation happened" from "what validation is available"
- **Example**: For Kazakhstan (KZ), `Level = Structural` and `SupportedLevel = Structural` means full validation was done
- **Example**: For UK (GB), `Level = Structural` and `SupportedLevel = AccountLevel` means validation stopped early (e.g., bad checksum)

**New Enum Value: `NotValidated`**
- **Why**: Clearly indicates no validation occurred (null/empty input)
- **Current Behavior**: Uses `Structural` even for null input (misleading)
- **Better**: `Level = NotValidated` for input errors

**Factory Method Updates**
- **Why**: Support new `SupportedLevel` parameter
- **Default**: `supportedLevel` defaults to `level` (backward compatible)

---

## Complete Error Code Schema

### Error Code Format

**Schema**: `ERR_[CATEGORY]_[SPECIFIC]`

**Categories**:
- `FORMAT`: Structural validation errors (ISO 13616 compliance)
- `ACCOUNT`: Account-level validation errors (modulus/BBAN)
- `INPUT`: Input processing errors

### Format Errors (ERR_FORMAT_*)

Structural validation failures per ISO 13616:2020.

| Error Code | Description | Example IBAN | Message Template |
|------------|-------------|--------------|------------------|
| `ERR_FORMAT_INVALID_COUNTRY` | Country code not in ISO 3166-1 alpha-2 | `XX12ABCD...` | "Invalid country code '{0}'. Must be a valid ISO 3166-1 alpha-2 code." |
| `ERR_FORMAT_INVALID_LENGTH` | Length doesn't match country specification | `GB82WEST` (too short) | "Invalid IBAN length for country {0}. Expected {1} characters, got {2}." |
| `ERR_FORMAT_INVALID_CHECKSUM` | MOD-97-10 checksum verification failed | `NL00ABNA0417164300` | "Invalid IBAN checksum. The check digits do not validate against MOD-97-10 algorithm." |
| `ERR_FORMAT_INVALID_STRUCTURE` | Contains invalid characters or format | `GB82-WEST-1234...` | "Invalid IBAN structure. Contains invalid characters or does not match expected format." |

### Account Errors (ERR_ACCOUNT_*)

Account-level validation failures (modulus checking, BBAN validation).

| Error Code | Description | Applies To | Message Template |
|------------|-------------|------------|------------------|
| `ERR_ACCOUNT_MODULUS_CHECK_FAILED` | UK modulus-11/10 check failed | GB (UK) | "UK bank account modulus check failed. The sort code and account number combination is invalid." |
| `ERR_ACCOUNT_INVALID_BBAN` | BBAN validation failed | FR, IT, ES, PT, NO + 4 more | "BBAN validation failed for country {0}. The bank account number structure is invalid." |
| `ERR_ACCOUNT_INVALID_BANK_CODE` | Bank code not recognized/invalid | BBAN countries | "Invalid bank code for country {0}. The bank identifier is not recognized." |
| `ERR_ACCOUNT_INVALID_STRUCTURE` | Account number structure invalid | BBAN countries | "Invalid account number structure for country {0}. Does not match expected format." |

### Input Errors (ERR_INPUT_*)

Input processing and normalization failures.

| Error Code | Description | Example Input | Message Template |
|------------|-------------|---------------|------------------|
| `ERR_INPUT_NULL_OR_EMPTY` | Input is null or empty | `null`, `""`, `"   "` | "IBAN input is required. Cannot be null or empty." |
| `ERR_INPUT_UNPARSEABLE` | Cannot parse as IBAN format | `"12345"`, `"ABC"` | "Input cannot be parsed as a valid IBAN format." |
| `ERR_INPUT_NORMALIZATION_FAILED` | Normalization process failed | (rare edge cases) | "Failed to normalize IBAN input. Contains unsupported characters or format." |

### Static Error Code Constants

```csharp
namespace Iban.Core;

/// <summary>
/// Standard error codes for IBAN validation failures.
/// </summary>
public static class ValidationErrorCodes
{
    // Format Errors (Structural Validation)
    public const string InvalidCountry = "ERR_FORMAT_INVALID_COUNTRY";
    public const string InvalidLength = "ERR_FORMAT_INVALID_LENGTH";
    public const string InvalidChecksum = "ERR_FORMAT_INVALID_CHECKSUM";
    public const string InvalidStructure = "ERR_FORMAT_INVALID_STRUCTURE";

    // Account Errors (Account-Level Validation)
    public const string ModulusCheckFailed = "ERR_ACCOUNT_MODULUS_CHECK_FAILED";
    public const string InvalidBban = "ERR_ACCOUNT_INVALID_BBAN";
    public const string InvalidBankCode = "ERR_ACCOUNT_INVALID_BANK_CODE";
    public const string InvalidAccountStructure = "ERR_ACCOUNT_INVALID_STRUCTURE";

    // Input Errors (Input Processing)
    public const string NullOrEmpty = "ERR_INPUT_NULL_OR_EMPTY";
    public const string Unparseable = "ERR_INPUT_UNPARSEABLE";
    public const string NormalizationFailed = "ERR_INPUT_NORMALIZATION_FAILED";
}
```

---

## Integration Specifications

### Changes to IIbanValidationService

**No interface changes required** - existing methods continue to work with enhanced `ValidationResult`.

```csharp
public interface IIbanValidationService
{
    // Existing methods - no signature changes
    ValidationResult Validate(string? iban);
    bool IsValid(string? iban);
    bool TryParse(string? iban, out ParsedIban? parsedIban);
    ValidationResult ValidateWithAccountCheck(string? iban);
}
```

### Implementation Changes

**IbanValidationService.cs** - Update to use standardized error codes:

```csharp
public ValidationResult Validate(string? iban)
{
    // Input validation
    if (string.IsNullOrWhiteSpace(iban))
    {
        return ValidationResult.Failed(
            ValidationErrorCodes.NullOrEmpty,
            "IBAN input is required. Cannot be null or empty.",
            level: ValidationLevel.NotValidated
        );
    }

    // Normalization
    var normalized = _normalizer.Normalize(iban);
    
    // Parsing
    var parseResult = IbanParser.Parse(normalized);
    if (!parseResult.IsValid)
    {
        return ValidationResult.Failed(
            ValidationErrorCodes.Unparseable,
            "Input cannot be parsed as a valid IBAN format.",
            level: ValidationLevel.NotValidated
        );
    }

    // Extract country
    string country = normalized.Substring(0, 2);

    // Structural validation via IbanNet
    var validationResult = _validator.Validate(normalized);
    if (!validationResult.IsValid)
    {
        string errorCode = MapIbanNetError(validationResult.Error);
        return ValidationResult.Failed(
            errorCode,
            validationResult.Error.ErrorMessage,
            country: country,
            level: ValidationLevel.Structural,
            supportedLevel: GetSupportedLevel(country)
        );
    }

    // Success
    return ValidationResult.Success(
        country: country,
        level: ValidationLevel.Structural,
        supportedLevel: GetSupportedLevel(country)
    );
}

public ValidationResult ValidateWithAccountCheck(string? iban)
{
    // First perform structural validation
    var structuralResult = Validate(iban);
    if (!structuralResult.IsValid)
    {
        return structuralResult; // Return early with error
    }

    var country = structuralResult.Country!;
    var normalized = _normalizer.Normalize(iban!);

    // UK modulus checking
    if (country == "GB")
    {
        var modulusResult = _modulusChecker.CheckBankAccount(normalized);
        if (!modulusResult.IsValid)
        {
            return ValidationResult.Failed(
                ValidationErrorCodes.ModulusCheckFailed,
                "UK bank account modulus check failed. The sort code and account number combination is invalid.",
                country: country,
                level: ValidationLevel.AccountLevel,
                supportedLevel: ValidationLevel.AccountLevel
            );
        }

        return ValidationResult.Success(
            country: country,
            level: ValidationLevel.AccountLevel,
            supportedLevel: ValidationLevel.AccountLevel
        );
    }

    // BBAN validation for supported countries
    if (IsBbanSupported(country))
    {
        var bbanResult = _bbanValidator.Validate(normalized);
        if (!bbanResult.IsValid)
        {
            return ValidationResult.Failed(
                ValidationErrorCodes.InvalidBban,
                $"BBAN validation failed for country {country}. The bank account number structure is invalid.",
                country: country,
                level: ValidationLevel.AccountLevel,
                supportedLevel: ValidationLevel.AccountLevel
            );
        }

        return ValidationResult.Success(
            country: country,
            level: ValidationLevel.AccountLevel,
            supportedLevel: ValidationLevel.AccountLevel
        );
    }

    // Fallback: Structural validation only (no account-level support)
    return ValidationResult.Success(
        country: country,
        level: ValidationLevel.Structural,
        supportedLevel: ValidationLevel.Structural
    );
}

private ValidationLevel GetSupportedLevel(string country)
{
    // UK supports modulus checking
    if (country == "GB") return ValidationLevel.AccountLevel;

    // 9 countries support BBAN validation
    if (IsBbanSupported(country)) return ValidationLevel.AccountLevel;

    // All others: structural only
    return ValidationLevel.Structural;
}

private bool IsBbanSupported(string country)
{
    // IbanNet.Extensions.Bban supported countries
    return country is "FR" or "IT" or "ES" or "PT" or "NO"
        or "FI" or "BE" or "EE" or "LT"; // Example - verify actual list
}

private string MapIbanNetError(IbanNetError error)
{
    return error.Code switch
    {
        "InvalidCountryCode" => ValidationErrorCodes.InvalidCountry,
        "InvalidLength" => ValidationErrorCodes.InvalidLength,
        "InvalidChecksum" => ValidationErrorCodes.InvalidChecksum,
        _ => ValidationErrorCodes.InvalidStructure
    };
}
```

---

## Usage Examples

### Example 1: Successful Structural Validation (No Account-Level Support)

```csharp
var service = new IbanValidationService();
var result = service.Validate("KZ86125KZT5004100100");

// result.IsValid = true
// result.Country = "KZ"
// result.Level = ValidationLevel.Structural
// result.SupportedLevel = ValidationLevel.Structural
// result.ErrorCode = null
// result.ErrorMessage = null
```

**Interpretation**: Kazakhstan IBAN is valid. Only structural validation is available for KZ.

### Example 2: Successful Account-Level Validation (UK)

```csharp
var service = new IbanValidationService();
var result = service.ValidateWithAccountCheck("GB82WEST12345698765432");

// result.IsValid = true
// result.Country = "GB"
// result.Level = ValidationLevel.AccountLevel
// result.SupportedLevel = ValidationLevel.AccountLevel
// result.ErrorCode = null
// result.ErrorMessage = null
```

**Interpretation**: UK IBAN passed both structural and modulus checking. Full validation performed.

### Example 3: Checksum Failure (Validation Stopped Early)

```csharp
var service = new IbanValidationService();
var result = service.ValidateWithAccountCheck("GB00WEST12345698765432");

// result.IsValid = false
// result.Country = "GB"
// result.Level = ValidationLevel.Structural
// result.SupportedLevel = ValidationLevel.AccountLevel
// result.ErrorCode = "ERR_FORMAT_INVALID_CHECKSUM"
// result.ErrorMessage = "Invalid IBAN checksum..."
```

**Interpretation**: UK IBAN failed checksum validation. Account-level validation was not performed because structural validation failed first.

### Example 4: UK Modulus Check Failure

```csharp
var service = new IbanValidationService();
var result = service.ValidateWithAccountCheck("GB82ABCD99999912345678");

// result.IsValid = false
// result.Country = "GB"
// result.Level = ValidationLevel.AccountLevel
// result.SupportedLevel = ValidationLevel.AccountLevel
// result.ErrorCode = "ERR_ACCOUNT_MODULUS_CHECK_FAILED"
// result.ErrorMessage = "UK bank account modulus check failed..."
```

**Interpretation**: UK IBAN passed structural validation but failed modulus checking.

### Example 5: Null Input

```csharp
var service = new IbanValidationService();
var result = service.Validate(null);

// result.IsValid = false
// result.Country = null
// result.Level = ValidationLevel.NotValidated
// result.SupportedLevel = null
// result.ErrorCode = "ERR_INPUT_NULL_OR_EMPTY"
// result.ErrorMessage = "IBAN input is required..."
```

**Interpretation**: No validation was performed due to null input.

### Example 6: API Client Error Handling

```csharp
var result = service.Validate(userInput);

if (!result.IsValid)
{
    switch (result.ErrorCode)
    {
        case ValidationErrorCodes.NullOrEmpty:
            return BadRequest("Please provide an IBAN.");
        
        case ValidationErrorCodes.InvalidChecksum:
            return BadRequest("Invalid IBAN: Checksum verification failed.");
        
        case ValidationErrorCodes.ModulusCheckFailed:
            return BadRequest("Invalid UK bank account details.");
        
        default:
            return BadRequest($"Invalid IBAN: {result.ErrorMessage}");
    }
}

// Proceed with valid IBAN
ProcessPayment(userInput);
```

---

## Testing Strategy

### Unit Test Coverage Requirements

**ValidationResult Tests:**
- ✅ Factory methods create correct instances
- ✅ Success results have `IsValid = true`, no error codes
- ✅ Failed results have `IsValid = false`, error code, error message
- ✅ `SupportedLevel` defaults to `Level` when not specified
- ✅ Readonly record struct equality works correctly

**Error Code Tests:**
- ✅ All ERR_FORMAT_* codes tested (invalid country, length, checksum, structure)
- ✅ All ERR_ACCOUNT_* codes tested (modulus failed, BBAN invalid, etc.)
- ✅ All ERR_INPUT_* codes tested (null, empty, unparseable)

**ValidationLevel Enum Tests:**
- ✅ NotValidated = 0, Structural = 1, AccountLevel = 2
- ✅ Enum ordering supports comparison (AccountLevel > Structural > NotValidated)

**Integration Tests:**
- ✅ Null input returns `Level = NotValidated`, `SupportedLevel = null`
- ✅ Valid KZ IBAN returns `Level = Structural`, `SupportedLevel = Structural`
- ✅ Valid GB IBAN returns `Level = AccountLevel`, `SupportedLevel = AccountLevel`
- ✅ Invalid GB checksum returns `Level = Structural`, `SupportedLevel = AccountLevel`
- ✅ GB modulus failure returns `Level = AccountLevel`, `ErrorCode = ERR_ACCOUNT_MODULUS_CHECK_FAILED`
- ✅ FR BBAN failure returns `Level = AccountLevel`, `ErrorCode = ERR_ACCOUNT_INVALID_BBAN`

### Test Data

**Countries to Test:**
- **Structural only**: KZ (Kazakhstan), DE (Germany), NL (Netherlands)
- **Account-level (UK modulus)**: GB (United Kingdom)
- **Account-level (BBAN)**: FR (France), IT (Italy), PT (Portugal), NO (Norway)

**Error Scenarios:**
- Invalid country code: `XX12ABCD...`
- Invalid length: `GB82WEST` (too short)
- Invalid checksum: `NL00ABNA0417164300`
- UK modulus failure: (requires valid IBAN with invalid modulus)
- BBAN failure: (requires valid IBAN with invalid BBAN structure)
- Null input: `null`
- Empty input: `""`
- Whitespace input: `"   "`
- Unparseable input: `"12345"`

---

## Backward Compatibility

### Breaking Changes: NONE ✅

**Existing API Surface:**
- ✅ `ValidationResult` properties remain unchanged
- ✅ Factory methods `Success()` and `Failed()` signatures extended (new optional parameters)
- ✅ `IIbanValidationService` interface unchanged
- ✅ Existing method calls continue to work

**New Features (Non-Breaking):**
- ✅ `SupportedLevel` property (nullable - default is null for existing callers)
- ✅ `NotValidated` enum value (existing code uses Structural)
- ✅ `ValidationErrorCodes` static class (new - doesn't affect existing code)
- ✅ Factory methods have new optional parameters (backward compatible)

### Migration Path

**Phase 1: Add Enhanced Structure (Non-Breaking)**
- Add `SupportedLevel` property (nullable)
- Add `NotValidated` enum value
- Add `ValidationErrorCodes` static class
- Update factory methods with optional parameters

**Phase 2: Update Implementation**
- Update `IbanValidationService` to use standardized error codes
- Set `SupportedLevel` in all validation paths
- Use `NotValidated` for input errors

**Phase 3: Update Tests**
- Add tests for new error codes
- Add tests for `SupportedLevel` property
- Add tests for `NotValidated` enum value

**Existing Code Continues to Work:**
```csharp
// Old code - still works
var result = ValidationResult.Success();
var failed = ValidationResult.Failed("ERROR", "Message");

// New code - enhanced
var result = ValidationResult.Success(
    country: "GB",
    level: ValidationLevel.AccountLevel,
    supportedLevel: ValidationLevel.AccountLevel
);
```

---

## Implementation Roadmap

### Ticket 1: Enhance ValidationResult Structure

**Scope:**
- Add `SupportedLevel` property to `ValidationResult`
- Add `NotValidated` enum value to `ValidationLevel`
- Update factory methods with optional `supportedLevel` parameter
- Update XML documentation

**Deliverables:**
- Updated `src/Iban.Core/ValidationResult.cs`
- Unit tests for new property and enum value

**Estimate:** Small (1-2 hours)

---

### Ticket 2: Standardize Error Codes

**Scope:**
- Create `ValidationErrorCodes` static class
- Define all ERR_FORMAT_*, ERR_ACCOUNT_*, ERR_INPUT_* constants
- Add XML documentation with examples

**Deliverables:**
- New file: `src/Iban.Core/ValidationErrorCodes.cs`
- Unit tests for error code constants

**Estimate:** Small (1 hour)

---

### Ticket 3: Update IbanValidationService Implementation

**Scope:**
- Update `Validate()` to use standardized error codes
- Update `ValidateWithAccountCheck()` to use standardized error codes
- Set `SupportedLevel` in all validation paths
- Use `NotValidated` for input errors
- Add `GetSupportedLevel()` helper method
- Add `MapIbanNetError()` error mapping method

**Deliverables:**
- Updated `src/Iban.Core/IbanValidationService.cs`
- Updated unit tests to verify error codes
- Updated integration tests to verify `SupportedLevel`

**Estimate:** Medium (3-4 hours)

---

### Ticket 4: Update Error Messages (Localization Prep)

**Scope:**
- Create error message templates for all error codes
- Add English default messages
- Prepare for future localization (strategy only, not implementation)

**Deliverables:**
- Documentation of error message templates
- Strategy document for localization (if needed in future)

**Estimate:** Small (1-2 hours)

---

## Localization Strategy

### Current Approach: English Defaults

All error messages are hardcoded in English within `IbanValidationService.cs`.

### Future Localization (Out of Scope for IBA-89)

**When Needed:**
- If API is exposed to international users requiring localized error messages
- If UI requires multi-language support

**Recommended Approach** (per dev.md):
- Use .NET resource files (.resx) for built-in localization support
- Keep localization "as late as possible" (near UI, not in service layer)
- For API: Return error codes (machine-readable) + English messages (default)
- Let API consumers handle localization on their end

**Decision:** Localization strategy defined, implementation deferred to future ticket if required.

---

## Definition of Done - Validation

### Architecture Checklist

- [x] Result structure architecture decided (Option A selected)
- [x] Complete class/interface definitions documented
- [x] Error code schema defined with all codes listed
- [x] Error message templates created (English defaults)
- [x] Integration points specified (IIbanValidationService - no changes required)
- [x] API consumer usage examples documented (6 examples provided)
- [x] Localization strategy decided (English defaults, .resx for future)
- [x] Testing requirements defined (unit + integration test coverage)
- [x] Backward compatibility assessment complete (no breaking changes)
- [x] Architecture specification document saved to `log_summary/iban_validation_result_architecture.md`
- [x] Ready for implementation ticket creation (4 tickets specified in roadmap)

---

## References

### Related Tickets
- **IBA-84**: IBAN Validation Service Architecture Design (baseline)
- **IBA-85**: IbanNet Library Integration (structural validation)
- **IBA-87**: Account-Level IBAN Validation Implementation (modulus + BBAN)

### Standards
- **ISO 13616:2020**: IBAN Standard
- **ISO/IEC 7064:2003**: MOD-97-10 Checksum Algorithm
- **ISO 3166-1 alpha-2**: Country Codes

### Libraries
- **IbanNet**: [https://github.com/skwasjer/IbanNet/wiki](https://github.com/skwasjer/IbanNet/wiki)
- **ModulusChecker.NetCore**: UK modulus checking
- **IbanNet.Extensions.Bban**: BBAN validation for 9+ countries

### Development Guides
- `docs/development/AI-DEVELOPMENT-GUIDE.md`: AI-optimized code organization
- `docs/development/dev.md`: .NET development guidance and best practices

---

## Appendix A: Country Support Matrix

| Country | Code | Structural | Account-Level | Validation Method |
|---------|------|-----------|---------------|-------------------|
| United Kingdom | GB | ✅ | ✅ | ModulusChecker.NetCore |
| France | FR | ✅ | ✅ | IbanNet.Extensions.Bban |
| Italy | IT | ✅ | ✅ | IbanNet.Extensions.Bban |
| Spain | ES | ✅ | ✅ | IbanNet.Extensions.Bban |
| Portugal | PT | ✅ | ✅ | IbanNet.Extensions.Bban |
| Norway | NO | ✅ | ✅ | IbanNet.Extensions.Bban |
| Finland | FI | ✅ | ✅ | IbanNet.Extensions.Bban |
| Belgium | BE | ✅ | ✅ | IbanNet.Extensions.Bban |
| Estonia | EE | ✅ | ✅ | IbanNet.Extensions.Bban |
| Lithuania | LT | ✅ | ✅ | IbanNet.Extensions.Bban |
| Netherlands | NL | ✅ | ❌ | IbanNet only |
| Germany | DE | ✅ | ❌ | IbanNet only |
| Kazakhstan | KZ | ✅ | ❌ | IbanNet only |
| *(Others)* | * | ✅ | ❌ | IbanNet only |

**Note**: BBAN country list (FI, BE, EE, LT) is illustrative - verify actual IbanNet.Extensions.Bban support during implementation.

---

## Appendix B: Error Code Quick Reference

```
Input Errors (No Validation):
  ERR_INPUT_NULL_OR_EMPTY        - Input is null, empty, or whitespace
  ERR_INPUT_UNPARSEABLE          - Cannot parse as IBAN format
  ERR_INPUT_NORMALIZATION_FAILED - Normalization failed

Format Errors (Structural Validation):
  ERR_FORMAT_INVALID_COUNTRY     - Country code not in ISO 3166-1
  ERR_FORMAT_INVALID_LENGTH      - Length doesn't match country spec
  ERR_FORMAT_INVALID_CHECKSUM    - MOD-97-10 checksum failed
  ERR_FORMAT_INVALID_STRUCTURE   - Invalid characters or structure

Account Errors (Account-Level Validation):
  ERR_ACCOUNT_MODULUS_CHECK_FAILED - UK modulus-11/10 check failed
  ERR_ACCOUNT_INVALID_BBAN         - BBAN validation failed
  ERR_ACCOUNT_INVALID_BANK_CODE    - Bank code not recognized
  ERR_ACCOUNT_INVALID_STRUCTURE    - Account number structure invalid
```

---

**End of Architecture Specification**
