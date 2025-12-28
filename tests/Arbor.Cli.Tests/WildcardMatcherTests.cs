using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Tests;

public sealed class WildcardMatcherTests
{
    [Fact]
    public void ShouldIncludeFile_Should_HandleAsteriskAndQuestionMarkPatterns()
    {
        var options = TreeOptions.Default with
        {
            IncludeFilePatterns = new[] { "src/*.cs", "file?.txt" },
        };

        Assert.True(options.ShouldIncludeFile("src/Program.cs"));
        Assert.True(options.ShouldIncludeFile("file1.txt"));
        Assert.False(options.ShouldIncludeFile("file10.txt"));
    }

    [Fact]
    public void ShouldIncludeFile_Should_BeCaseInsensitive()
    {
        var options = TreeOptions.Default with
        {
            IncludeFilePatterns = new[] { "readme.MD" },
        };

        Assert.True(options.ShouldIncludeFile("README.md"));
    }
}
