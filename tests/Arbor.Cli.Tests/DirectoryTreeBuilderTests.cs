using System;
using System.Linq;
using Arbor.Cli.Tests.Support;
using Arbor.Core.TreeBuilding;
using System.IO.Abstractions.TestingHelpers;

namespace Arbor.Cli.Tests;

public sealed class DirectoryTreeBuilderTests
{
    [Fact]
    public void DirectoryTreeBuilder_Should_ReturnRootFullPath()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);

        var result = builder.Build(rootPath, TreeOptions.Default);

        Assert.Equal(rootPath, result.Root.Name);
    }

    [Fact]
    public void DirectoryTreeBuilder_Should_IncludeFiles_When_Enabled()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with { IncludeFiles = true };

        var result = builder.Build(rootPath, options);

        Assert.Contains(result.Root.Children, child => child.Kind == NodeKind.File && child.Name == "README.md");
    }

    [Fact]
    public void DirectoryTreeBuilder_Should_RespectMaxDepth_When_Set()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with
        {
            IncludeFiles = true,
            MaxDepth = 1,
        };

        var result = builder.Build(rootPath, options);
        var testsNode = result.Root.Children.Single(node => node.Name == "tests");

        Assert.DoesNotContain(testsNode.Children, node => node.Name == "Nested");
    }

    [Fact]
    public void DirectoryTreeBuilder_Should_PopulateMetadata_When_Enabled()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with
        {
            IncludeFiles = true,
            Metadata = MetadataFields.Size | MetadataFields.Modified,
        };

        var readmePath = fileSystem.Path.Combine(rootPath, "README.md");
        var timestamp = new DateTime(2024, 2, 1, 8, 30, 0, DateTimeKind.Utc);
        fileSystem.File.SetLastWriteTimeUtc(readmePath, timestamp);

        var result = builder.Build(rootPath, options);
        var readmeNode = result.Root.Children.First(node => node.Name == "README.md");

        Assert.Equal(0, readmeNode.Metadata.SizeBytes); // Empty file from factory.
        Assert.Equal(timestamp, readmeNode.Metadata.LastModifiedUtc?.UtcDateTime);
    }

    [Fact]
    public void DirectoryTreeBuilder_Should_ExcludeDirectories_WhenPatternMatches()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with
        {
            IncludeFiles = true,
            ExcludeDirectoryPatterns = new[] { "tests**" },
        };

        var result = builder.Build(rootPath, options);

        Assert.DoesNotContain(result.Root.Children, node => node.Name == "tests");
        Assert.Contains(result.Root.Children, node => node.Name == "src");
    }

    [Fact]
    public void DirectoryTreeBuilder_Should_IncludeOnlySpecifiedDirectories()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with
        {
            IncludeFiles = true,
            IncludeDirectoryPatterns = new[] { "tests" },
        };

        var result = builder.Build(rootPath, options);

        Assert.Single(result.Root.Children, node => node.Name == "tests");
        Assert.DoesNotContain(result.Root.Children, node => node.Name == "src");
    }

    [Fact]
    public void DirectoryTreeBuilder_Should_PruneEmptyBranches_AfterFiltering()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var builder = new DirectoryTreeBuilder(fileSystem);
        var options = TreeOptions.Default with
        {
            IncludeFiles = true,
            IncludeFilePatterns = new[] { "src/*.cs" },
            ExcludeDirectoryPatterns = new[] { "tests" },
        };

        var result = builder.Build(rootPath, options);

        Assert.DoesNotContain(result.Root.Children, node => node.Name == "tests");
        var src = Assert.Single(result.Root.Children, node => node.Name == "src");
        Assert.All(src.Children, child => Assert.False(child.Name.Equals("Nested", StringComparison.OrdinalIgnoreCase)));
    }
}
