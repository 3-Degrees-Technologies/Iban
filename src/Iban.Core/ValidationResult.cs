namespace Iban.Core;

/// <summary>
/// Represents the result of an IBAN validation operation.
/// </summary>
public readonly record struct ValidationResult
{
    /// <summary>
    /// Gets whether the IBAN is valid.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Gets the error code if validation failed; otherwise, null.
    /// Provides a structured, consistent error key for programmatic handling.
    /// </summary>
    public IbanValidationError? ErrorCode { get; init; }

    /// <summary>
    /// Gets the error message if validation failed; otherwise, null.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the two-letter ISO country code if the IBAN was structurally valid enough to parse; otherwise, null.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Gets the level of validation that was actually performed on this IBAN.
    /// This may differ from <see cref="SupportedLevel"/> when partial validation is done.
    /// For example, structural validation may be performed even when account-level validation is supported.
    /// </summary>
    public ValidationLevel Level { get; init; }

    /// <summary>
    /// Gets the level of validation supported for this IBAN's country.
    /// Indicates what validation capabilities are available, which may differ from the actual validation performed.
    /// </summary>
    public ValidationLevel SupportedLevel { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="country">The two-letter ISO country code.</param>
    /// <param name="level">The validation level performed.</param>
    /// <param name="supportedLevel">The validation level supported for this country.</param>
    public static ValidationResult Success(string? country = null, ValidationLevel level = ValidationLevel.Structural, ValidationLevel supportedLevel = ValidationLevel.Structural) => new()
    {
        IsValid = true,
        Country = country,
        Level = level,
        SupportedLevel = supportedLevel
    };

    /// <summary>
    /// Creates a failed validation result with an error code and message.
    /// </summary>
    /// <param name="errorCode">The structured error code.</param>
    /// <param name="errorMessage">The human-readable error message.</param>
    /// <param name="country">The two-letter ISO country code if parseable.</param>
    /// <param name="level">The validation level at which the failure occurred.</param>
    /// <param name="supportedLevel">The validation level supported for this country.</param>
    public static ValidationResult Failed(IbanValidationError errorCode, string errorMessage, string? country = null, ValidationLevel level = ValidationLevel.Structural, ValidationLevel supportedLevel = ValidationLevel.Structural) => new()
    {
        IsValid = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        Country = country,
        Level = level,
        SupportedLevel = supportedLevel
    };
}

/// <summary>
/// Indicates the level of validation performed on an IBAN.
/// </summary>
public enum ValidationLevel
{
    /// <summary>
    /// No validation has been performed.
    /// </summary>
    NotValidated = 0,

    /// <summary>
    /// Basic structural validation only (format, length, check digits).
    /// </summary>
    Structural = 1,

    /// <summary>
    /// Account-level validation performed (modulus checking, BBAN validation).
    /// </summary>
    AccountLevel = 2
}
