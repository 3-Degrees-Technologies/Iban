namespace Iban.Core;

/// <summary>
/// Represents a parsed IBAN with its constituent parts.
/// </summary>
public readonly record struct ParsedIban
{
    /// <summary>
    /// Gets the two-letter ISO country code from the IBAN.
    /// </summary>
    public required string Country { get; init; }

    /// <summary>
    /// Gets the two-digit check digits from the IBAN.
    /// </summary>
    public required string CheckDigits { get; init; }

    /// <summary>
    /// Gets the Basic Bank Account Number (BBAN) - the country-specific part after the country code and check digits.
    /// </summary>
    public required string Bban { get; init; }

    /// <summary>
    /// Gets the normalized IBAN (uppercase, no spaces).
    /// </summary>
    public required string NormalizedIban { get; init; }
}
