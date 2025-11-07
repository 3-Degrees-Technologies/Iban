using IbanNet;

namespace Iban.Core;

/// <summary>
/// Implementation of IBAN validation service using the IbanNet library.
/// Provides validation, detailed validation results, and parsing capabilities for IBANs.
/// </summary>
public class IbanValidationService : IIbanValidationService
{
    private readonly IbanValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="IbanValidationService"/> class.
    /// </summary>
    public IbanValidationService()
    {
        _validator = new IbanValidator();
    }

    /// <inheritdoc />
    public bool IsValid(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        var normalized = iban.Replace(" ", "").Trim().ToUpperInvariant();
        var result = _validator.Validate(normalized);
        return result.IsValid;
    }

    /// <inheritdoc />
    public ValidationResult Validate(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "IBAN cannot be null or empty"
            };
        }

        var normalized = iban.Replace(" ", "").Trim().ToUpperInvariant();
        var result = _validator.Validate(normalized);

        return new ValidationResult
        {
            IsValid = result.IsValid,
            ErrorMessage = result.IsValid ? null : result.Error?.ErrorMessage
        };
    }

    /// <inheritdoc />
    public bool TryParse(string? iban, out ParsedIban? parsedIban)
    {
        parsedIban = null;

        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        var normalized = iban.Replace(" ", "").Trim().ToUpperInvariant();
        var result = _validator.Validate(normalized);

        if (result.IsValid && normalized.Length >= 2)
        {
            parsedIban = new ParsedIban
            {
                Country = normalized.Substring(0, 2)
            };
            return true;
        }

        return false;
    }
}
