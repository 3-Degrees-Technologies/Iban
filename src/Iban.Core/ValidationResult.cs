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
    /// Gets the error code if validation failed; otherwise, null.
    /// Provides a structured, consistent error key for programmatic handling.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets the error message if validation failed; otherwise, null.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with an error code and message.
    /// </summary>
    /// <param name="errorCode">The structured error code.</param>
    /// <param name="errorMessage">The human-readable error message.</param>
    public static ValidationResult Failed(string errorCode, string errorMessage) => new()
    {
        IsValid = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage
    };
}
