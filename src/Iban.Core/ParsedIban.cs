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

    /// <summary>
    /// Gets the bank code the IBAN registry defines for this country — a German BLZ, a Dutch
    /// four-letter code, an Italian ABI — or null where the registry defines no bank section
    /// (Poland models its whole routing number as a branch; see <see cref="BranchCode"/>).
    /// Read from the normalized IBAN at the registry's position, which is not always the
    /// start of the BBAN: Italy's ABI follows a check character.
    /// </summary>
    public string? BankCode { get; init; }

    /// <summary>
    /// Gets the branch code the IBAN registry defines for this country — a UK sort code, an
    /// Italian CAB, a French guichet, Poland's eight-digit routing number — or null where the
    /// registry defines no branch section.
    /// </summary>
    public string? BranchCode { get; init; }

    /// <summary>
    /// Gets the contiguous span that identifies the institution and branch: the part of the IBAN
    /// a bank directory is keyed on. Bank and branch together where the registry defines both
    /// (GB: <c>NWBK601613</c>, IT: ABI+CAB), whichever exists otherwise (DE: the BLZ, PL: the
    /// routing number). Null only when the registry defines neither.
    /// <para>This is structure, not identity: it says WHERE the identifier is, not whether the
    /// bank exists. Resolving it to a name or a BIC is a lookup against data that changes —
    /// a registry snapshot or a provider's live directory — and belongs to the caller.</para>
    /// </summary>
    public string? BankIdentifier { get; init; }
}
