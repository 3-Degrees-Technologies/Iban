namespace Iban.Core;

/// <summary>
/// Standardized validation error codes for IBAN validation.
/// Error codes are organized by category:
/// - 1000-1999: Format validation errors
/// - 2000-2999: Account-level validation errors
/// - 3000-3999: Input validation errors
/// </summary>
public enum IbanValidationError
{
    // Format Validation Errors (1000-1999)

    /// <summary>
    /// Basic format violation - the IBAN does not conform to the expected pattern.
    /// User-facing message: "Invalid IBAN format"
    /// Example: Contains invalid characters, incorrect structure
    /// </summary>
    ERR_FORMAT_INVALID = 1001,

    /// <summary>
    /// Incorrect length for country - the IBAN length does not match the expected length for the country code.
    /// User-facing message: "Invalid IBAN length for this country"
    /// Example: DE IBAN with 20 characters instead of 22
    /// </summary>
    ERR_FORMAT_LENGTH = 1002,

    /// <summary>
    /// MOD-97 checksum failed - the check digits do not produce a valid MOD-97 checksum.
    /// User-facing message: "Invalid IBAN check digits"
    /// Example: Checksum calculation does not equal 1
    /// </summary>
    ERR_FORMAT_CHECKSUM = 1003,

    /// <summary>
    /// Unknown country code - the country code is not recognized or supported.
    /// User-facing message: "Unrecognized country code"
    /// Example: "XX" is not a valid ISO 3166-1 alpha-2 country code
    /// </summary>
    ERR_FORMAT_COUNTRY_INVALID = 1004,

    // Account-Level Validation Errors (2000-2999)

    /// <summary>
    /// UK modulus check failed - the UK-specific modulus validation did not pass.
    /// User-facing message: "Invalid UK account details (modulus check failed)"
    /// Example: Sort code and account number combination fails modulus algorithm
    /// </summary>
    ERR_ACCOUNT_MODULUS = 2001,

    /// <summary>
    /// BBAN validation failed - the Basic Bank Account Number portion is invalid.
    /// User-facing message: "Invalid bank account number format"
    /// Example: Country-specific BBAN structure rules violated
    /// </summary>
    ERR_ACCOUNT_BBAN = 2002,

    /// <summary>
    /// Invalid UK sort code - the sort code portion of a UK IBAN is invalid.
    /// User-facing message: "Invalid UK sort code"
    /// Example: Sort code not found in valid UK bank branch registry
    /// </summary>
    ERR_ACCOUNT_SORTCODE = 2003,

    /// <summary>
    /// Invalid account number - the account number portion is invalid.
    /// User-facing message: "Invalid account number"
    /// Example: Account number contains invalid characters or incorrect length
    /// </summary>
    ERR_ACCOUNT_NUMBER = 2004,

    // Input Validation Errors (3000-3999)

    /// <summary>
    /// Null input - the provided IBAN value is null.
    /// User-facing message: "IBAN cannot be null"
    /// Example: IbanValidator.Validate(null)
    /// </summary>
    ERR_INPUT_NULL = 3001,

    /// <summary>
    /// Empty string - the provided IBAN is an empty string.
    /// User-facing message: "IBAN cannot be empty"
    /// Example: IbanValidator.Validate("")
    /// </summary>
    ERR_INPUT_EMPTY = 3002,

    /// <summary>
    /// Whitespace-only - the provided IBAN contains only whitespace characters.
    /// User-facing message: "IBAN cannot be whitespace only"
    /// Example: IbanValidator.Validate("   ")
    /// </summary>
    ERR_INPUT_WHITESPACE = 3003
}
