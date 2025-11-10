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
        Assert.That(invalidResult1.ErrorCode, Is.EqualTo("ERR_ACCOUNT_INVALID_UK_MODULUS"), "Should return UK modulus error code");
        Assert.That(invalidResult1.ErrorMessage, Is.Not.Null.And.Not.Empty, "Should provide error message");

        var invalidResult2 = service.ValidateWithAccountCheck("GB68CITI18500483515538");
        Assert.That(invalidResult2.IsValid, Is.False, "Second UK IBAN with invalid modulus should fail");
        Assert.That(invalidResult2.ErrorCode, Is.EqualTo("ERR_ACCOUNT_INVALID_UK_MODULUS"), "Should return UK modulus error code");

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
        Assert.That(invalidBban.ErrorCode, Is.EqualTo("ERR_ACCOUNT_INVALID_BBAN"), "Should return BBAN error code");
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
        Assert.That(ukFailed.ErrorCode, Is.EqualTo("ERR_ACCOUNT_INVALID_UK_MODULUS"));

        // Act - Failed BBAN validation
        var frFailed = service.ValidateWithAccountCheck("FR2520041010050500013M02699");

        // Assert
        Assert.That(frFailed.IsValid, Is.False);
        Assert.That(frFailed.Country, Is.EqualTo("FR"), "Failed result should include country");
        Assert.That(frFailed.Level, Is.EqualTo(ValidationLevel.AccountLevel), "Failure occurred at account level");
        Assert.That(frFailed.ErrorCode, Is.EqualTo("ERR_ACCOUNT_INVALID_BBAN"));
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
}
