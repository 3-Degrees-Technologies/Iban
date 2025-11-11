namespace Iban.Core;

/// <summary>
/// Provides IBAN formatting functionality for human-readable display.
/// </summary>
public class IbanFormatter
{
    /// <summary>
    /// Formats an IBAN for display by adding spaces in groups of 4 characters.
    /// This follows the common IBAN display format: AAAA BBBB CCCC DDDD...
    /// </summary>
    /// <param name="iban">The IBAN to format (normalized or with spaces)</param>
    /// <returns>The formatted IBAN with spaces every 4 characters, or the original input if invalid</returns>
    public string FormatForDisplay(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return string.Empty;
        }

        // Normalize the IBAN first (remove spaces, hyphens, convert to uppercase)
        var normalized = iban.Replace(" ", "").Replace("-", "").Trim().ToUpperInvariant();

        // If the normalized IBAN is too short (less than 4 characters), return as-is
        if (normalized.Length < 4)
        {
            return normalized;
        }

        // Format with spaces every 4 characters
        var formatted = string.Empty;
        for (int i = 0; i < normalized.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
            {
                formatted += " ";
            }
            formatted += normalized[i];
        }

        return formatted;
    }
}
