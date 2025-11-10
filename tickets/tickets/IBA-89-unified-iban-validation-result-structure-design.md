# Unified IBAN Validation Result Structure Design

**Objective**: Design a unified result structure that seamlessly handles both structural validation (IBA-85) and account-level validation (IBA-87) results, providing clear, actionable error information to API consumers.

**Context**:

* IBA-85 implemented structural IBAN validation (format, checksum, length per ISO 13616)
* IBA-87 added account-level validation (UK modulus checking + 9 countries BBAN validation)
* Currently, validation results may come from different sources (IbanNet, ModulusChecker, IbanNet.Extensions.Bban)
* Need unified result structure that communicates validation level, error type, and actionable messages

**Related Tickets**:

* IBA-85: IbanNet Library Integration (structural validation)
* IBA-87: Account-Level IBAN Validation Implementation (ModulusChecker + BBAN)
* IBA-84: IBAN Validation Service Architecture Design (baseline architecture)

---

## Design Requirements

### 1\. Validation Levels

The result structure must distinguish between validation levels:

**Structural Validation** (ISO 13616:2020):

* Country code validity (ISO 3166-1 alpha-2)
* MOD-97-10 checksum (ISO/IEC 7064:2003)
* Length validation per country
* Character structure validation

**Account-Level Validation** (Enhanced):

* UK: Modulus-11/10 checking via ModulusChecker.NetCore
* 9 Countries: BBAN validation via IbanNet.Extensions.Bban (FR, IT, ES, PT, NO + 4 more)
* Other countries: Graceful fallback to structural validation only

### 2\. Error Classification

Result structure must support multiple error categories:

**Format Errors** (structural):

* Invalid country code
* Invalid length for country
* Invalid characters
* Invalid checksum (MOD-97-10 failed)

**Account Errors** (account-level):

* UK modulus check failed
* BBAN validation failed
* Bank code not found/invalid
* Account number structure invalid

**Input Errors**:

* Null/empty input
* Unparseable format
* Whitespace/normalization issues

### 3\. Result Metadata

Must include:

* **ValidationLevel**: Which level of validation was performed (Structural, AccountLevel, NotValidated)
* **IsValid**: Boolean success indicator
* **ErrorCode**: Machine-readable error code (for API clients)
* **ErrorMessage**: Human-readable error message (for UI display)
* **Country**: Detected country code (if parseable)
* **SupportedValidationLevel**: What level of validation is available for this country

---

## Design Approach Options

### Option A: Unified Result Class (RECOMMENDED)

Single result class with clear properties. Simple API, easy to consume.

### Option B: Result Pattern with Success/Failure Types

Type-safe discriminated union. Better for pattern matching scenarios.

### Option C: Builder Pattern

Flexible but verbose. Overkill for current needs.

---

## Error Code Design

**Proposed Schema**: `ERR_[CATEGORY]_[SPECIFIC]`

**Format Errors** (ERR_FORMAT\_\*):

* ERR_FORMAT_INVALID_COUNTRY - Country code not in ISO 3166-1
* ERR_FORMAT_INVALID_LENGTH - Length doesn't match country spec
* ERR_FORMAT_INVALID_CHECKSUM - MOD-97-10 checksum failed
* ERR_FORMAT_INVALID_CHAR - Contains invalid characters

**Account Errors** (ERR_ACCOUNT\_\*):

* ERR_ACCOUNT_UK_MODULUS - UK modulus-11/10 check failed
* ERR_ACCOUNT_BBAN_INVALID - BBAN validation failed
* ERR_ACCOUNT_BANK_NOT_FOUND - Bank code not recognized
* ERR_ACCOUNT_STRUCTURE - Account number structure invalid

**Input Errors** (ERR_INPUT\_\*):

* ERR_INPUT_NULL_OR_EMPTY - Input is null or empty
* ERR_INPUT_UNPARSEABLE - Cannot parse as IBAN
* ERR_INPUT_NORMALIZATION - Normalization failed

---

## Definition of Done


---

## Deliverables

1. **Architecture Decision Document** - Complete design specification
2. **Result Structure Specification** - Chosen option with full API definition
3. **Error Code Reference** - Complete list with descriptions and messages
4. **Integration Specification** - Changes to IIbanValidationService
5. **Testing Strategy** - Coverage requirements for unit tests

---

## Out of Scope

* ❌ Implementation (design/architecture only - no coding)
* ❌ Localization implementation (strategy decided, implementation later)
* ❌ API endpoint design (service layer only)
* ❌ Database schema changes (no persistence needed)

---

## References

* IBA-84: IBAN Validation Service Architecture Design
* IBA-85: IbanNet Library Integration
* IBA-87: Account-Level IBAN Validation Implementation
* ISO 13616:2020: IBAN Standard
* IbanNet Documentation: [https://github.com/skwasjer/IbanNet/wiki](https://github.com/skwasjer/IbanNet/wiki)

---
## Implementation Completed
- **Ticket**: IBA-89
- **PR**: 
- **Domain**: tickets
- **TDD Cycles**: 0 completed
- **Tests**: unknown passing
- **Files Changed**: unknown
- **Merged**: 2025-11-10T13:48:41+00:00
- **Branch**: feature/IBA-89-unified-iban-validation-result-structure-design (deleted)

This ticket has been completed and deployed through the Centro development workflow.
The implementation has been merged to dev branch and deployed to staging environment.
