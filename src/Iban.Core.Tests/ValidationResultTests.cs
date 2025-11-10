namespace Iban.Core.Tests;

[TestFixture]
public class ValidationResultTests
{
    [Test]
    public void ValidationLevel_ShouldIncludeNotValidatedValue()
    {
        // Behavior: ValidationLevel enum should have NotValidated member with value 0
        
        // Test enum member exists
        var notValidated = ValidationLevel.NotValidated;
        
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
}
