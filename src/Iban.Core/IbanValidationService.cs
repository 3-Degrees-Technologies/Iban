using IbanNet;
using IbanNet.Extensions.Bban.Validation.Rules;
using IbanNet.Validation.Results;
using ModulusChecking;

namespace Iban.Core;

/// <summary>
/// Implementation of IBAN validation service using the IbanNet library.
/// Provides validation, detailed validation results, and parsing capabilities for IBANs.
/// </summary>
public class IbanValidationService : IIbanValidationService
{
    /// <summary>
    /// Countries for which IbanNet provides national (BBAN) check-digit validation
    /// in addition to structural validation.
    /// </summary>
    private static readonly string[] BbanSupportedCountries =
        { "FR", "IT", "PT", "NO", "MC", "MR", "BA", "SM" };

    /// <summary>
    /// Validator performing structural validation only (format, length, MOD-97 check digits).
    /// </summary>
    private readonly IbanValidator _structuralValidator;

    /// <summary>
    /// Validator that additionally enforces national (BBAN) check digits. Used only for
    /// account-level validation of <see cref="BbanSupportedCountries"/>.
    /// </summary>
    private readonly IbanValidator _bbanValidator;

    private readonly ModulusChecker _modulusChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="IbanValidationService"/> class.
    /// </summary>
    public IbanValidationService()
    {
        _structuralValidator = new IbanValidator();

        var bbanOptions = new IbanValidatorOptions();
        bbanOptions.Rules.Add(new HasValidNationalCheckDigitsRule());
        _bbanValidator = new IbanValidator(bbanOptions);

        _modulusChecker = new ModulusChecker();
    }

    /// <inheritdoc />
    public ValidationResult Validate(string? iban)
    {
        var inputError = ValidateInput(iban);
        if (inputError is not null)
        {
            return inputError.Value;
        }

        var normalized = Normalize(iban!);
        var result = _structuralValidator.Validate(normalized);

        var country = ExtractCountry(normalized);
        var supportedLevel = GetSupportedValidationLevel(country);

        if (result.IsValid)
        {
            return ValidationResult.Success(country, ValidationLevel.Structural, supportedLevel);
        }

        return ValidationResult.Failed(
            MapStructuralError(result.Error),
            result.Error?.ErrorMessage ?? "IBAN structural validation failed",
            country,
            ValidationLevel.Structural,
            supportedLevel);
    }

    /// <inheritdoc />
    public bool IsValid(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        return _structuralValidator.Validate(Normalize(iban)).IsValid;
    }

    /// <inheritdoc />
    public bool TryParse(string? iban, out ParsedIban? parsedIban)
    {
        parsedIban = null;

        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        var normalized = Normalize(iban);
        var result = _structuralValidator.Validate(normalized);

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
        var inputError = ValidateInput(iban);
        if (inputError is not null)
        {
            return inputError.Value;
        }

        var normalized = Normalize(iban!);

        // Structural validation first (without national check-digit rules).
        var structuralResult = _structuralValidator.Validate(normalized);

        var countryCode = ExtractCountry(normalized);
        var supportedLevel = GetSupportedValidationLevel(countryCode);

        if (!structuralResult.IsValid)
        {
            return ValidationResult.Failed(
                MapStructuralError(structuralResult.Error),
                structuralResult.Error?.ErrorMessage ?? "IBAN structural validation failed",
                countryCode,
                ValidationLevel.Structural,
                supportedLevel);
        }

        // UK modulus checking for GB IBANs.
        if (countryCode == "GB")
        {
            // GB IBAN structure: GBkk BBBB SSSSSS AAAAAAAA
            // Where kk=check digits, BBBB=bank code, SSSSSS=sort code, AAAAAAAA=account number.
            var sortCode = normalized.Substring(8, 6);
            var accountNumber = normalized.Substring(14, 8);

            var modulusCheckPassed = _modulusChecker.CheckBankAccount(sortCode, accountNumber);
            if (!modulusCheckPassed)
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

        // National (BBAN) check-digit validation for supported countries.
        if (Array.Exists(BbanSupportedCountries, c => c == countryCode))
        {
            var bbanResult = _bbanValidator.Validate(normalized);
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

        // No account-level validation available; structural validation succeeded.
        return ValidationResult.Success(countryCode, ValidationLevel.Structural, supportedLevel);
    }

    /// <summary>
    /// Validates the raw input string, distinguishing null, empty, and whitespace-only inputs.
    /// </summary>
    /// <returns>A failed <see cref="ValidationResult"/> when the input is unusable; otherwise null.</returns>
    private static ValidationResult? ValidateInput(string? iban) => iban switch
    {
        null => ValidationResult.Failed(IbanValidationError.ERR_INPUT_NULL, "IBAN cannot be null"),
        { Length: 0 } => ValidationResult.Failed(IbanValidationError.ERR_INPUT_EMPTY, "IBAN cannot be empty"),
        _ when string.IsNullOrWhiteSpace(iban) =>
            ValidationResult.Failed(IbanValidationError.ERR_INPUT_WHITESPACE, "IBAN cannot be whitespace only"),
        _ => null
    };

    /// <summary>
    /// Normalizes an IBAN for validation: trims surrounding whitespace, removes spaces, and uppercases.
    /// </summary>
    private static string Normalize(string iban) => iban.Trim().Replace(" ", "").ToUpperInvariant();

    /// <summary>
    /// Extracts the two-letter country code from a normalized IBAN, or null if too short.
    /// </summary>
    private static string? ExtractCountry(string normalized) =>
        normalized.Length >= 2 ? normalized.Substring(0, 2) : null;

    /// <summary>
    /// Maps an IbanNet structural validation error to a standardized <see cref="IbanValidationError"/> code.
    /// </summary>
    private static IbanValidationError MapStructuralError(ErrorResult? error) => error switch
    {
        InvalidLengthResult => IbanValidationError.ERR_FORMAT_LENGTH,
        InvalidCheckDigitsResult => IbanValidationError.ERR_FORMAT_CHECKSUM,
        _ => IbanValidationError.ERR_FORMAT_INVALID
    };

    /// <summary>
    /// Determines the highest level of validation supported for a given country code.
    /// </summary>
    /// <param name="countryCode">The two-letter ISO country code.</param>
    /// <returns>The highest validation level available for this country.</returns>
    private static ValidationLevel GetSupportedValidationLevel(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            return ValidationLevel.NotValidated;
        }

        // UK supports modulus checking, and BBAN countries support national check digits.
        if (countryCode == "GB" || Array.Exists(BbanSupportedCountries, c => c == countryCode))
        {
            return ValidationLevel.AccountLevel;
        }

        return ValidationLevel.Structural;
    }
}
