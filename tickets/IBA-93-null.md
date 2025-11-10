# Update IbanValidationService to Use Enhanced Result Structure

**Objective**: Integrate enhanced IbanValidationResult and IbanValidationError enum into the validation service, updating all validation logic to use new error codes and validation levels.

## Context

This ticket integrates the deliverables from:
- **IBA-91**: ValidationLevel enum with SupportedLevel property
- **IBA-92**: IbanValidationError enum with 11 error codes

The IbanValidationService currently exists but needs to be updated to use the new enhanced result structures.

## Acceptance Criteria

* Update IbanValidationService to return IbanValidationResult
* Map validation failures to appropriate IbanValidationError codes
* Set correct ValidationLevel for each validation check
* Populate SupportedLevel property based on validation depth
* Update all tests to use new result structure
* Ensure backward compatibility where needed

## Implementation Tasks

### 1. Update Service Signature

Update IbanValidationService methods to return IbanValidationResult:

```csharp
public interface IIbanValidationService
{
    IbanValidationResult Validate(string iban);
    // ... other methods
}
```

### 2. Error Code Mapping

Map existing validation failures to IbanValidationError codes:

**Format Validation** (ValidationLevel.Structure):
- Null/Empty → ERR_INPUT_NULL / ERR_INPUT_EMPTY
- Invalid format → ERR_FORMAT_INVALID
- Wrong length → ERR_FORMAT_LENGTH
- Bad country code → ERR_FORMAT_COUNTRY_INVALID
- Checksum failure → ERR_FORMAT_CHECKSUM

**Account Validation** (ValidationLevel.AccountLevel):
- Modulus check failure → ERR_ACCOUNT_MODULUS
- BBAN validation failure → ERR_ACCOUNT_BBAN
- Sort code issues → ERR_ACCOUNT_SORTCODE
- Account number issues → ERR_ACCOUNT_NUMBER

### 3. Validation Level Assignment

Set appropriate ValidationLevel based on check type:
- IbanNet structural checks → ValidationLevel.Structure
- ModulusChecker / BBAN checks → ValidationLevel.AccountLevel

### 4. SupportedLevel Property

Populate SupportedLevel to indicate validation depth:
- If only structural validation available → SupportedLevel = Structure
- If account-level validation available → SupportedLevel = AccountLevel

### 5. Test Updates

Update existing tests to:
- Assert against IbanValidationError codes
- Verify ValidationLevel is set correctly
- Check SupportedLevel matches validation capability
- Maintain existing test coverage (93.54% line coverage)

## Integration Points

**Dependencies**:
- IBA-91: ValidationLevel enum (completed)
- IBA-92: IbanValidationError enum (completed)
- IBA-85: IbanNet integration (completed)
- IBA-87: Account-level validation (completed)

**Service Integration**:
- Update IbanValidationService implementation
- Ensure IIbanValidationService interface compatibility
- Maintain dependency injection configuration

## Definition of Done

- [ ] IbanValidationService returns IbanValidationResult
- [ ] All validation failures mapped to IbanValidationError codes
- [ ] ValidationLevel set correctly for each check
- [ ] SupportedLevel populated based on validation capability
- [ ] All existing tests updated and passing
- [ ] Test coverage maintained (≥93%)
- [ ] Code compiles without warnings
- [ ] TDD process followed

**Labels**: backend
