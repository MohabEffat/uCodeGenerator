using CodeService;
using System.Reflection;
using Xunit;

namespace CodeServiceTests
{
    public class CodeGeneratorTests
    {
        [Fact]
        public void GenerateCode_ShouldReturn_Valid13CharacterCode()
        {
            // Arrange
            var generator = new uCodeGenerator();
            string prefix = "BR";
            string year = "25";

            // Act
            string code = generator.GenerateCode(prefix, year);

            // Assert
            Xunit.Assert.NotNull(code);
            Xunit.Assert.Equal(13, code.Length); 

            // Prefix check
            Xunit.Assert.StartsWith(prefix, code);
            Xunit.Assert.Equal(2, prefix.Length);

            Xunit.Assert.All(prefix, c =>
            Xunit.Assert.Contains(c, uCodeGenerator.AllowedPrefixChars));

            Xunit.Assert.Equal(year, code.Substring(2, 2));

            string numericPart = code.Substring(4, 6);
            Xunit.Assert.True(int.TryParse(numericPart, out _), "Numeric part must be digits only.");

            // UID check (positions 10–11)
            string uid = code.Substring(10, 2);
            Xunit.Assert.True(uid.All(char.IsLetter), "UID must be letters.");

            // Checksum validation
            char checksum = code[^1];
            int expectedChecksum = Helper.CalculateChecksum(code.Substring(0, 12));
            Xunit.Assert.Equal(expectedChecksum.ToString()[0], checksum);
        }

        [Fact]
        public void GenerateCode_ShouldThrow_WhenSequenceExceedsLimit()
        {
            // Arrange
            var generator = new uCodeGenerator();

            // Simulate limit reached
            typeof(uCodeGenerator)
                .GetField("_sequence", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(generator, 1_000_000);

            // Act & Assert
            Xunit.Assert.Throws<InvalidOperationException>(() => generator.GenerateCode("BR", "25"));
        }

        [Theory]
        [InlineData("BR", "25")]
        public void GenerateCode_ShouldNotThrow_OnValidInputs(string prefix, string year)
        {
            var generator = new uCodeGenerator();
            var code = generator.GenerateCode(prefix, year);
            Xunit.Assert.NotNull(code);
        }

    }
}
