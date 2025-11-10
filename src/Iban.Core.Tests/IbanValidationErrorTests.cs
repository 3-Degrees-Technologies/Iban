using Iban.Core;

namespace Iban.Core.Tests;

public class IbanValidationErrorTests
{
    [Test]
    public void IbanValidationError_ShouldContainAll11ErrorCodesWithCorrectValues()
    {
        // Format Validation Errors (1000-1999)
        Assert.That((int)IbanValidationError.ERR_FORMAT_INVALID, Is.EqualTo(1001));
        Assert.That((int)IbanValidationError.ERR_FORMAT_LENGTH, Is.EqualTo(1002));
        Assert.That((int)IbanValidationError.ERR_FORMAT_CHECKSUM, Is.EqualTo(1003));
        Assert.That((int)IbanValidationError.ERR_FORMAT_COUNTRY_INVALID, Is.EqualTo(1004));
        
        // Account-Level Validation Errors (2000-2999)
        Assert.That((int)IbanValidationError.ERR_ACCOUNT_MODULUS, Is.EqualTo(2001));
        Assert.That((int)IbanValidationError.ERR_ACCOUNT_BBAN, Is.EqualTo(2002));
        Assert.That((int)IbanValidationError.ERR_ACCOUNT_SORTCODE, Is.EqualTo(2003));
        Assert.That((int)IbanValidationError.ERR_ACCOUNT_NUMBER, Is.EqualTo(2004));
        
        // Input Validation Errors (3000-3999)
        Assert.That((int)IbanValidationError.ERR_INPUT_NULL, Is.EqualTo(3001));
        Assert.That((int)IbanValidationError.ERR_INPUT_EMPTY, Is.EqualTo(3002));
        Assert.That((int)IbanValidationError.ERR_INPUT_WHITESPACE, Is.EqualTo(3003));
    }
    
    [Test]
    public void IbanValidationError_ShouldHaveExactly11Values()
    {
        var allValues = Enum.GetValues<IbanValidationError>();
        Assert.That(allValues.Length, Is.EqualTo(11));
    }
    
    [Test]
    public void IbanValidationError_ShouldOrganizeErrorsIntoCategoriesByRange()
    {
        // Format errors should be in 1000-1999 range
        Assert.That((int)IbanValidationError.ERR_FORMAT_INVALID, Is.InRange(1000, 1999));
        Assert.That((int)IbanValidationError.ERR_FORMAT_LENGTH, Is.InRange(1000, 1999));
        Assert.That((int)IbanValidationError.ERR_FORMAT_CHECKSUM, Is.InRange(1000, 1999));
        Assert.That((int)IbanValidationError.ERR_FORMAT_COUNTRY_INVALID, Is.InRange(1000, 1999));
        
        // Account errors should be in 2000-2999 range
        Assert.That((int)IbanValidationError.ERR_ACCOUNT_MODULUS, Is.InRange(2000, 2999));
        Assert.That((int)IbanValidationError.ERR_ACCOUNT_BBAN, Is.InRange(2000, 2999));
        Assert.That((int)IbanValidationError.ERR_ACCOUNT_SORTCODE, Is.InRange(2000, 2999));
        Assert.That((int)IbanValidationError.ERR_ACCOUNT_NUMBER, Is.InRange(2000, 2999));
        
        // Input errors should be in 3000-3999 range
        Assert.That((int)IbanValidationError.ERR_INPUT_NULL, Is.InRange(3000, 3999));
        Assert.That((int)IbanValidationError.ERR_INPUT_EMPTY, Is.InRange(3000, 3999));
        Assert.That((int)IbanValidationError.ERR_INPUT_WHITESPACE, Is.InRange(3000, 3999));
    }
}
