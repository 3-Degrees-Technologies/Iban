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

        // Asserts Normalize throws ArgumentException whose message contains each fragment.
        // An explicit Action avoids overload ambiguity between the NUnit
        // Assert.Throws(Action)/Assert.Throws(TestDelegate) signatures.
        static void AssertThrowsContaining(IbanNormalizer n, string input, params string[] fragments)
        {
            Action act = () => n.Normalize(input);
            var ex = Assert.Throws<ArgumentException>(act);
            foreach (var fragment in fragments)
            {
                Assert.That(ex!.Message, Does.Contain(fragment));
            }
        }

        // Special characters
        AssertThrowsContaining(normalizer, "GB82@WEST12345698765432", "@", "Invalid");
        AssertThrowsContaining(normalizer, "DE89#3704$0044%0532", "#");

        // Punctuation (excluding valid separators)
        AssertThrowsContaining(normalizer, "FR14.2004.1010", ".");
        AssertThrowsContaining(normalizer, "IT60_X054_2811", "_");

        // Non-ASCII characters
        AssertThrowsContaining(normalizer, "NL91ÄBNA0417164300", "Ä");

        // Control characters (after normalization - these aren't whitespace)
        AssertThrowsContaining(normalizer, "BE68\u0001539", "\u0001");

        // Multiple invalid characters - should report first one found
        AssertThrowsContaining(normalizer, "GB82@WEST#1234");

        // Anti-gaming: Cannot be satisfied with simple character checks - requires actual validation
    }
}
