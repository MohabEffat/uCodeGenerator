namespace uCodeGenerator.Core.Utils;

public static class Helper
{
    // Map for checksum weights
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

    // Compute checksum modulo 10
    public static int ComputeChecksum(string codeWithoutChecksum)
    {
        int sum = 0;
        foreach (var c in codeWithoutChecksum.ToUpperInvariant())
        {
            if (!CharacterValues.TryGetValue(c, out var value))
                throw new Exception($"Unsupported character '{c}' in checksum calc.");

            sum += value;
        }

        return sum % 10;
    }

    // ---------- VALIDATION HELPERS ----------
    // These do NOT return IResult anymore.
    // They now return `null` if OK, or a string error message if invalid.

    public static string? ValidatePrefix(string? prefix, HashSet<char> allowedPrefixChars)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return "prefix is required";

        if (prefix.Length != 2)
            return "prefix must be exactly 2 characters";

        if (!prefix.All(char.IsLetter))
            return "prefix must be A-Z letters";

        // business rule: only certain letters allowed
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
