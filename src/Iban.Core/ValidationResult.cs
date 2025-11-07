namespace Iban.Core;

/// <summary>
/// Represents the result of an IBAN validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets whether the IBAN is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the error message if validation failed; otherwise, null.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
