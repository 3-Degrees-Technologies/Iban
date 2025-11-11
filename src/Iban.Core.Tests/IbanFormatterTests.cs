namespace Iban.Core.Tests;

public class IbanFormatterTests
{
    [Test]
    public void FormatForDisplay_WithNormalizedIban_ShouldFormatWithSpaces()
    {
        var formatter = new IbanFormatter();

        // Standard valid IBANs (already normalized)
        Assert.That(formatter.FormatForDisplay("GB82WEST12345698765432"), Is.EqualTo("GB82 WEST 1234 5698 7654 32"));
        Assert.That(formatter.FormatForDisplay("DE89370400440532013000"), Is.EqualTo("DE89 3704 0044 0532 0130 00"));
        Assert.That(formatter.FormatForDisplay("FR1420041010050500013M02606"), Is.EqualTo("FR14 2004 1010 0505 0001 3M02 606"));
        Assert.That(formatter.FormatForDisplay("IT60X0542811101000000123456"), Is.EqualTo("IT60 X054 2811 1010 0000 0123 456"));
        Assert.That(formatter.FormatForDisplay("NL91ABNA0417164300"), Is.EqualTo("NL91 ABNA 0417 1643 00"));
        
        // Shorter IBANs
        Assert.That(formatter.FormatForDisplay("NO9386011117947"), Is.EqualTo("NO93 8601 1117 947"));
        
        // Longer IBANs
        Assert.That(formatter.FormatForDisplay("MT84MALT011000012345MTLCAST001S"), Is.EqualTo("MT84 MALT 0110 0001 2345 MTLC AST0 01S"));
    }

    [Test]
    public void FormatForDisplay_WithAlreadyFormattedIban_ShouldNormalizeAndReformat()
    {
        var formatter = new IbanFormatter();

        // IBANs that already have spaces - should be re-normalized and formatted
        Assert.That(formatter.FormatForDisplay("GB82 WEST 1234 5698 7654 32"), Is.EqualTo("GB82 WEST 1234 5698 7654 32"));
        Assert.That(formatter.FormatForDisplay("DE89 3704 0044 0532 0130 00"), Is.EqualTo("DE89 3704 0044 0532 0130 00"));
        
        // Irregularly spaced IBANs - should be normalized and properly formatted
        Assert.That(formatter.FormatForDisplay("GB82WEST 12345698765432"), Is.EqualTo("GB82 WEST 1234 5698 7654 32"));
        Assert.That(formatter.FormatForDisplay("DE89 37040044 0532013000"), Is.EqualTo("DE89 3704 0044 0532 0130 00"));
    }

    [Test]
    public void FormatForDisplay_WithHyphensAndMixedSeparators_ShouldNormalizeAndFormat()
    {
        var formatter = new IbanFormatter();

        // IBANs with hyphens
        Assert.That(formatter.FormatForDisplay("DE89-3704-0044-0532-0130-00"), Is.EqualTo("DE89 3704 0044 0532 0130 00"));
        
        // Mixed separators (spaces and hyphens)
        Assert.That(formatter.FormatForDisplay("IT60-X054 2811-1010 000-00-123456"), Is.EqualTo("IT60 X054 2811 1010 0000 0123 456"));
    }

    [Test]
    public void FormatForDisplay_WithLowercaseInput_ShouldConvertToUppercase()
    {
        var formatter = new IbanFormatter();

        // Lowercase IBANs should be converted to uppercase
        Assert.That(formatter.FormatForDisplay("gb82west12345698765432"), Is.EqualTo("GB82 WEST 1234 5698 7654 32"));
        Assert.That(formatter.FormatForDisplay("de89370400440532013000"), Is.EqualTo("DE89 3704 0044 0532 0130 00"));
        
        // Mixed case
        Assert.That(formatter.FormatForDisplay("Fr1420041010050500013M02606"), Is.EqualTo("FR14 2004 1010 0505 0001 3M02 606"));
    }

    [Test]
    public void FormatForDisplay_WithWhitespaceAround_ShouldTrimAndFormat()
    {
        var formatter = new IbanFormatter();

        // Leading/trailing whitespace
        Assert.That(formatter.FormatForDisplay("  GB82WEST12345698765432  "), Is.EqualTo("GB82 WEST 1234 5698 7654 32"));
        Assert.That(formatter.FormatForDisplay("\tDE89370400440532013000\n"), Is.EqualTo("DE89 3704 0044 0532 0130 00"));
        Assert.That(formatter.FormatForDisplay("  FR14 2004 1010 0505 0001 3M02 606  "), Is.EqualTo("FR14 2004 1010 0505 0001 3M02 606"));
    }

    [Test]
    public void FormatForDisplay_WithNullOrEmpty_ShouldReturnEmptyString()
    {
        var formatter = new IbanFormatter();

        // Null input
        Assert.That(formatter.FormatForDisplay(null), Is.EqualTo(string.Empty));
        
        // Empty string
        Assert.That(formatter.FormatForDisplay(""), Is.EqualTo(string.Empty));
        
        // Whitespace-only inputs
        Assert.That(formatter.FormatForDisplay("   "), Is.EqualTo(string.Empty));
        Assert.That(formatter.FormatForDisplay("\t"), Is.EqualTo(string.Empty));
        Assert.That(formatter.FormatForDisplay("\n"), Is.EqualTo(string.Empty));
        Assert.That(formatter.FormatForDisplay("  \t\n  "), Is.EqualTo(string.Empty));
    }

    [Test]
    public void FormatForDisplay_WithShortInput_ShouldReturnNormalizedWithoutSpaces()
    {
        var formatter = new IbanFormatter();

        // Less than 4 characters - no formatting
        Assert.That(formatter.FormatForDisplay("GB"), Is.EqualTo("GB"));
        Assert.That(formatter.FormatForDisplay("DE8"), Is.EqualTo("DE8"));
        Assert.That(formatter.FormatForDisplay("gb"), Is.EqualTo("GB"));
        
        // With separators that normalize to less than 4 characters
        Assert.That(formatter.FormatForDisplay("G B"), Is.EqualTo("GB"));
        Assert.That(formatter.FormatForDisplay("D-E-8"), Is.EqualTo("DE8"));
    }

    [Test]
    public void FormatForDisplay_WithExactly4Characters_ShouldReturnWithoutSpaces()
    {
        var formatter = new IbanFormatter();

        // Exactly 4 characters - no space needed
        Assert.That(formatter.FormatForDisplay("GB82"), Is.EqualTo("GB82"));
        Assert.That(formatter.FormatForDisplay("de89"), Is.EqualTo("DE89"));
        Assert.That(formatter.FormatForDisplay("G B 8 2"), Is.EqualTo("GB82"));
    }

    [Test]
    public void FormatForDisplay_WithExactly5Characters_ShouldFormatCorrectly()
    {
        var formatter = new IbanFormatter();

        // 5 characters - should have one space after 4th character
        Assert.That(formatter.FormatForDisplay("GB82W"), Is.EqualTo("GB82 W"));
        Assert.That(formatter.FormatForDisplay("DE893"), Is.EqualTo("DE89 3"));
    }

    [Test]
    public void FormatForDisplay_WithInvalidCharacters_ShouldStillFormat()
    {
        var formatter = new IbanFormatter();

        // The formatter doesn't validate - it just formats
        // Invalid characters should still be formatted with spaces
        Assert.That(formatter.FormatForDisplay("GB82@WEST12345698765432"), Is.EqualTo("GB82 @WES T123 4569 8765 432"));
        Assert.That(formatter.FormatForDisplay("DE89#3704$0044%0532"), Is.EqualTo("DE89 #370 4$00 44%0 532"));
        Assert.That(formatter.FormatForDisplay("INVALID"), Is.EqualTo("INVA LID"));
    }

    [Test]
    public void FormatForDisplay_WithNumericOnlyInput_ShouldFormat()
    {
        var formatter = new IbanFormatter();

        // Purely numeric strings (invalid IBANs but should still format)
        Assert.That(formatter.FormatForDisplay("1234567890123456"), Is.EqualTo("1234 5678 9012 3456"));
    }

    [Test]
    public void FormatForDisplay_IsIdempotent_WhenCalledMultipleTimes()
    {
        var formatter = new IbanFormatter();

        var iban = "GB82WEST12345698765432";
        var firstFormat = formatter.FormatForDisplay(iban);
        var secondFormat = formatter.FormatForDisplay(firstFormat);
        var thirdFormat = formatter.FormatForDisplay(secondFormat);

        // Should stabilize to the same formatted output
        Assert.That(firstFormat, Is.EqualTo("GB82 WEST 1234 5698 7654 32"));
        Assert.That(secondFormat, Is.EqualTo(firstFormat));
        Assert.That(thirdFormat, Is.EqualTo(firstFormat));
    }

    [Test]
    public void FormatForDisplay_WithRealWorldExamples_ShouldFormatCorrectly()
    {
        var formatter = new IbanFormatter();

        // Real-world IBAN examples from various countries
        Assert.That(formatter.FormatForDisplay("GB29NWBK60161331926819"), Is.EqualTo("GB29 NWBK 6016 1331 9268 19"));
        Assert.That(formatter.FormatForDisplay("DE44500105175407324931"), Is.EqualTo("DE44 5001 0517 5407 3249 31"));
        Assert.That(formatter.FormatForDisplay("FR1420041010050500013M02606"), Is.EqualTo("FR14 2004 1010 0505 0001 3M02 606"));
        Assert.That(formatter.FormatForDisplay("IT60X0542811101000000123456"), Is.EqualTo("IT60 X054 2811 1010 0000 0123 456"));
        Assert.That(formatter.FormatForDisplay("ES9121000418450200051332"), Is.EqualTo("ES91 2100 0418 4502 0005 1332"));
        Assert.That(formatter.FormatForDisplay("CH9300762011623852957"), Is.EqualTo("CH93 0076 2011 6238 5295 7"));
        Assert.That(formatter.FormatForDisplay("BE68539007547034"), Is.EqualTo("BE68 5390 0754 7034"));
    }
}
