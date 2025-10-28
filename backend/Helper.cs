namespace uCodeGenerator
{
    public static class Helper
    {
        public static readonly IReadOnlyDictionary<char, int> CharacterValues = new Dictionary<char, int>
        {
            { '0', 2 },
            { '1', 21 },
            { '2', 30 },
            { '3', 10 },
            { '4', 4 },
            { '5', 3 },
            { '6', 16 },
            { '7', 5 },
            { '8', 27 },
            { '9', 28 },
            { 'B', 22 },
            { 'C', 9 },
            { 'D', 15 },
            { 'F', 23 },
            { 'G', 6 },
            { 'H', 24 },
            { 'J', 12 },
            { 'K', 18 },
            { 'L', 19 },
            { 'M', 7 },
            { 'N', 29 },
            { 'P', 20 },
            { 'Q', 11 },
            { 'R', 17 },
            { 'S', 25 },
            { 'T', 14 },
            { 'V', 26 },
            { 'W', 8 },
            { 'X', 13 },
            { 'Z', 1 }
        };
        public static int CalculateWeights(string code)
            =>  code.Where(CharacterValues.ContainsKey).Sum(c => CharacterValues[c]);

        public static int CalculateChecksum(string code)
        {
            int Weight = CalculateWeights(code);

            int WeightFirstDigit = Weight / 100;           // 1
            int WeightSecondDigit = (Weight / 10) % 10;    // 2
            int WeightThirdDigit = Weight % 10;            // 9         

            int sumOfModules = 0;

            int[] arrayOfValuesAndWeights = { WeightFirstDigit, WeightSecondDigit, WeightThirdDigit };

            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];
                if (CharacterValues.TryGetValue(c, out int value))
                {
                    sumOfModules += value * arrayOfValuesAndWeights[i % 3];
                }
            }
            int checksum = (10 - (sumOfModules % 10)) % 10;
            return checksum;
        }

        public static IResult? ValidatePrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
               return Results.BadRequest("Prefix cannot be null, empty, or whitespace.");

            if (prefix.Length != 2)
                return Results.BadRequest("Prefix must be exactly 2 characters long.");

            if (!prefix.All(c => uCodeGenerator.AllowedPrefixChars.Contains(c)))
                return Results.BadRequest("Prefix contains invalid characters.");
            return null;
        }

        public static IResult? ValidateYear(string? year)
        {
            if (string.IsNullOrWhiteSpace(year))
                return Results.BadRequest("Year is required.");

            if (year.Length != 2 || !year.All(char.IsDigit))
                return Results.BadRequest("Year must be exactly 2 digits.");

            return null;
        }
    }
}
