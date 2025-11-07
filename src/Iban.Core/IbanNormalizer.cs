namespace Iban.Core;

/// <summary>
/// Provides IBAN normalization functionality including stripping separators,
/// converting to uppercase, and trimming whitespace.
/// </summary>
public class IbanNormalizer
{
    /// <summary>
    /// Normalizes an IBAN string by:
    /// 1. Trimming leading and trailing whitespace
    /// 2. Removing common separators (spaces, hyphens)
    /// 3. Converting to uppercase
    /// 4. Validating that only alphanumeric characters remain
    /// </summary>
    /// <param name="input">The IBAN string to normalize</param>
    /// <returns>The normalized IBAN string</returns>
    /// <exception cref="ArgumentException">Thrown when invalid characters are detected after normalization</exception>
    public string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Trim whitespace
        var trimmed = input.Trim();

        // Remove separators (spaces and hyphens)
        var withoutSeparators = trimmed.Replace(" ", "").Replace("-", "");

        // Convert to uppercase
        var normalized = withoutSeparators.ToUpperInvariant();

        // Validate that only alphanumeric characters remain (A-Z, 0-9)
        foreach (char c in normalized)
        {
            if (!char.IsAsciiLetterOrDigit(c))
            {
                throw new ArgumentException(
                    $"Invalid character '{c}' detected in IBAN. Only alphanumeric characters (A-Z, 0-9) are allowed.",
                    nameof(input));
            }
        }

        return normalized;
    }
}
