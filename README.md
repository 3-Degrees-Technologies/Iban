# Iban validation

This is 3-Degrees IBAN validation component.
It is essentially a wrapper to 3 separate packages:

- IbanNet (used to validate Iban as high level)
- IbanNet.Extensions.Bban (used to validate some country-specific modulus rules)
- ModulusChecker.NetCore (used to validate UK-specific modulus rules)
