using IbanNet;
using ModulusChecking;

namespace Iban.Core;

/// <summary>
/// Implementation of IBAN validation service using the IbanNet library.
/// Provides validation, detailed validation results, and parsing capabilities for IBANs.
/// </summary>
public class IbanValidationService : IIbanValidationService
{
    private readonly IbanValidator _validator;
    private readonly ModulusChecker _modulusChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="IbanValidationService"/> class.
    /// </summary>
    public IbanValidationService()
    {
        _validator = new IbanValidator();
        _modulusChecker = new ModulusChecker();
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
            return ValidationResult.Failed("ERR_NULL_OR_EMPTY", "IBAN cannot be null or empty");
        }

        var normalized = iban.Replace(" ", "").Trim().ToUpperInvariant();
        var result = _validator.Validate(normalized);

        if (result.IsValid)
        {
            return ValidationResult.Success();
        }

        return ValidationResult.Failed("ERR_STRUCTURAL_INVALID", result.Error?.ErrorMessage ?? "IBAN structural validation failed");
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

    /// <inheritdoc />
    public ValidationResult ValidateWithAccountCheck(string? iban)
    {
        // First perform structural validation
        var structuralResult = Validate(iban);
        if (!structuralResult.IsValid)
        {
            return structuralResult;
        }

        var normalized = iban!.Replace(" ", "").Trim().ToUpperInvariant();
        var countryCode = normalized.Substring(0, 2);

        // Perform UK modulus checking for GB IBANs
        if (countryCode == "GB")
        {
            // GB IBAN structure: GBkk BBBB SSSSSS AAAAAAAA
            // Where kk=check digits, BBBB=bank code, SSSSSS=sort code, AAAAAAAA=account number
            // Extract sort code (positions 8-13) and account number (positions 14-21)
            var sortCode = normalized.Substring(8, 6);
            var accountNumber = normalized.Substring(14, 8);

            var modulusCheckResult = _modulusChecker.CheckBankAccount(sortCode, accountNumber);
            if (!modulusCheckResult)
            {
                return ValidationResult.Failed(
                    "ERR_ACCOUNT_INVALID_UK_MODULUS",
                    $"UK IBAN failed modulus checking for sort code {sortCode} and account number {accountNumber}");
            }
        }

        // For other countries or successful checks, return success
        return ValidationResult.Success();
    }
}
