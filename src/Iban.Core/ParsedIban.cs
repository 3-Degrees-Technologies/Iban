namespace Iban.Core;

/// <summary>
/// Represents a parsed IBAN with its constituent parts.
/// </summary>
public class ParsedIban
{
    /// <summary>
    /// Gets the two-letter ISO country code from the IBAN.
    /// </summary>
    public string Country { get; init; } = string.Empty;
}
