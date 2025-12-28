using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Tests;

public sealed class TreeOptionsTests
{
    [Fact]
    public void ShouldIncludeFile_Should_RespectExcludeEvenWhenIncludeMatches()
    {
        var options = TreeOptions.Default with
        {
            IncludeFilePatterns = new[] { "src/*.cs" },
            ExcludeFilePatterns = new[] { "src/Program.cs" },
        };

        Assert.False(options.ShouldIncludeFile("src/Program.cs"));
    }

    [Fact]
    public void ShouldIncludeFile_Should_MatchExtensions_IgnoringCaseAndDot()
    {
        var options = TreeOptions.Default with
        {
            IncludeExtensions = new[] { "CS" },
        };

        Assert.True(options.ShouldIncludeFile("src/Program.cs"));
        Assert.False(options.ShouldIncludeFile("README.md"));
    }

    [Fact]
    public void ShouldIncludeFile_Should_NormalizeBackslashes()
    {
        var options = TreeOptions.Default with
        {
            IncludeFilePatterns = new[] { "src/Program.cs" },
        };

        Assert.True(options.ShouldIncludeFile("src\\Program.cs"));
    }

    [Fact]
    public void ShouldIncludeDirectory_Should_ReturnTrue_ForRoot()
    {
        var options = TreeOptions.Default with
        {
            IncludeDirectoryPatterns = new[] { "src" },
        };

        Assert.True(options.ShouldIncludeDirectory(string.Empty));
    }

    [Fact]
    public void ShouldIncludeDirectory_Should_ExcludeWhenPatternMatches()
    {
        var options = TreeOptions.Default with
        {
            IncludeDirectoryPatterns = new[] { "tests" },
            ExcludeDirectoryPatterns = new[] { "tests" },
        };

        Assert.False(options.ShouldIncludeDirectory("tests"));
    }

    [Fact]
    public void ShouldIncludeDirectory_Should_RequireMatchWhenIncludeSpecified()
    {
        var options = TreeOptions.Default with
        {
            IncludeDirectoryPatterns = new[] { "src", "src/**" },
        };

        Assert.True(options.ShouldIncludeDirectory("src"));
        Assert.True(options.ShouldIncludeDirectory("src/nested"));
        Assert.False(options.ShouldIncludeDirectory("tests"));
    }

    [Fact]
    public void ShouldIncludeLevel_Should_RespectMaxDepth()
    {
        var options = TreeOptions.Default with
        {
            MaxDepth = 1,
        };

        Assert.True(options.ShouldIncludeLevel(0));
        Assert.True(options.ShouldIncludeLevel(1));
        Assert.False(options.ShouldIncludeLevel(2));
    }
}
