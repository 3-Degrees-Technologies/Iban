using IbanNet;

namespace Iban.Core;

public class IbanValidationService : IIbanValidationService
{
    private readonly IbanValidator _validator;

    public IbanValidationService()
    {
        _validator = new IbanValidator();
    }

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
