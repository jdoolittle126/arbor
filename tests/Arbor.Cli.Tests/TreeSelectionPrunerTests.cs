using System.Collections.Generic;
using System.Linq;
using Arbor.Cli.Interactive;
using Arbor.Cli.Tests.Support;
using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Tests;

public sealed class TreeSelectionPrunerTests
{
    [Fact]
    public void Prune_Should_KeepSelectedFile()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with { IncludeFiles = true };

        var tree = builder.Build(rootPath, options);
        var selection = new HashSet<string> { Normalize("src/Program.cs") };

        var pruned = TreeSelectionPruner.Prune(tree, selection);

        Assert.Contains(pruned.Root.Children, node => node.Name == "src");
        var src = Assert.Single(pruned.Root.Children, node => node.Name == "src");
        Assert.Contains(src.Children, node => node.Name == "Program.cs");
    }

    [Fact]
    public void Prune_Should_ReturnOriginalTree_WhenDirectorySelected()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with { IncludeFiles = true };

        var tree = builder.Build(rootPath, options);
        var selection = new HashSet<string> { Normalize("src") };

        var pruned = TreeSelectionPruner.Prune(tree, selection);

        var src = Assert.Single(pruned.Root.Children, node => node.Name == "src");
        Assert.Equal(tree.Root.Children.Single(node => node.Name == "src").Children.Count, src.Children.Count);
    }

    [Fact]
    public void Prune_Should_PruneUnselectedSiblingDirectories()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with { IncludeFiles = true };

        var tree = builder.Build(rootPath, options);
        var selection = new HashSet<string> { Normalize("tests/UnitTest.cs") };

        var pruned = TreeSelectionPruner.Prune(tree, selection);

        Assert.DoesNotContain(pruned.Root.Children, node => node.Name == "src");
        Assert.Contains(pruned.Root.Children, node => node.Name == "tests");
    }

    [Fact]
    public void Prune_Should_KeepRoot_WhenRootSelected()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with { IncludeFiles = true };

        var tree = builder.Build(rootPath, options);
        var selection = new HashSet<string> { string.Empty };

        var pruned = TreeSelectionPruner.Prune(tree, selection);

        Assert.Equal(tree.Root.Children.Count, pruned.Root.Children.Count);
    }

    private static string Normalize(string path) => path.Replace('\\', '/');
}
