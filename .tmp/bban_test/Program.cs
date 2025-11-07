using IbanNet;
using IbanNet.Extensions.Bban.Validation.Rules;

// Create validators
var basicValidator = new IbanValidator();

var bbanOptions = new IbanValidatorOptions();
bbanOptions.Rules.Add(new HasValidNationalCheckDigitsRule());
var bbanValidator = new IbanValidator(bbanOptions);

// Start with FR1420041010050500013M02606 which we know is valid
// French IBAN structure: FR kk BBBBB GGGGG CCCCCCCCCC KK
// Where: kk = IBAN check, BBBBB = bank, GGGGG = branch, CCCCCCCCCC = account, KK = RIB key

// Take the BBAN part and change the RIB key
string validIban = "FR1420041010050500013M02606";
string bbanPart = "20041010050500013M026";  // Without RIB key
string invalidRibKey = "99";  // Wrong RIB key (original is 06)

// Now we need to recalculate the IBAN check digits for this new BBAN
// Format: FR00 + BBAN, then calculate check digits

string testBban = bbanPart + invalidRibKey;
string ibanWithWrongCheck = "FR00" + testBban;

// Calculate proper IBAN check digit
string numericIban = "";
foreach (char c in (testBban + "FR00"))
{
    if (char.IsDigit(c))
        numericIban += c;
    else
        numericIban += (c - 'A' + 10).ToString();
}

// Calculate mod 97
var mod = System.Numerics.BigInteger.Parse(numericIban) % 97;
var checkDigits = (98 - mod).ToString("00");
string corruptedIban = "FR" + checkDigits + testBban;

Console.WriteLine($"Original valid IBAN:    {validIban}");
Console.WriteLine($"  Basic: {basicValidator.Validate(validIban).IsValid}");
Console.WriteLine($"  BBAN:  {bbanValidator.Validate(validIban).IsValid}\n");

Console.WriteLine($"Corrupted IBAN (invalid RIB key): {corruptedIban}");
var basicResult = basicValidator.Validate(corruptedIban);
var bbanResult = bbanValidator.Validate(corruptedIban);
Console.WriteLine($"  Basic (IBAN check only): {basicResult.IsValid}");
Console.WriteLine($"  BBAN (with RIB check):   {bbanResult.IsValid}");

if (basicResult.IsValid && !bbanResult.IsValid)
{
    Console.WriteLine($"\n✓ PERFECT! Use this IBAN for the test: {corruptedIban}");
    Console.WriteLine($"  BBAN Error: {bbanResult.Error?.ErrorMessage}");
}
