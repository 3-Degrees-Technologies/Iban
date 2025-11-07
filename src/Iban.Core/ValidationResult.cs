namespace Iban.Core;

public class ValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
}
