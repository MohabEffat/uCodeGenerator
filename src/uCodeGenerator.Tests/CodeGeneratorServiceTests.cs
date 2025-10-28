using uCodeGenerator.Core.Services;
using Xunit;

namespace uCodeGenerator.Tests;

public class CodeGeneratorServiceTests
{
    [Fact]
    public void GenerateMany_ReturnsRequestedCount()
    {
        var svc = new UCodeGeneratorService();

        var codes = svc.GenerateMany("TJ", 5, "25");

        Assert.Equal(5, codes.Count);
    }

    [Fact]
    public void EachCode_HasLength13()
    {
        var svc = new UCodeGeneratorService();

        var code = svc.GenerateCode("TJ", "25");

        Assert.Equal(13, code.Length);
    }
}
