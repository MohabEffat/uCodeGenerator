namespace uCodeGenerator.Core.Utils;

public static class Helper
{
    // Character weight table (keep yours here – do NOT change unless domain team tells you)
    public static readonly IReadOnlyDictionary<char, int> CharacterValues = new Dictionary<char, int>
    {
        { '0', 2 }, { '1', 21 }, { '2', 30 }, { '3', 10 }, { '4', 4 },
        { '5', 3 }, { '6', 16 }, { '7', 5 }, { '8', 27 }, { '9', 28 },

        { 'A', 18 }, { 'B', 14 }, { 'C', 19 }, { 'D', 9 },  { 'E', 8 },
        { 'F', 17 }, { 'G', 6 },  { 'H', 12 }, { 'I', 13 }, { 'J', 7 },
        { 'K', 11 }, { 'L', 20 }, { 'M', 24 }, { 'N', 26 }, { 'O', 15 },
        { 'P', 22 }, { 'Q', 29 }, { 'R', 23 }, { 'S', 25 }, { 'T', 1 },
        { 'U', 0 },  { 'V', 31 }, { 'W', 32 }, { 'X', 33 }, { 'Y', 34 },
        { 'Z', 35 },
    };

    //
    // NEW CHECKSUM LOGIC (from you)
    //
    private static int CalculateWeights(string code)
        => code.Where(CharacterValues.ContainsKey)
               .Sum(c => CharacterValues[c]);

    private static int CalculateChecksumInternal(string codeWithoutChecksum)
    {
        // 1. global weight = sum of CharacterValues for all chars
        int weight = CalculateWeights(codeWithoutChecksum);

        // Take its hundreds / tens / ones digits
        // Example: if weight = 129
        //   firstDigit  = 1
        //   secondDigit = 2
        //   thirdDigit  = 9
        int weightFirstDigit = weight / 100;
        int weightSecondDigit = (weight / 10) % 10;
        int weightThirdDigit = weight % 10;

        // We build a repeating 3-number pattern [d1, d2, d3]
        int[] multipliers = { weightFirstDigit, weightSecondDigit, weightThirdDigit };

        int sumOfModules = 0;

        for (int i = 0; i < codeWithoutChecksum.Length; i++)
        {
            char c = codeWithoutChecksum[i];
            if (CharacterValues.TryGetValue(c, out int value))
            {
                // multiply each character weight by the repeating [d1,d2,d3]
                int m = multipliers[i % 3];
                sumOfModules += value * m;
            }
        }

        // checksum digit is (10 - (sumOfModules % 10)) % 10
        int checksum = (10 - (sumOfModules % 10)) % 10;
        return checksum;
    }

    // This is the method we call everywhere else
    public static int ComputeChecksum(string codeWithoutChecksum)
        => CalculateChecksumInternal(codeWithoutChecksum);


    // ---------- VALIDATION HELPERS ----------
    // Return null if valid, or an error message string if invalid.

    public static string? ValidatePrefix(string? prefix, HashSet<char> allowedPrefixChars)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return "prefix is required";

        if (prefix.Length != 2)
            return "prefix must be exactly 2 characters";

        if (!prefix.All(char.IsLetter))
            return "prefix must be A-Z letters";

        // enforce allowed chars rule (your business rule)
        if (!allowedPrefixChars.Contains(char.ToUpperInvariant(prefix[0])) ||
            !allowedPrefixChars.Contains(char.ToUpperInvariant(prefix[1])))
            return "prefix contains invalid characters";

        return null;
    }

    public static string? ValidateCount(int? count)
    {
        if (count is null)
            return "count is required";

        if (count <= 0 || count > 1_000_000)
            return "count must be between 1 and 1,000,000";

        return null;
    }

    public static string? ValidateYear(string? year)
    {
        if (string.IsNullOrWhiteSpace(year))
            return "year is required";

        if (year.Length != 2 || !year.All(char.IsDigit))
            return "year must be exactly 2 digits";

        return null;
    }
}
