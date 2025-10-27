namespace uCodeGenerator
{
    public class uCodeGenerator
    {
        public static readonly HashSet<char> AllowedPrefixChars = new HashSet<char>
        {
            'B', 'C', 'D', 'F', 'G', 'J', 'K', 'L', 'P', 'Q', 'R', 'T', 'V'
        };

        private readonly char[] charsWithoutVowels =
        {
            'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M',
            'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Z'
        };

        private readonly string _year;

        private int _sequence = 0;

        public uCodeGenerator(string? year = "25")
        {
            if (string.IsNullOrWhiteSpace(year) || year!.Length != 2 || !year.All(char.IsDigit))
                throw new ArgumentException("Year must be a 2-digit string (e.g., '25').");
            _year = year;
        }

        public string GenerateCode(string prefix)
        {

            if (prefix.Length != 2 || !prefix.All(c => AllowedPrefixChars.Contains(c)))
                throw new ArgumentException("Prefix must be exactly 2 allowed characters.");

            if (_sequence > 999999)
                throw new InvalidOperationException("All possible codes have been generated.");

            string numericPart = _sequence.ToString("D6");
            _sequence++;

            Random random = new Random();

            char[] codeChars = new char[12];

            codeChars[0] = prefix[0];

            codeChars[1] = prefix[1];

            codeChars[2] = _year[0];

            codeChars[3] = _year[1];


            for (int i = 0; i < 6; i++)
                codeChars[4 + i] = numericPart[i];

            codeChars[10] = charsWithoutVowels[random.Next(charsWithoutVowels.Length)];
            codeChars[11] = charsWithoutVowels[random.Next(charsWithoutVowels.Length)];

            var code = new string(codeChars);
            var fullCode = code + Helper.CalculateChecksum(code);
            return fullCode;
        }
    }
    
}
