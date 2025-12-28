using Arbor.Cli.Interactive;
using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Tests;

public sealed class FilterCommandEmitterTests
{
    [Fact]
    public void CreateFilterCommand_Should_Include_RootPath_When_Provided()
    {
        var root = BuildSampleTree();
        var options = TreeOptions.Default with { IncludeFiles = true };

        var command = FilterCommandEmitter.CreateFilterCommand([
            "README.md"
        ], options, root, "C:\\Work\\Arbor CLI");

        Assert.NotNull(command);
        Assert.Contains("\"C:\\Work\\Arbor CLI\"", command);
    }

    [Fact]
    public void CreateFilterCommand_Should_Use_IncludeDir_For_Selected_Directory()
    {
        var root = BuildSampleTree();
        var options = TreeOptions.Default;

        var command = FilterCommandEmitter.CreateFilterCommand([
            "src"
        ], options, root, "C:\\Repo");

        Assert.NotNull(command);
        Assert.Contains("--include-dir src", command);
        Assert.DoesNotContain("--include src", command);
    }

    [Fact]
    public void CreateFilterCommand_Should_Add_Ancestor_Directories_For_Deep_File()
    {
        var root = BuildSampleTree();
        var options = TreeOptions.Default with { IncludeFiles = true };

        var command = FilterCommandEmitter.CreateFilterCommand([
            "src/nested/deep.txt"
        ], options, root, "C:\\Repo");

        Assert.NotNull(command);
        Assert.Contains("--include-dir src", command);
        Assert.Contains("--include-dir src/nested", command);
    }

    [Fact]
    public void CreateFilterCommand_Should_PreserveOptions_And_Patterns()
    {
        var root = BuildSampleTree();
        var options = TreeOptions.Default with
        {
            IncludeFiles = true,
            MaxDepth = 2,
            ConnectorStyle = ConnectorStyle.Ascii,
            Metadata = MetadataFields.Size | MetadataFields.Modified | MetadataFields.Owner,
            IncludeExtensions = new[] { "CS" },
            ExcludeFilePatterns = new[] { "**/*.md" },
            IncludeDirectoryPatterns = new[] { "src" },
            ExcludeDirectoryPatterns = new[] { "tests" },
        };

        var command = FilterCommandEmitter.CreateFilterCommand([
            "src/Program.cs"
        ], options, root, "C:\\Repo");

        Assert.NotNull(command);
        Assert.Contains("tree C:\\Repo", command);
        Assert.Contains("--files", command);
        Assert.Contains("--ascii", command);
        Assert.Contains("--depth 2", command);
        Assert.Contains("--details", command);
        Assert.Contains("--details-field owner", command);
        Assert.Contains("--ext .cs", command);
        Assert.Contains("--exclude **/*.md", command);
        Assert.Contains("--include-dir src", command);
        Assert.Contains("--exclude-dir tests", command);
        Assert.Contains("--include src/Program.cs", command);
    }

    [Fact]
    public void CreateFilterCommand_Should_QuoteArguments_WithSpaces()
    {
        var root = BuildSampleTree();
        var options = TreeOptions.Default with { IncludeFiles = true };

        var command = FilterCommandEmitter.CreateFilterCommand([
            "docs/More Files/Report.txt"
        ], options, root, "C:\\Work\\Arbor CLI");

        Assert.NotNull(command);
        Assert.Contains("\"C:\\Work\\Arbor CLI\"", command);
        Assert.Contains("--include-dir \"docs/More Files\"", command);
        Assert.Contains("--include \"docs/More Files/Report.txt\"", command);
    }

    [Fact]
    public void CreateFilterCommand_Should_NotAddIncludeDir_ForRootFile()
    {
        var root = BuildSampleTree();
        var options = TreeOptions.Default with { IncludeFiles = true };

        var command = FilterCommandEmitter.CreateFilterCommand([
            "README.md"
        ], options, root, "C:\\Repo");

        Assert.NotNull(command);
        Assert.Contains("--include README.md", command);
        Assert.DoesNotContain("--include-dir", command);
    }

    [Fact]
    public void CreateFilterCommand_Should_ReturnNull_When_NoSelections()
    {
        var root = BuildSampleTree();
        var options = TreeOptions.Default;

        var command = FilterCommandEmitter.CreateFilterCommand([
            "   ",
            string.Empty
        ], options, root, "C:\\Repo");

        Assert.Null(command);
    }

    private static TreeNode BuildSampleTree()
    {
        return TreeNode.Directory("root", string.Empty, NodeMetadata.Empty, new[]
        {
            TreeNode.Directory("src", "src", NodeMetadata.Empty, new[]
            {
                TreeNode.Directory("nested", "src/nested", NodeMetadata.Empty, new[]
                {
                    TreeNode.File("deep.txt", "src/nested/deep.txt", NodeMetadata.Empty),
                }),
                TreeNode.File("Program.cs", "src/Program.cs", NodeMetadata.Empty),
            }),
            TreeNode.Directory("docs", "docs", NodeMetadata.Empty, new[]
            {
                TreeNode.Directory("More Files", "docs/More Files", NodeMetadata.Empty, new[]
                {
                    TreeNode.File("Report.txt", "docs/More Files/Report.txt", NodeMetadata.Empty),
                }),
            }),
            TreeNode.File("README.md", "README.md", NodeMetadata.Empty),
        });
    }
}
