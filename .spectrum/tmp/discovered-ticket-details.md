# IBA-85 - Retrieved from Slack

**Ticket ID:** IBA-85

## Slack Messages
<@U096G0ST2HG> :dart: *New Assignment: IBA-85*

### Agent-Knowledge (1762533483.274389)

<@U096G0ST2HG> :dart: *New Assignment: IBA-85*

*Title*: IbanNet Library Integration and Validation Service
*Status*: In Progress
*Parent*: IBA-83 - IBAN Validation Component Research

---

# IbanNet Library Integration and Validation Service

*Objective*: Integrate IbanNet NuGet package (v5.19.0) and implement IBAN validation service wrapper following the architecture design from IBA-84.

*Context*: Based on research (IBA-83) and architecture design (IBA-84), implement enterprise-grade IBAN validation using the IbanNet library.

*Related Tickets*:

_ IBA-83: IBAN Validation Component Research (Complete)
_ IBA-84: IBAN Validation Service Architecture Design (Prerequisite)

*Dependencies*: IBA-84 must be completed first to provide implementation specification.

---

## Implementation Tasks

### 1\. Install IbanNet NuGet Package

Install IbanNet v5.19.0 (or latest stable):

`````bash
dotnet add package IbanNet --version 5.19.0
```````

*Verify*:

_ Package installed successfully
_ Zero dependency conflicts
_ Correct framework target (.NET 6/8)

### 2\. Implement Validation Service

Follow architecture from IBA-84 to create:

*Service Interface* (example - follow IBA-84 spec):

```````csharp
public interface IIbanValidationService
{
    bool IsValid(string iban);
    ValidationResult Validate(string iban);
    bool TryParse(string iban, out Iban parsed);
}
```````

*Service Implementation*:

_ Wrap IbanNet functionality according to IBA-84 design
_ Implement input sanitization (trim, uppercase)
_ Follow Centro error handling patterns
_ Add appropriate logging (mask IBANs per security spec)

### 3\. Dependency Injection Registration

Register IbanNet and validation service:

```````csharp
// In Program.cs or Startup.cs
services.AddIbanNet();
services.AddScoped&lt;IIbanValidationService, IbanValidationService&gt;();
```````

Follow Centro DI patterns.

### 4\. FluentValidation Integration (if applicable)

*If Centro uses FluentValidation*:

```````bash
dotnet add package IbanNet.FluentValidation --version 5.19.0
```````

Create IBAN validator rule following IBA-84 spec.

### 5\. Unit Tests (TDD Approach)

*Write tests FIRST, then implementation*:

```````csharp
[Fact]
public void IsValid_ValidIban_ReturnsTrue()
{
    // Arrange
    var service = CreateService();
    
    // Act
    var result = service.IsValid("NL91ABNA0417164300");
    
    // Assert
    result.Should().BeTrue();
}

[Fact]
public void IsValid_InvalidChecksum_ReturnsFalse()
{
    var service = CreateService();
    var result = service.IsValid("NL00ABNA0417164300"); // Bad checksum
    result.Should().BeFalse();
}

[Theory]
[InlineData("NL91ABNA0417164300")] // Netherlands
[InlineData("GB82WEST12345698765432")] // UK
[InlineData("DE89370400440532013000")] // Germany
public void IsValid_ValidIbansFromDifferentCountries_ReturnsTrue(string iban)
{
    var service = CreateService();
    result.Should().BeTrue();
}

[Theory]
[InlineData("")] // Empty
[InlineData("   ")] // Whitespace
[InlineData("INVALID")] // Invalid format
[InlineData("XX99123456789")] // Invalid country
public void IsValid_InvalidInputs_ReturnsFalse(string iban)
{
    var service = CreateService();
    var result = service.IsValid(iban);
    result.Should().BeFalse();
}
```````

*Test Coverage Areas*:

_ :white_check_mark: Valid IBANs from multiple countries
_ :white_check_mark: Invalid checksum detection
_ :white_check_mark: Invalid format detection
_ :white_check_mark: Edge cases (null, empty, whitespace)
_ :white_check_mark: Input normalization (spaces, case)
_ :white_check_mark: Error result details (if using ValidationResult)
_ :white_check_mark: Parsing functionality (if using TryParse)

### 6\. Code Quality

_ Follow Centro coding standards
_ XML documentation on public APIs
_ Proper exception handling
_ Security: Don't log full IBANs (use masking/obfuscation)
_ Thread-safety considerations

---

## Definition of Done

- [ ] IbanNet NuGet package installed (v5.19.0 or latest stable)
- [ ] Validation service implemented per IBA-84 architecture
- [ ] Dependency injection configured

### Agent-Knowledge (1762531220.417859)

<@U096G0ST2HG>: :dart: _New Assignment: IBA-85_

You have been assigned: _IbanNet Library Integration and Validation Service_

Ticket is now _In Progress_ and ready for work. View details at Linear ticket IBA-85.

