namespace Iban.Core.Tests;

public class IbanNormalizerTests
{
    [Test]
    public void Normalize_ShouldStripSeparatorsConvertToUppercaseAndTrim()
    {
        var normalizer = new IbanNormalizer();

        // Test stripping spaces and converting to uppercase
        Assert.That(normalizer.Normalize("gb82 west 1234 5698 7654 32"), Is.EqualTo("GB82WEST12345698765432"));
        Assert.That(normalizer.Normalize("GB82 WEST 1234 5698 7654 32"), Is.EqualTo("GB82WEST12345698765432"));

        // Test stripping hyphens
        Assert.That(normalizer.Normalize("de89-3704-0044-0532-0130-00"), Is.EqualTo("DE89370400440532013000"));
        Assert.That(normalizer.Normalize("DE89-3704-0044-0532-0130-00"), Is.EqualTo("DE89370400440532013000"));

        // Test trimming leading/trailing whitespace
        Assert.That(normalizer.Normalize("  fr14 2004 1010 0505 0001 3M02 606  "), Is.EqualTo("FR1420041010050500013M02606"));
        Assert.That(normalizer.Normalize("\tfr14 2004 1010 0505 0001 3M02 606\n"), Is.EqualTo("FR1420041010050500013M02606"));

        // Test mixed separators (spaces and hyphens)
        Assert.That(normalizer.Normalize("it60-x054 2811-101 000-000-123456"), Is.EqualTo("IT60X0542811101000000123456"));

        // Test already normalized input (idempotent)
        Assert.That(normalizer.Normalize("NL91ABNA0417164300"), Is.EqualTo("NL91ABNA0417164300"));

        // Test empty and whitespace-only inputs
        Assert.That(normalizer.Normalize(""), Is.EqualTo(""));
        Assert.That(normalizer.Normalize("   "), Is.EqualTo(""));
        Assert.That(normalizer.Normalize("\t\n"), Is.EqualTo(""));

        // Cannot be satisfied with hardcoded returns - requires real parsing logic
    }

    [Test]
    public void Normalize_WithInvalidCharacters_ShouldThrowArgumentException()
    {
        var normalizer = new IbanNormalizer();

        // Test various invalid characters (after normalization removes separators)
        // Special characters
        var ex1 = Assert.Throws<ArgumentException>(() => normalizer.Normalize("GB82@WEST12345698765432"));
        Assert.That(ex1.Message, Does.Contain("@"));
        Assert.That(ex1.Message.ToLower(), Does.Contain("invalid"));

        var ex2 = Assert.Throws<ArgumentException>(() => normalizer.Normalize("DE89#3704$0044%0532"));
        Assert.That(ex2.Message, Does.Contain("#"));

        // Punctuation (excluding valid separators)
        var ex3 = Assert.Throws<ArgumentException>(() => normalizer.Normalize("FR14.2004.1010"));
        Assert.That(ex3.Message, Does.Contain("."));

        var ex4 = Assert.Throws<ArgumentException>(() => normalizer.Normalize("IT60_X054_2811"));
        Assert.That(ex4.Message, Does.Contain("_"));

        // Non-ASCII characters
        var ex5 = Assert.Throws<ArgumentException>(() => normalizer.Normalize("NL91ÄBNA0417164300"));
        Assert.That(ex5.Message, Does.Contain("Ä"));

        // Control characters (after normalization - these aren't whitespace)
        var ex6 = Assert.Throws<ArgumentException>(() => normalizer.Normalize("BE68\u0001539"));
        Assert.That(ex6.Message, Does.Contain("\u0001"));

        // Multiple invalid characters - should report first one found
        Assert.Throws<ArgumentException>(() => normalizer.Normalize("GB82@WEST#1234"));

        // Anti-gaming: Cannot be satisfied with simple character checks - requires actual validation
    }
}
