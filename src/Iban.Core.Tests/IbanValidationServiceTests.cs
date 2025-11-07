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

        // Act & Assert - Invalid IBANs (bad checksum)
        Assert.That(service.IsValid("NL00ABNA0417164300"), Is.False, "Invalid checksum should return false");
        Assert.That(service.IsValid("GB00WEST12345698765432"), Is.False, "Invalid checksum should return false");

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
}
