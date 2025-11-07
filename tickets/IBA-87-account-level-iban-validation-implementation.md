# Account-Level IBAN Validation Implementation

**Objective**: Integrate ModulusChecker.NetCore and IbanNet.Extensions.Bban NuGet packages to add account-level validation for 10 countries, improving validation coverage and UK test success rate from 77% to 92%.

**Context**: Based on IBA-86 research findings, we can enhance our existing IbanNet structural validation (IBA-85) with account-level validation using two free, well-maintained NuGet packages.

**Related Tickets**:

* IBA-86: Account-Level IBAN Validation Research (Complete - research findings)
* IBA-85: IbanNet Library Integration (Complete - structural validation baseline)

---

## Implementation Tasks

### 1\. Install NuGet Packages

Install both packages for combined 10-country coverage:

```bash
dotnet add package ModulusChecker.NetCore --version 1.x.x
dotnet add package IbanNet.Extensions.Bban --version 5.x.x
```

**Package Details**:

* **ModulusChecker.NetCore**: UK modulus checking (620K downloads, Apache-2.0)
* **IbanNet.Extensions.Bban**: 9 countries BBAN validation (Apache-2.0)

**Verify**:

* Both packages installed successfully
* No dependency conflicts with existing IbanNet v5.19.0
* Licenses compatible (both Apache-2.0)

### 2\. Extend IbanValidationService

Enhance existing `IIbanValidationService` with account-level validation:

**Add New Method** (example - adapt to existing architecture):

```csharp
public interface IIbanValidationService
{
    // Existing structural validation methods
    bool IsValid(string iban);
    ValidationResult Validate(string iban);
    bool TryParse(string iban, out Iban parsed);
    
    // NEW: Account-level validation
    ValidationResult ValidateWithAccountCheck(string iban);
}
```

**Implementation Strategy**:

1. First perform structural validation (existing IbanNet logic)
2. If structural validation passes, apply account-level validation:
   * **UK IBANs**: Use ModulusChecker for modulus-11/10 validation
   * **Supported countries** (FR, IT, ES, PT, NO + 4 more): Use IbanNet.Extensions.Bban
   * **Unsupported countries**: Return structural validation result only

**Country Coverage**:

* UK (ModulusChecker.NetCore)
* France, Italy, Spain, Portugal, Norway + 4 more (IbanNet.Extensions.Bban)
* Other countries: Structural validation only (graceful fallback)

### 3\. UK Modulus Checking Integration

**Integrate ModulusChecker.NetCore for UK validation**:

```csharp
private ValidationResult ValidateUkAccount(string iban, Iban parsed)
{
    // Extract sort code and account number from UK IBAN
    // IBAN format: GBkk BBBB SSSS SSAA AAAA AA
    // Where: BB = bank code, SS = sort code, AA = account number
    
    var sortCode = ExtractSortCode(parsed);
    var accountNumber = ExtractAccountNumber(parsed);
    
    var checker = new ModulusChecker();
    var isValid = checker.CheckBankAccount(sortCode, accountNumber);
    
    if (!isValid)
        return ValidationResult.Failed("ERR_ACCOUNT_INVALID_UK_MODULUS");
        
    return ValidationResult.Success();
}
```

**UK-Specific Requirements**:

* Correctly extract sort code (6 digits) and account number (8 digits)
* Handle modulus-11, modulus-10, and double-alternate algorithms
* Handle VocaLink exception cases

### 4\. BBAN Validation Integration

**Integrate IbanNet.Extensions.Bban for multi-country validation**:

```csharp
private ValidationResult ValidateBban(string iban, Iban parsed)
{
    var bbanValidator = new BbanValidator();
    var result = bbanValidator.Validate(parsed);
    
    if (!result.IsValid)
        return ValidationResult.Failed("ERR_ACCOUNT_INVALID_BBAN");
        
    return ValidationResult.Success();
}
```

**Countries Supported** (verify exact list from IbanNet.Extensions.Bban docs):

* France (FR)
* Italy (IT)
* Spain (ES)
* Portugal (PT)
* Norway (NO)
* 
  * 4 additional countries (check package documentation)

### 5\. Unified Validation Strategy

**Implement cascading validation logic**:

```csharp
public ValidationResult ValidateWithAccountCheck(string iban)
{
    // Step 1: Structural validation (existing logic)
    var structuralResult = Validate(iban);
    if (!structuralResult.IsValid)
        return structuralResult;
        
    // Step 2: Parse IBAN
    if (!TryParse(iban, out var parsed))
        return ValidationResult.Failed("ERR_PARSE_FAILED");
        
    // Step 3: Account-level validation based on country
    var countryCode = parsed.Country.TwoLetterISORegionName;
    
    return countryCode switch
    {
        "GB" => ValidateUkAccount(iban, parsed),
        "FR" or "IT" or "ES" or "PT" or "NO" => ValidateBban(iban, parsed),
        // Add other supported countries from IbanNet.Extensions.Bban
        _ => structuralResult // Fallback: structural validation only
    };
}
```

**Error Codes to Add**:

* `ERR_ACCOUNT_INVALID_UK_MODULUS` - UK modulus checking failed
* `ERR_ACCOUNT_INVALID_BBAN` - BBAN validation failed for supported country
* Document that other countries return structural validation result

### 6\. Unit Tests (TDD Approach)

**Write tests FIRST for the 3 failing **[**IBAN.com**](http://IBAN.com)** test cases**:

```csharp
[Fact]
public void ValidateWithAccountCheck_GB02BARC_InvalidAccount_ReturnsFailed()
{
    // This was failing in IBA-85 - should now be caught
    var service = CreateService();
    var result = service.ValidateWithAccountCheck("GB02BARC20201530093451");
    
    result.IsValid.Should().BeFalse();
    result.ErrorCode.Should().Be("ERR_ACCOUNT_INVALID_UK_MODULUS");
}

[Fact]
public void ValidateWithAccountCheck_GB68CITI_InvalidAccount_ReturnsFailed()
{
    var service = CreateService();
    var result = service.ValidateWithAccountCheck("GB68CITI18500483515538");
    
    result.IsValid.Should().BeFalse();
    result.ErrorCode.Should().Be("ERR_ACCOUNT_INVALID_UK_MODULUS");
}

[Fact]
public void ValidateWithAccountCheck_GB24BARC_InvalidAccount_ReturnsFailed()
{
    var service = CreateService();
    var result = service.ValidateWithAccountCheck("GB24BARC20201630093459");
    
    result.IsValid.Should().BeFalse();
    result.ErrorCode.Should().Be("ERR_ACCOUNT_INVALID_UK_MODULUS");
}

[Theory]
[InlineData("GB82WEST12345698765432")] // Valid UK IBAN (from IBAN.com)
public void ValidateWithAccountCheck_ValidUkIban_ReturnsSuccess(string iban)
{
    var service = CreateService();
    var result = service.ValidateWithAccountCheck(iban);
    
    result.IsValid.Should().BeTrue();
}

[Theory]
[InlineData("FR1420041010050500013M02606")] // Valid French IBAN
[InlineData("IT60X0542811101000000123456")] // Valid Italian IBAN
[InlineData("ES9121000418450200051332")] // Valid Spanish IBAN
public void ValidateWithAccountCheck_ValidBbanCountries_ReturnsSuccess(string iban)
{
    var service = CreateService();
    var result = service.ValidateWithAccountCheck(iban);
    
    result.IsValid.Should().BeTrue();
}

[Fact]
public void ValidateWithAccountCheck_UnsupportedCountry_ReturnsStructuralValidation()
{
    // DE (Germany) not supported by either package
    var service = CreateService();
    var result = service.ValidateWithAccountCheck("DE89370400440532013000");
    
    // Should pass structural validation but not fail on missing account check
    result.IsValid.Should().BeTrue(); // Graceful fallback
}

[Fact]
public void ValidateWithAccountCheck_StructurallyInvalid_FailsBeforeAccountCheck()
{
    var service = CreateService();
    var result = service.ValidateWithAccountCheck("GB00BARC20201530093451"); // Bad checksum
    
    result.IsValid.Should().BeFalse();
    result.ErrorCode.Should().NotBe("ERR_ACCOUNT_INVALID_UK_MODULUS"); // Fails at structural level
}
```

**Additional Test Coverage**:

* Edge cases: Empty/null input handling
* All 10 supported countries with valid/invalid examples
* Graceful fallback for unsupported countries
* Integration: Both packages working together correctly
* Performance: Account validation doesn't significantly slow down validation

### 7\. Update [IBAN.com](http://IBAN.com) Test Success Rate

**Re-run **[**IBAN.com**](http://IBAN.com)** test suite** (13 test cases):

* **Before**: 10/13 passing (77%)
* **Expected After**: 12/13 passing (92%)
* **Remaining failure**: GB24BARC (bank code not found) - may still fail if ModulusChecker doesn't validate bank codes

Document actual results and any remaining failures.

---

## Definition of Done

- [ ] ModulusChecker.NetCore package installed (verify no conflicts)
- [ ] IbanNet.Extensions.Bban package installed (verify no conflicts)
- [ ] `ValidateWithAccountCheck` method implemented with cascading validation
- [ ] UK modulus checking integrated for GB country code
- [ ] BBAN validation integrated for supported countries (FR, IT, ES, PT, NO + 4 more)
- [ ] Graceful fallback for unsupported countries (structural validation only)
- [ ] Error codes added: ERR_ACCOUNT_INVALID_UK_MODULUS, ERR_ACCOUNT_INVALID_BBAN
- [ ] Unit tests written (TDD approach) - minimum 15 tests
- [ ] All 3 previously failing [IBAN.com](http://IBAN.com) cases now caught (or documented if not)
- [ ] [IBAN.com](http://IBAN.com) test success rate improved (document actual %)
- [ ] No regressions in existing structural validation tests
- [ ] All unit tests passing
- [ ] XML documentation added to new public APIs
- [ ] Code follows Centro patterns and standards
- [ ] Code review completed and approved
- [ ] No build warnings or errors

---

## Testing Approach (TDD)

**Test-Driven Development Process**:

1. Write failing test for UK modulus checking
2. Implement minimal code to pass test
3. Write failing test for BBAN validation
4. Implement minimal code to pass test
5. Refactor for quality
6. Test [IBAN.com](http://IBAN.com) cases
7. Document results

**NO separate integration test tickets** - unit tests cover validation logic.

---

## Reference Information

**Research Findings** (IBA-86):

* ModulusChecker.NetCore: 620K downloads, Apache-2.0, active maintenance
* IbanNet.Extensions.Bban: Part of IbanNet ecosystem, Apache-2.0
* Combined effort: 3-5 hours
* Coverage: 10 countries, 6/7 major EU markets

**Package Documentation**:

* ModulusChecker.NetCore: [https://www.nuget.org/packages/ModulusChecker.NetCore](https://www.nuget.org/packages/ModulusChecker.NetCore)
* IbanNet.Extensions.Bban: [https://www.nuget.org/packages/IbanNet.Extensions.Bban](https://www.nuget.org/packages/IbanNet.Extensions.Bban)
* IbanNet Wiki: [https://github.com/skwasjer/IbanNet/wiki](https://github.com/skwasjer/IbanNet/wiki)

**Test Reference**:

* [IBAN.com](http://IBAN.com) test cases: [https://www.iban.com/testibans](https://www.iban.com/testibans)
* Current: 10/13 passing (77%)
* Target: 12/13 passing (92%)

**Related Work**:

* IBA-85: Baseline structural validation implementation
* IBA-86: Research findings document

---

## Important Notes

* **Follow existing architecture**: Extend IBA-85 implementation, don't rebuild
* **TDD Approach**: Write tests for failing [IBAN.com](http://IBAN.com) cases FIRST
* **Graceful degradation**: Unsupported countries fall back to structural validation
* **Security**: Maintain existing IBAN masking/obfuscation practices
* **Performance**: Account validation adds minimal overhead (local algorithms, no API calls)
* **Future extensibility**: Architecture should support adding more countries/packages later

**Expected Outcome**: Validation coverage improves from 77% to 92% for UK test cases, with 10-country account-level validation support.
