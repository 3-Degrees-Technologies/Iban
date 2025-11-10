using IbanNet;
using IbanNet.Extensions.Bban.Validation.Rules;
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
        // Configure validator with BBAN national check digit validation
        var options = new IbanValidatorOptions();
        options.Rules.Add(new HasValidNationalCheckDigitsRule());
        _validator = new IbanValidator(options);
        _modulusChecker = new ModulusChecker();
    }

    /// <inheritdoc />
    public ValidationResult Validate(string? iban)
    {
        // Differentiate between null, empty, and whitespace inputs
        if (iban is null)
        {
            return ValidationResult.Failed(IbanValidationError.ERR_INPUT_NULL, "IBAN cannot be null");
        }
        if (iban.Length == 0)
        {
            return ValidationResult.Failed(IbanValidationError.ERR_INPUT_EMPTY, "IBAN cannot be empty");
        }
        if (string.IsNullOrWhiteSpace(iban))
        {
            return ValidationResult.Failed(IbanValidationError.ERR_INPUT_WHITESPACE, "IBAN cannot be whitespace only");
        }

        var normalized = iban.Replace(" ", "").Trim().ToUpperInvariant();
        var result = _validator.Validate(normalized);

        var country = normalized.Length >= 2 ? normalized.Substring(0, 2) : null;
        var supportedLevel = GetSupportedValidationLevel(country);

        if (result.IsValid)
        {
            return ValidationResult.Success(country, ValidationLevel.Structural, supportedLevel);
        }

        return ValidationResult.Failed(IbanValidationError.ERR_FORMAT_INVALID, result.Error?.ErrorMessage ?? "IBAN structural validation failed", country, ValidationLevel.Structural, supportedLevel);
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
    public bool TryParse(string? iban, out ParsedIban? parsedIban)
    {
        parsedIban = null;

        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        var normalized = iban.Replace(" ", "").Trim().ToUpperInvariant();
        var result = _validator.Validate(normalized);

        if (result.IsValid && normalized.Length >= 4)
        {
            parsedIban = new ParsedIban
            {
                Country = normalized.Substring(0, 2),
                CheckDigits = normalized.Substring(2, 2),
                Bban = normalized.Substring(4),
                NormalizedIban = normalized
            };
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public ValidationResult ValidateWithAccountCheck(string? iban)
    {
        // First perform structural validation (without BBAN rule)
        // Differentiate between null, empty, and whitespace inputs
        if (iban is null)
        {
            return ValidationResult.Failed(IbanValidationError.ERR_INPUT_NULL, "IBAN cannot be null");
        }
        if (iban.Length == 0)
        {
            return ValidationResult.Failed(IbanValidationError.ERR_INPUT_EMPTY, "IBAN cannot be empty");
        }
        if (string.IsNullOrWhiteSpace(iban))
        {
            return ValidationResult.Failed(IbanValidationError.ERR_INPUT_WHITESPACE, "IBAN cannot be whitespace only");
        }

        var normalized = iban.Replace(" ", "").Trim().ToUpperInvariant();

        // Basic structural validation using simple validator
        var basicValidator = new IbanValidator();
        var structuralResult = basicValidator.Validate(normalized);

        var countryCode = normalized.Length >= 2 ? normalized.Substring(0, 2) : null;
        var supportedLevel = GetSupportedValidationLevel(countryCode);

        if (!structuralResult.IsValid)
        {
            return ValidationResult.Failed(IbanValidationError.ERR_FORMAT_INVALID, structuralResult.Error?.ErrorMessage ?? "IBAN structural validation failed", countryCode, ValidationLevel.Structural, supportedLevel);
        }

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
                    IbanValidationError.ERR_ACCOUNT_MODULUS,
                    $"UK IBAN failed modulus checking for sort code {sortCode} and account number {accountNumber}",
                    countryCode,
                    ValidationLevel.AccountLevel,
                    supportedLevel);
            }

            return ValidationResult.Success(countryCode, ValidationLevel.AccountLevel, supportedLevel);
        }

        // Perform BBAN validation for supported countries (FR, IT, PT, NO, MC, MR, BA, SM)
        string[] bbanSupportedCountries = { "FR", "IT", "PT", "NO", "MC", "MR", "BA", "SM" };
        if (Array.Exists(bbanSupportedCountries, c => c == countryCode))
        {
            // Use validator with BBAN rule
            var bbanResult = _validator.Validate(normalized);
            if (!bbanResult.IsValid)
            {
                return ValidationResult.Failed(
                    IbanValidationError.ERR_ACCOUNT_BBAN,
                    $"IBAN failed BBAN national check digit validation: {bbanResult.Error?.ErrorMessage ?? "Unknown error"}",
                    countryCode,
                    ValidationLevel.AccountLevel,
                    supportedLevel);
            }

            return ValidationResult.Success(countryCode, ValidationLevel.AccountLevel, supportedLevel);
        }

        // For other countries, return structural validation success
        return ValidationResult.Success(countryCode, ValidationLevel.Structural, supportedLevel);
    }

    /// <summary>
    /// Determines the highest level of validation supported for a given country code.
    /// </summary>
    /// <param name="countryCode">The two-letter ISO country code.</param>
    /// <returns>The highest validation level available for this country.</returns>
    private ValidationLevel GetSupportedValidationLevel(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            return ValidationLevel.NotValidated;
        }

        // UK supports modulus checking (AccountLevel)
        if (countryCode == "GB")
        {
            return ValidationLevel.AccountLevel;
        }

        // Countries with BBAN national check digit support (AccountLevel)
        string[] bbanSupportedCountries = { "FR", "IT", "PT", "NO", "MC", "MR", "BA", "SM" };
        if (Array.Exists(bbanSupportedCountries, c => c == countryCode))
        {
            return ValidationLevel.AccountLevel;
        }

        // All other countries only support structural validation
        return ValidationLevel.Structural;
    }
}
