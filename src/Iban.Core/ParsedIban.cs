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
}
