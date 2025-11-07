namespace Iban.Core;

/// <summary>
/// Service for validating and parsing International Bank Account Numbers (IBANs).
/// </summary>
public interface IIbanValidationService
{
    /// <summary>
    /// Validates whether the provided IBAN is valid.
    /// </summary>
    /// <param name="iban">The IBAN string to validate. Can include spaces and be in any case.</param>
    /// <returns>True if the IBAN is valid; otherwise, false.</returns>
    bool IsValid(string? iban);

    /// <summary>
    /// Validates the provided IBAN and returns detailed validation results.
    /// </summary>
    /// <param name="iban">The IBAN string to validate. Can include spaces and be in any case.</param>
    /// <returns>A ValidationResult containing the validation status and any error messages.</returns>
    ValidationResult Validate(string? iban);

    /// <summary>
    /// Attempts to parse the provided IBAN string into a structured format.
    /// </summary>
    /// <param name="iban">The IBAN string to parse. Can include spaces and be in any case.</param>
    /// <param name="parsedIban">When this method returns, contains the parsed IBAN if successful; otherwise, null.</param>
    /// <returns>True if the IBAN was successfully parsed; otherwise, false.</returns>
    bool TryParse(string? iban, out ParsedIban? parsedIban);
}
