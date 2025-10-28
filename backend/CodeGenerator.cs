namespace uCodeGenerator
{
    public class uCodeGenerator
    {
        private static Random random = new Random();

        public static readonly HashSet<char> AllowedPrefixChars = new HashSet<char>
        {
            'B', 'C', 'D', 'F', 'G', 'J', 'K', 'L', 'P', 'Q', 'R', 'T', 'V'
        };

        private readonly char[] charsWithoutVowels =
        {
            'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M',
            'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Z'
        };

        private int _sequence = 0;

        public string GenerateCode(string prefix, string year)
        {
            if (_sequence > 999999)
                throw new InvalidOperationException("All possible codes have been generated.");

            string numericPart = _sequence.ToString("D6");

            Interlocked.Increment(ref _sequence);

            char[] codeChars = new char[12];

            codeChars[0] = prefix[0];

            codeChars[1] = prefix[1];

            codeChars[2] = year[0];

            codeChars[3] = year[1];

            for (int i = 0; i < 6; i++)
                codeChars[4 + i] = numericPart[i];

            codeChars[10] = charsWithoutVowels[random.Next(charsWithoutVowels.Length)];
            codeChars[11] = charsWithoutVowels[random.Next(charsWithoutVowels.Length)];

            var code = new string(codeChars);

            var checksum = Helper.CalculateChecksum(code);

            if (checksum < 0 || checksum > 9)
                throw new InvalidOperationException("Calculated checksum is out of valid range.");

            var fullCode = code + checksum;

            return fullCode;
        }
    }
    
}
