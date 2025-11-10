namespace Iban.Core.Tests;

[TestFixture]
public class ValidationResultTests
{
    [Test]
    public void ValidationLevel_ShouldIncludeNotValidatedValue()
    {
        // Behavior: ValidationLevel enum should have NotValidated member with value 0

        // Test its integer value is 0 (default/lowest level)
        Assert.That((int)ValidationLevel.NotValidated, Is.EqualTo(0), "NotValidated should have value 0");

        // Test it can be used in comparisons with other enum values
        Assert.That(ValidationLevel.NotValidated, Is.LessThan(ValidationLevel.Structural), "NotValidated should be less than Structural");
        Assert.That(ValidationLevel.NotValidated, Is.LessThan(ValidationLevel.AccountLevel), "NotValidated should be less than AccountLevel");

        // Test it can be assigned and compared
        ValidationLevel level = ValidationLevel.NotValidated;
        Assert.That(level, Is.EqualTo(ValidationLevel.NotValidated), "Assignment should work");
        Assert.That(level == ValidationLevel.NotValidated, Is.True, "Equality comparison should work");

        // Multiple assertions prevent gaming with simple hardcoded returns
    }

    [Test]
    public void ValidationResult_ShouldHaveSupportedLevelProperty()
    {
        // Behavior: ValidationResult should include SupportedLevel to indicate available validation for country

        // Test Success factory with different SupportedLevel values
        var successNotValidated = ValidationResult.Success(country: "XX", level: ValidationLevel.Structural, supportedLevel: ValidationLevel.NotValidated);
        Assert.That(successNotValidated.SupportedLevel, Is.EqualTo(ValidationLevel.NotValidated), "Success should preserve NotValidated SupportedLevel");
        Assert.That(successNotValidated.Level, Is.EqualTo(ValidationLevel.Structural), "Level should be independent from SupportedLevel");

        var successStructural = ValidationResult.Success(country: "DE", level: ValidationLevel.Structural, supportedLevel: ValidationLevel.Structural);
        Assert.That(successStructural.SupportedLevel, Is.EqualTo(ValidationLevel.Structural), "Success should preserve Structural SupportedLevel");

        var successAccountLevel = ValidationResult.Success(country: "GB", level: ValidationLevel.AccountLevel, supportedLevel: ValidationLevel.AccountLevel);
        Assert.That(successAccountLevel.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "Success should preserve AccountLevel SupportedLevel");

        // Test Failed factory with different SupportedLevel values
        var failedStructural = ValidationResult.Failed("ERR_FORMAT_INVALID", "Invalid format", country: "FR", level: ValidationLevel.Structural, supportedLevel: ValidationLevel.AccountLevel);
        Assert.That(failedStructural.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "Failed should preserve SupportedLevel");
        Assert.That(failedStructural.Level, Is.EqualTo(ValidationLevel.Structural), "Failed validation level should be independent");

        var failedAccountLevel = ValidationResult.Failed("ERR_ACCOUNT_INVALID", "Invalid account", country: "GB", level: ValidationLevel.AccountLevel, supportedLevel: ValidationLevel.AccountLevel);
        Assert.That(failedAccountLevel.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "Failed should preserve AccountLevel SupportedLevel");

        // Test that Level (performed) and SupportedLevel (available) can differ
        var partialValidation = ValidationResult.Success(country: "GB", level: ValidationLevel.Structural, supportedLevel: ValidationLevel.AccountLevel);
        Assert.That(partialValidation.Level, Is.EqualTo(ValidationLevel.Structural), "Level shows what validation was performed");
        Assert.That(partialValidation.SupportedLevel, Is.EqualTo(ValidationLevel.AccountLevel), "SupportedLevel shows what is available");
        Assert.That(partialValidation.Level, Is.Not.EqualTo(partialValidation.SupportedLevel), "Level and SupportedLevel should be different when partial validation performed");

        // Multiple different values prevent hardcoding any single enum value
    }
}
