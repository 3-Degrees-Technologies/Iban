using NUnit.Framework;

namespace Iban.Core.Tests;

[TestFixture]
public class IbanValidationServiceTests
{
    [Test]
    public void IsValid_ShouldValidateIbansCorrectly()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act & Assert - Valid IBANs from different countries
        Assert.That(service.IsValid("NL91ABNA0417164300"), Is.True, "Valid Netherlands IBAN should return true");
        Assert.That(service.IsValid("GB82WEST12345698765432"), Is.True, "Valid UK IBAN should return true");
        Assert.That(service.IsValid("DE89370400440532013000"), Is.True, "Valid Germany IBAN should return true");
        Assert.That(service.IsValid("KZ86125KZT5004100100"), Is.True, "Valid Kazakhstan IBAN should return true");
        Assert.That(service.IsValid("KZ176010251000042993"), Is.True, "Valid Kazakhstan IBAN (alternative format) should return true");

        // Act & Assert - Invalid IBANs (bad checksum)
        Assert.That(service.IsValid("NL00ABNA0417164300"), Is.False, "Invalid checksum should return false");
        Assert.That(service.IsValid("GB00WEST12345698765432"), Is.False, "Invalid checksum should return false");
        Assert.That(service.IsValid("KZ00125KZT5004100100"), Is.False, "Invalid Kazakhstan IBAN checksum should return false");

        // Act & Assert - Invalid formats
        Assert.That(service.IsValid(""), Is.False, "Empty string should return false");
        Assert.That(service.IsValid("   "), Is.False, "Whitespace should return false");
        Assert.That(service.IsValid("INVALID"), Is.False, "Invalid format should return false");
        Assert.That(service.IsValid("XX99123456789"), Is.False, "Invalid country code should return false");

        // Mixed results force real validation logic, prevent gaming
    }

    [Test]
    public void IsValid_ShouldHandleNullInput()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act & Assert
        Assert.That(service.IsValid(null), Is.False, "Null input should return false");
    }

    [Test]
    public void IsValid_ShouldNormalizeInput()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act & Assert - Input normalization (spaces, lowercase)
        Assert.That(service.IsValid("NL91 ABNA 0417 1643 00"), Is.True, "IBAN with spaces should be normalized and valid");
        Assert.That(service.IsValid("nl91abna0417164300"), Is.True, "Lowercase IBAN should be normalized and valid");
        Assert.That(service.IsValid("  NL91ABNA0417164300  "), Is.True, "IBAN with surrounding whitespace should be trimmed and valid");

        // Mixed normalization cases prevent hardcoding
    }

    [Test]
    public void Validate_ShouldReturnDetailedValidationResult()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act - Valid IBAN
        var validResult = service.Validate("NL91ABNA0417164300");

        // Assert
        Assert.That(validResult.IsValid, Is.True, "Valid IBAN should have IsValid=true");
        Assert.That(validResult.ErrorMessage, Is.Null.Or.Empty, "Valid IBAN should have no error message");

        // Act - Invalid IBAN
        var invalidResult = service.Validate("NL00ABNA0417164300");

        // Assert
        Assert.That(invalidResult.IsValid, Is.False, "Invalid IBAN should have IsValid=false");
        Assert.That(invalidResult.ErrorMessage, Is.Not.Null.And.Not.Empty, "Invalid IBAN should have error message");
    }

    [Test]
    public void Validate_ShouldHandleNullInput()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.Validate(null);

        // Assert
        Assert.That(result.IsValid, Is.False, "Null input should return invalid result");
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty, "Null input should have error message");
    }

    [Test]
    public void TryParse_ShouldParseValidIbanSuccessfully()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var successResult = service.TryParse("NL91ABNA0417164300", out var parsedIban);

        // Assert
        Assert.That(successResult, Is.True, "Valid IBAN should parse successfully");
        Assert.That(parsedIban, Is.Not.Null, "Parsed IBAN should not be null");
        Assert.That(parsedIban!.Value.Country, Is.EqualTo("NL"), "Country code should be NL");

        // Act - Invalid IBAN
        var failureResult = service.TryParse("INVALID", out var failedIban);

        // Assert
        Assert.That(failureResult, Is.False, "Invalid IBAN should fail to parse");
        Assert.That(failedIban, Is.Null, "Failed parse should return null");
    }

    [Test]
    public void ValidateWithAccountCheck_ShouldEnforceUkModulusCheckingCorrectly()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act & Assert - Valid UK IBANs with correct modulus
        var validResult1 = service.ValidateWithAccountCheck("GB33BUKB20201555555555");
        Assert.That(validResult1.IsValid, Is.True, "Valid UK IBAN with correct modulus should pass");
        Assert.That(validResult1.ErrorCode, Is.Null, "Valid result should have no error code");

        var validResult2 = service.ValidateWithAccountCheck("GB29NWBK60161331926819");
        Assert.That(validResult2.IsValid, Is.True, "Second valid UK IBAN should pass");
        Assert.That(validResult2.ErrorCode, Is.Null, "Valid result should have no error code");

        // Act & Assert - Invalid UK IBANs failing modulus check (from IBAN.com test cases)
        var invalidResult1 = service.ValidateWithAccountCheck("GB02BARC20201530093451");
        Assert.That(invalidResult1.IsValid, Is.False, "UK IBAN with invalid modulus should fail");
        Assert.That(invalidResult1.ErrorCode, Is.EqualTo(IbanValidationError.ERR_ACCOUNT_MODULUS), "Should return UK modulus error code");
        Assert.That(invalidResult1.ErrorMessage, Is.Not.Null.And.Not.Empty, "Should provide error message");

        var invalidResult2 = service.ValidateWithAccountCheck("GB68CITI18500483515538");
        Assert.That(invalidResult2.IsValid, Is.False, "Second UK IBAN with invalid modulus should fail");
        Assert.That(invalidResult2.ErrorCode, Is.EqualTo(IbanValidationError.ERR_ACCOUNT_MODULUS), "Should return UK modulus error code");

        // Mixed true/false results force real UK modulus checking logic
    }

    [Test]
    public void ValidateWithAccountCheck_ShouldValidateBbanCheckDigitsForSupportedCountries()
    {
        // Arrange
        var service = new IbanValidationService();

        // This test ensures BBAN (national check digit) validation works for supported countries
        // Currently only tests the happy path - GREEN phase will add actual BBAN failure cases

        // France - RIB check validation
        var resultFr = service.ValidateWithAccountCheck("FR1420041010050500013M02606");
        Assert.That(resultFr.IsValid, Is.True, "Valid FR IBAN should pass");

        // Italy - CIN check validation  
        var resultIt = service.ValidateWithAccountCheck("IT60X0542811101000000123456");
        Assert.That(resultIt.IsValid, Is.True, "Valid IT IBAN should pass");

        // Portugal - NIB check validation
        var resultPt = service.ValidateWithAccountCheck("PT50000201231234567890154");
        Assert.That(resultPt.IsValid, Is.True, "Valid PT IBAN should pass");

        // Norway - MOD-11 check validation
        var resultNo = service.ValidateWithAccountCheck("NO9386011117947");
        Assert.That(resultNo.IsValid, Is.True, "Valid NO IBAN should pass");

        // Test one invalid case to force RED state
        // This IBAN has correct structure but invalid BBAN check digit (invalid RIB key)
        // Without BBAN validation implementation, this will incorrectly pass
        var invalidBban = service.ValidateWithAccountCheck("FR2520041010050500013M02699");
        Assert.That(invalidBban.IsValid, Is.False, "IBAN with invalid BBAN check should fail");
        Assert.That(invalidBban.ErrorCode, Is.EqualTo(IbanValidationError.ERR_ACCOUNT_BBAN), "Should return BBAN error code");
    }

    [Test]
    public void TryParse_ShouldPopulateAllIbanComponents()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.TryParse("GB82 WEST 1234 5698 7654 32", out var parsed);

        // Assert
        Assert.That(result, Is.True, "Valid IBAN should parse successfully");
        Assert.That(parsed, Is.Not.Null);
        Assert.That(parsed!.Value.Country, Is.EqualTo("GB"), "Country should be GB");
        Assert.That(parsed!.Value.CheckDigits, Is.EqualTo("82"), "Check digits should be 82");
        Assert.That(parsed!.Value.Bban, Is.EqualTo("WEST12345698765432"), "BBAN should be the account-specific part");
        Assert.That(parsed!.Value.NormalizedIban, Is.EqualTo("GB82WEST12345698765432"), "Normalized IBAN should have no spaces");
    }

    [Test]
    public void ValidationResult_ShouldIncludeCountryAndLevel()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act - Account-level validation for UK
        var ukResult = service.ValidateWithAccountCheck("GB33BUKB20201555555555");

        // Assert
        Assert.That(ukResult.Country, Is.EqualTo("GB"), "Should include country code");
        Assert.That(ukResult.Level, Is.EqualTo(ValidationLevel.AccountLevel), "UK validation should be account-level");
        Assert.That(ukResult.IsValid, Is.True);

        // Act - Account-level validation for FR
        var frResult = service.ValidateWithAccountCheck("FR1420041010050500013M02606");

        // Assert
        Assert.That(frResult.Country, Is.EqualTo("FR"), "Should include country code");
        Assert.That(frResult.Level, Is.EqualTo(ValidationLevel.AccountLevel), "FR validation should be account-level");
        Assert.That(frResult.IsValid, Is.True);

        // Act - Structural-only validation for unsupported country
        var deResult = service.ValidateWithAccountCheck("DE89370400440532013000");

        // Assert
        Assert.That(deResult.Country, Is.EqualTo("DE"), "Should include country code");
        Assert.That(deResult.Level, Is.EqualTo(ValidationLevel.Structural), "DE validation should be structural-only");
        Assert.That(deResult.IsValid, Is.True, "Structural validation should pass");
    }

    [Test]
    public void ValidationResult_ShouldIncludeCountryOnFailure()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act - Failed UK modulus check
        var ukFailed = service.ValidateWithAccountCheck("GB02BARC20201530093451");

        // Assert
        Assert.That(ukFailed.IsValid, Is.False);
        Assert.That(ukFailed.Country, Is.EqualTo("GB"), "Failed result should include country");
        Assert.That(ukFailed.Level, Is.EqualTo(ValidationLevel.AccountLevel), "Failure occurred at account level");
        Assert.That(ukFailed.ErrorCode, Is.EqualTo(IbanValidationError.ERR_ACCOUNT_MODULUS));

        // Act - Failed BBAN validation
        var frFailed = service.ValidateWithAccountCheck("FR2520041010050500013M02699");

        // Assert
        Assert.That(frFailed.IsValid, Is.False);
        Assert.That(frFailed.Country, Is.EqualTo("FR"), "Failed result should include country");
        Assert.That(frFailed.Level, Is.EqualTo(ValidationLevel.AccountLevel), "Failure occurred at account level");
        Assert.That(frFailed.ErrorCode, Is.EqualTo(IbanValidationError.ERR_ACCOUNT_BBAN));
    }

    [Test]
    public void ValidateWithAccountCheck_ShouldHandleNullInput()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.ValidateWithAccountCheck(null);

        // Assert
        Assert.That(result.IsValid, Is.False, "Null input should return invalid result");
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty, "Null input should have error message");
    }

    [Test]
    public void TryParse_ShouldHandleNullInput()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var parseResult = service.TryParse(null, out var parsed);

        // Assert
        Assert.That(parseResult, Is.False, "Null input should return false");
        Assert.That(parsed, Is.Null, "Null input should not produce parsed IBAN");
    }

    #region Input Validation Error Code Tests

    [Test]
    public void Validate_ShouldReturnERR_INPUT_NULL_ForNullInput()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.Validate(null);

        // Assert
        Assert.That(result.IsValid, Is.False, "Null input should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_INPUT_NULL), "Should return ERR_INPUT_NULL");
        Assert.That(result.ErrorMessage, Is.EqualTo("IBAN cannot be null"), "Should have correct error message");
    }

    [Test]
    public void ValidateWithAccountCheck_ShouldReturnERR_INPUT_NULL_ForNullInput()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.ValidateWithAccountCheck(null);

        // Assert
        Assert.That(result.IsValid, Is.False, "Null input should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_INPUT_NULL), "Should return ERR_INPUT_NULL");
        Assert.That(result.ErrorMessage, Is.EqualTo("IBAN cannot be null"), "Should have correct error message");
    }

    [Test]
    public void Validate_ShouldReturnERR_INPUT_EMPTY_ForEmptyString()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.Validate("");

        // Assert
        Assert.That(result.IsValid, Is.False, "Empty string should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_INPUT_EMPTY), "Should return ERR_INPUT_EMPTY");
        Assert.That(result.ErrorMessage, Is.EqualTo("IBAN cannot be empty"), "Should have correct error message");
    }

    [Test]
    public void ValidateWithAccountCheck_ShouldReturnERR_INPUT_EMPTY_ForEmptyString()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.ValidateWithAccountCheck("");

        // Assert
        Assert.That(result.IsValid, Is.False, "Empty string should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_INPUT_EMPTY), "Should return ERR_INPUT_EMPTY");
        Assert.That(result.ErrorMessage, Is.EqualTo("IBAN cannot be empty"), "Should have correct error message");
    }

    [Test]
    public void Validate_ShouldReturnERR_INPUT_WHITESPACE_ForWhitespaceOnlyString()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.Validate("   ");

        // Assert
        Assert.That(result.IsValid, Is.False, "Whitespace-only string should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_INPUT_WHITESPACE), "Should return ERR_INPUT_WHITESPACE");
        Assert.That(result.ErrorMessage, Is.EqualTo("IBAN cannot be whitespace only"), "Should have correct error message");
    }

    [Test]
    public void ValidateWithAccountCheck_ShouldReturnERR_INPUT_WHITESPACE_ForWhitespaceOnlyString()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.ValidateWithAccountCheck("   ");

        // Assert
        Assert.That(result.IsValid, Is.False, "Whitespace-only string should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_INPUT_WHITESPACE), "Should return ERR_INPUT_WHITESPACE");
        Assert.That(result.ErrorMessage, Is.EqualTo("IBAN cannot be whitespace only"), "Should have correct error message");
    }

    [Test]
    public void Validate_ShouldReturnERR_INPUT_WHITESPACE_ForTabsAndNewlines()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act
        var result = service.Validate("\t\n\r");

        // Assert
        Assert.That(result.IsValid, Is.False, "Tabs/newlines should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_INPUT_WHITESPACE), "Should return ERR_INPUT_WHITESPACE");
    }

    [Test]
    public void Validate_ShouldReturnERR_FORMAT_INVALID_ForInvalidFormat()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act - Invalid format (too short)
        var result1 = service.Validate("INVALID");

        // Assert
        Assert.That(result1.IsValid, Is.False, "Invalid format should be invalid");
        Assert.That(result1.ErrorCode, Is.EqualTo(IbanValidationError.ERR_FORMAT_INVALID), "Should return ERR_FORMAT_INVALID");
        Assert.That(result1.ErrorMessage, Is.Not.Null.And.Not.Empty, "Should have error message");

        // Act - Invalid country code
        var result2 = service.Validate("XX99123456789");

        // Assert
        Assert.That(result2.IsValid, Is.False, "Invalid country code should be invalid");
        Assert.That(result2.ErrorCode, Is.EqualTo(IbanValidationError.ERR_FORMAT_INVALID), "Should return ERR_FORMAT_INVALID for invalid country");
    }

    [Test]
    public void ValidateWithAccountCheck_ShouldReturnERR_FORMAT_INVALID_ForInvalidFormat()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act - Invalid format (bad checksum)
        var result = service.ValidateWithAccountCheck("GB00WEST12345698765432");

        // Assert
        Assert.That(result.IsValid, Is.False, "Invalid checksum should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_FORMAT_INVALID), "Should return ERR_FORMAT_INVALID");
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty, "Should have error message");
    }

    [Test]
    public void Validate_ShouldReturnERR_FORMAT_LENGTH_ForWrongLength()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act - DE IBAN that is one character too short (21 instead of 22)
        var result = service.Validate("DE8937040044053201300");

        // Assert
        Assert.That(result.IsValid, Is.False, "Wrong-length IBAN should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_FORMAT_LENGTH), "Should return ERR_FORMAT_LENGTH");
    }

    [Test]
    public void Validate_ShouldReturnERR_FORMAT_CHECKSUM_ForBadCheckDigits()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act - structurally well-formed NL IBAN with an invalid MOD-97 checksum
        var result = service.Validate("NL92ABNA0417164300");

        // Assert
        Assert.That(result.IsValid, Is.False, "Bad checksum IBAN should be invalid");
        Assert.That(result.ErrorCode, Is.EqualTo(IbanValidationError.ERR_FORMAT_CHECKSUM), "Should return ERR_FORMAT_CHECKSUM");
    }

    [Test]
    public void Validate_ShouldPerformStructuralValidationOnly_NotBbanCheck()
    {
        // Arrange
        var service = new IbanValidationService();

        // A FR IBAN that is structurally valid but fails the national (BBAN) check digit.
        // Validate() is documented as structural-only, so it must accept it; only
        // ValidateWithAccountCheck() should reject it at the account level.
        const string structurallyValidBadBban = "FR2520041010050500013M02699";

        // Act
        var structural = service.Validate(structurallyValidBadBban);
        var accountLevel = service.ValidateWithAccountCheck(structurallyValidBadBban);

        // Assert
        Assert.That(structural.IsValid, Is.True, "Structural validation should pass (BBAN not checked)");
        Assert.That(structural.Level, Is.EqualTo(ValidationLevel.Structural));
        Assert.That(accountLevel.IsValid, Is.False, "Account-level validation should fail the BBAN check");
        Assert.That(accountLevel.ErrorCode, Is.EqualTo(IbanValidationError.ERR_ACCOUNT_BBAN));
    }

    #endregion

    #region SupportedLevel Property Tests

    [Test]
    public void Validate_ShouldSetSupportedLevel_ForGBCountry()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act - Valid GB IBAN
        var validResult = service.Validate("GB82WEST12345698765432");

        // Assert
        Assert.That(validResult.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "GB should support AccountLevel validation");
        Assert.That(validResult.Level, Is.EqualTo(ValidationLevel.Structural), "Validate() performs Structural validation");

        // Act - Invalid GB IBAN
        var invalidResult = service.Validate("GB00WEST12345698765432");

        // Assert
        Assert.That(invalidResult.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "GB should support AccountLevel even when invalid");
    }

    [Test]
    public void Validate_ShouldSetSupportedLevel_ForBBANSupportedCountries()
    {
        // Arrange
        var service = new IbanValidationService();

        // Test FR (France) - BBAN supported
        var frResult = service.Validate("FR1420041010050500013M02606");
        Assert.That(frResult.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "FR should support AccountLevel");

        // Test IT (Italy) - BBAN supported
        var itResult = service.Validate("IT60X0542811101000000123456");
        Assert.That(itResult.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "IT should support AccountLevel");

        // Test PT (Portugal) - BBAN supported
        var ptResult = service.Validate("PT50000201231234567890154");
        Assert.That(ptResult.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "PT should support AccountLevel");

        // Test NO (Norway) - BBAN supported
        var noResult = service.Validate("NO9386011117947");
        Assert.That(noResult.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "NO should support AccountLevel");
    }

    [Test]
    public void Validate_ShouldSetSupportedLevel_ForStructuralOnlyCountries()
    {
        // Arrange
        var service = new IbanValidationService();

        // Test DE (Germany) - Structural only
        var deResult = service.Validate("DE89370400440532013000");
        Assert.That(deResult.SupportedLevel, Is.EqualTo(ValidationLevel.Structural), "DE should support Structural only");

        // Test NL (Netherlands) - Structural only
        var nlResult = service.Validate("NL91ABNA0417164300");
        Assert.That(nlResult.SupportedLevel, Is.EqualTo(ValidationLevel.Structural), "NL should support Structural only");
    }

    [Test]
    public void ValidateWithAccountCheck_ShouldSetBothLevelsToAccountLevel_WhenAccountCheckPerformed()
    {
        // Arrange
        var service = new IbanValidationService();

        // Act - GB with successful account check
        var gbResult = service.ValidateWithAccountCheck("GB33BUKB20201555555555");

        // Assert
        Assert.That(gbResult.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "GB supports AccountLevel");
        Assert.That(gbResult.Level, Is.EqualTo(ValidationLevel.AccountLevel), "Account check was performed");

        // Act - FR with successful BBAN check
        var frResult = service.ValidateWithAccountCheck("FR1420041010050500013M02606");

        // Assert
        Assert.That(frResult.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "FR supports AccountLevel");
        Assert.That(frResult.Level, Is.EqualTo(ValidationLevel.AccountLevel), "BBAN check was performed");
    }

    #endregion
}

[TestFixture]
public class ParsedIbanBankIdentifierTests
{
    // country, iban, bank code, branch code, identifier (the contiguous span a bank directory keys on)
    [TestCase("DE", "DE89370400440532013000", "37040044", null, "37040044")]              // BLZ
    [TestCase("NL", "NL91ABNA0417164300", "ABNA", null, "ABNA")]
    [TestCase("NO", "NO9386011117947", "8601", null, "8601")]
    [TestCase("GB", "GB29NWBK60161331926819", "NWBK", "601613", "NWBK601613")]          // bank + sort code
    [TestCase("FR", "FR1420041010050500013M02606", "20041", "01005", "2004101005")]      // banque + guichet
    [TestCase("PT", "PT50000201231234567890154", "0002", "0123", "00020123")]
    [TestCase("IT", "IT60X0542811101000000123456", "05428", "11101", "0542811101")]       // ABI + CAB, AFTER the CIN
    [TestCase("PL", "PL61109010140000071219812874", null, "10901014", "10901014")]        // registry models PL's routing number as a branch
    public void TryParse_ExposesTheBankAndBranchIdentifiersTheRegistryDefines(
        string country, string iban, string? bank, string? branch, string identifier)
    {
        var service = new IbanValidationService();

        var ok = service.TryParse(iban, out var parsed);

        Assert.That(ok, Is.True);
        Assert.That(parsed!.Value.Country, Is.EqualTo(country));
        Assert.That(parsed.Value.BankCode, Is.EqualTo(bank));
        Assert.That(parsed.Value.BranchCode, Is.EqualTo(branch));
        Assert.That(parsed.Value.BankIdentifier, Is.EqualTo(identifier),
            "the span a bank directory is keyed on: bank and branch together where both exist, whichever exists otherwise");
    }

    [Test]
    public void TryParse_BankIdentifierIsTakenFromTheNormalizedIban()
    {
        // Spaces and case must not shift the registry positions.
        var service = new IbanValidationService();

        service.TryParse("it60 x054 2811 1010 0000 0123 456", out var parsed);

        Assert.That(parsed!.Value.BankIdentifier, Is.EqualTo("0542811101"));
    }

    [Test]
    public void TryParse_StillRefusesAnInvalidIbanRatherThanGuessingABank()
    {
        var service = new IbanValidationService();

        var ok = service.TryParse("DE00370400440532013000", out var parsed);

        Assert.That(ok, Is.False, "wrong check digits");
        Assert.That(parsed, Is.Null);
    }
}
