# Iban.Core - IBAN Validation Library

A comprehensive IBAN (International Bank Account Number) validation library for .NET applications. This library provides robust validation, parsing, and formatting capabilities for IBANs across all supported countries.

## Overview

`Iban.Core` is a validation component that wraps and integrates three specialized packages to provide complete IBAN validation:

- **IbanNet** - High-level IBAN structure validation
- **IbanNet.Extensions.Bban** - Country-specific BBAN validation and modulus checking
- **ModulusChecker.NetCore** - UK-specific modulus validation rules

## NuGet Package

Install via NuGet Package Manager:
```bash
dotnet add package Iban.Core
```

**Package URL**: https://www.nuget.org/packages/Iban.Core/

## Features

- ✅ **Structural Validation** - Validates IBAN format, length, and check digits
- ✅ **Country-Specific Rules** - Enforces country-specific BBAN validation
- ✅ **UK Modulus Checking** - Advanced validation for UK bank accounts
- ✅ **IBAN Parsing** - Extract country code, check digits, and BBAN components
- ✅ **Flexible Input** - Accepts IBANs with or without spaces, any case
- ✅ **Dependency Injection** - Built for modern .NET DI containers

## Quick Start

### Basic Validation

```csharp
using Iban.Core;

// Create validation service
var validator = new IbanValidationService();

// Simple validation
ValidationResult result = validator.Validate("GB82 WEST 1234 5698 7654 32");
if (result.IsValid)
{
    Console.WriteLine("IBAN is valid!");
}
else
{
    Console.WriteLine($"Invalid IBAN: {result.ErrorMessage}");
}

// Quick boolean check
bool isValid = validator.IsValid("GB82WEST12345698765432");
```

### Parsing IBANs

```csharp
using Iban.Core;

var validator = new IbanValidationService();

if (validator.TryParse("DE89370400440532013000", out ParsedIban? iban))
{
    Console.WriteLine($"Country: {iban.Value.Country}");           // DE
    Console.WriteLine($"Check Digits: {iban.Value.CheckDigits}");  // 89
    Console.WriteLine($"BBAN: {iban.Value.Bban}");                 // 370400440532013000
}
```

### Advanced Validation with Account Checking

For countries with account-level validation support (e.g., UK modulus checking):

```csharp
// Performs structural + account-level validation
ValidationResult result = validator.ValidateWithAccountCheck("GB82WEST12345698765432");
```

## Dependency Injection

Register with ASP.NET Core DI container:

```csharp
using Iban.Core;
using Microsoft.Extensions.DependencyInjection;

services.AddScoped<IIbanValidationService, IbanValidationService>();
```

Then inject into your services:

```csharp
public class BankAccountService
{
    private readonly IIbanValidationService _ibanValidator;

    public BankAccountService(IIbanValidationService ibanValidator)
    {
        _ibanValidator = ibanValidator;
    }

    public bool ValidateAccount(string iban)
    {
        var result = _ibanValidator.Validate(iban);
        return result.IsValid;
    }
}
```

## API Reference

### IIbanValidationService Interface

```csharp
public interface IIbanValidationService
{
    /// <summary>
    /// Validates the provided IBAN with detailed results.
    /// </summary>
    ValidationResult Validate(string? iban);

    /// <summary>
    /// Quick boolean validation check.
    /// </summary>
    bool IsValid(string? iban);

    /// <summary>
    /// Attempts to parse IBAN into structured format.
    /// </summary>
    bool TryParse(string? iban, out ParsedIban? parsedIban);

    /// <summary>
    /// Validates with account-level checking (UK modulus, etc.).
    /// Falls back to structural validation for unsupported countries.
    /// </summary>
    ValidationResult ValidateWithAccountCheck(string? iban);
}
```

### ValidationResult

```csharp
public readonly record struct ValidationResult
{
    public bool IsValid { get; init; }
    public IbanValidationError? ErrorCode { get; init; }  // structured error code (null when valid)
    public string? ErrorMessage { get; init; }
    public string? Country { get; init; }                 // ISO 3166-1 alpha-2 country code
    public ValidationLevel Level { get; init; }           // validation actually performed
    public ValidationLevel SupportedLevel { get; init; }  // validation available for this country

    // Instances are produced via the factory methods:
    public static ValidationResult Success(string? country = null, ...);
    public static ValidationResult Failed(IbanValidationError errorCode, string errorMessage, ...);
}
```

### ParsedIban

```csharp
public readonly record struct ParsedIban
{
    public required string Country { get; init; }        // ISO 3166-1 alpha-2 country code
    public required string CheckDigits { get; init; }    // Two-digit check digits
    public required string Bban { get; init; }           // Basic Bank Account Number
    public required string NormalizedIban { get; init; } // Uppercase, no spaces
}
```

## Use Cases

### Payment Systems Integration


```csharp
public class PaymentValidator
{
    private readonly IIbanValidationService _validator;

    public ValidationResult ValidateIbanBeneficiary(string iban, string countryCode)
    {
        // Validate IBAN structure
        var result = _validator.Validate(iban);
        if (!result.IsValid)
            return result;

        // Extract country from IBAN and verify it matches expected country
        if (_validator.TryParse(iban, out ParsedIban? parsed))
        {
            if (parsed!.Value.Country != countryCode)
            {
                return ValidationResult.Failed(
                    IbanValidationError.ERR_FORMAT_COUNTRY_INVALID,
                    "IBAN country code doesn't match beneficiary country");
            }
        }

        return result;
    }
}
```

### SEPA Payment Validation

Ensure IBANs are from SEPA-participating countries:

```csharp
private static readonly HashSet<string> SepaCountries = new()
{
    "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR",
    "DE", "GR", "HU", "IS", "IE", "IT", "LV", "LI", "LT", "LU",
    "MT", "MC", "NL", "NO", "PL", "PT", "RO", "SM", "SK", "SI",
    "ES", "SE", "CH", "GB"
};

public bool IsSepaIban(string iban)
{
    if (!_validator.TryParse(iban, out ParsedIban? parsed))
        return false;

    return SepaCountries.Contains(parsed!.Value.Country);
}
```

## Supported IBAN Countries

The library supports IBANs from 70+ countries including:

- 🇬🇧 United Kingdom (with advanced modulus checking)
- 🇩🇪 Germany
- 🇫🇷 France
- 🇮🇹 Italy
- 🇪🇸 Spain
- 🇳🇱 Netherlands
- And 60+ more countries...

## Testing

The library is thoroughly tested with:
- Unit tests for all validation methods
- Country-specific test cases
- Edge cases (null, empty, malformed IBANs)
- Real-world IBAN examples

## Requirements

- .NET 8.0 or later

