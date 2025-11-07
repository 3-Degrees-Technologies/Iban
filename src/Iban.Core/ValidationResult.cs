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
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets the error message if validation failed; otherwise, null.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the two-letter ISO country code if the IBAN was structurally valid enough to parse; otherwise, null.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Gets the level of validation performed.
    /// </summary>
    public ValidationLevel Level { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="country">The two-letter ISO country code.</param>
    /// <param name="level">The validation level performed.</param>
    public static ValidationResult Success(string? country = null, ValidationLevel level = ValidationLevel.Structural) => new()
    {
        IsValid = true,
        Country = country,
        Level = level
    };

    /// <summary>
    /// Creates a failed validation result with an error code and message.
    /// </summary>
    /// <param name="errorCode">The structured error code.</param>
    /// <param name="errorMessage">The human-readable error message.</param>
    /// <param name="country">The two-letter ISO country code if parseable.</param>
    /// <param name="level">The validation level at which the failure occurred.</param>
    public static ValidationResult Failed(string errorCode, string errorMessage, string? country = null, ValidationLevel level = ValidationLevel.Structural) => new()
    {
        IsValid = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        Country = country,
        Level = level
    };
}

/// <summary>
/// Indicates the level of validation performed on an IBAN.
/// </summary>
public enum ValidationLevel
{
    /// <summary>
    /// Basic structural validation only (format, length, check digits).
    /// </summary>
    Structural,

    /// <summary>
    /// Account-level validation performed (modulus checking, BBAN validation).
    /// </summary>
    AccountLevel
}
