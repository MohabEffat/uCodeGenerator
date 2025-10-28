namespace uCodeGenerator.Core.Services;

public class UCodeGeneratorService
{
    private static readonly Random random = new Random();

    // Allowed prefix chars rule. This must match what you had in your original code.
    public static readonly HashSet<char> AllowedPrefixChars = new()
    {
        'B', 'C', 'D', 'F', 'G', 'J', 'K', 'L', 'P', 'Q', 'R', 'T', 'V'
    };

    private readonly char[] charsWithoutVowels =
    {
        'B','C','D','F','G','H','J','K','L','M',
        'N','P','Q','R','S','T','V','W','X','Z'
    };

    public string GenerateCode(string prefix, string year)
    {
        var randomBlock = new string(Enumerable.Range(0, 8)
            .Select(_ => charsWithoutVowels[random.Next(charsWithoutVowels.Length)])
            .ToArray());

        var partial = $"{prefix}{randomBlock}{year}";
        var checksum = Utils.Helper.ComputeChecksum(partial);

        return partial + checksum.ToString();
    }

    public List<string> GenerateMany(string prefix, int count, string year)
    {
        var list = new List<string>(capacity: count);
        for (int i = 0; i < count; i++)
        {
            list.Add(GenerateCode(prefix, year));
        }
        return list;
    }
}
