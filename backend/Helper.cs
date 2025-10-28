namespace uCodeGenerator
{
    public static class Helper
    {
        public static readonly Dictionary<char, int> Values = new Dictionary<char, int>
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
        {
            int sum = 0;
            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];
                if (Values.TryGetValue(c, out int value))
                {
                    sum += value;
                }
            }
            return sum;
        }
        public static int CalculateChecksum(string code)
        {
            int Weight = CalculateWeights(code);

            int firDigit = Weight % 10;
            int secDigit = (Weight / 10) % 10;
            int thirdDigit = (Weight / 100);

            int sumOfModules = 0;

            int[] arrayOfValuesAndWeights = { thirdDigit, secDigit, firDigit };

            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];
                if (Values.TryGetValue(c, out int value))
                {
                    sumOfModules += value * arrayOfValuesAndWeights[i % 3];
                }
            }

            int roundedSum = (int)(Math.Ceiling(sumOfModules / 10.0) * 10);
            int checksum = roundedSum - sumOfModules;
            return checksum;
        }

    }
}
