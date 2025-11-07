namespace Iban.Core;

public interface IIbanValidationService
{
    bool IsValid(string? iban);
    ValidationResult Validate(string? iban);
    bool TryParse(string? iban, out ParsedIban? parsedIban);
}
