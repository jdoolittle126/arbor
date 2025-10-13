using System;
using Arbor.Cli.Commands;
using Arbor.Cli.Exporting;
using Arbor.Cli.Tests.Support;
using Spectre.Console.Testing;
using System.IO.Abstractions.TestingHelpers;

namespace Arbor.Cli.Tests;

public sealed class TreeCommandTests
{
    [Fact]
    public void TreeCommand_Should_RenderDirectories_ByDefault()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings { RootPath = rootPath };

        var exitCode = command.Execute(null!, settings);

        Assert.Equal(0, exitCode);
        var output = console.Output;
        Assert.Contains("src", output);
        Assert.Contains("tests", output);
        Assert.DoesNotContain("README.md", output);
    }

    [Fact]
    public void TreeCommand_Should_RenderFiles_When_FlagEnabled()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.Contains("README.md", output);
        Assert.Contains("Program.cs", output);
    }

    [Fact]
    public void TreeCommand_Should_HideNestedDirectories_When_DepthExceeded()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            MaxDepth = 1,
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.Contains("tests", output);
        Assert.DoesNotContain("Nested", output);
    }

    [Fact]
    public void TreeCommand_Should_RenderAsciiConnectors_When_AsciiRequested()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            Ascii = true,
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.Contains("|-- src", output);
        Assert.DoesNotContain("├──", output);
    }

    [Fact]
    public void TreeCommand_Should_RenderMetadata_When_DetailsEnabled()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var readmePath = fileSystem.Path.Combine(rootPath, "README.md");
        fileSystem.File.WriteAllText(readmePath, "hello world");
        var timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        fileSystem.File.SetLastWriteTimeUtc(readmePath, timestamp);

        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            UseDefaultDetails = true,
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.Contains("README.md", output);
        Assert.Contains("11 B", output);
        Assert.Contains("2024-01-01 12:00 UTC", output);
    }

    [Fact]
    public void TreeCommand_Should_FilterFiles_ByIncludePattern()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        fileSystem.AddFile(fileSystem.Path.Combine(rootPath, "notes.txt"), new MockFileData("notes"));

        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            IncludePatterns = new[] { "src/Program.cs" },
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.Contains("Program.cs", output);
        Assert.DoesNotContain("README.md", output);
        Assert.DoesNotContain("notes.txt", output);
    }

    [Fact]
    public void TreeCommand_Should_ExcludeFiles_When_PatternMatches()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            ExcludePatterns = new[] { "*.md" },
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.DoesNotContain("README.md", output);
        Assert.Contains("Program.cs", output);
    }

    [Fact]
    public void TreeCommand_Should_FilterFiles_ByExtension_When_ExtensionProvided()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        fileSystem.AddFile(fileSystem.Path.Combine(rootPath, "diagram.png"), new MockFileData(new byte[] { 0, 1, 2 }));

        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            Extensions = new[] { ".cs" },
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.Contains("Program.cs", output);
        Assert.DoesNotContain("README.md", output);
        Assert.DoesNotContain("diagram.png", output);
    }

    [Fact]
    public void TreeCommand_Should_ExcludeDirectories_When_ExcludeDirProvided()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            ExcludeDirectories = new[] { "tests" },
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.DoesNotContain("├── tests", output);
        Assert.DoesNotContain("└── tests", output);
        Assert.Contains("src", output);
    }

    [Fact]
    public void TreeCommand_Should_IncludeOnlySpecifiedDirectories_When_IncludeDirProvided()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            IncludeDirectories = new[] { "tests" },
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.Contains("tests", output);
        Assert.DoesNotContain("├── src", output);
        Assert.DoesNotContain("└── src", output);
    }

    [Fact]
    public void TreeCommand_Should_WriteTextExport_When_OutputSpecified()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var outputPath = fileSystem.Path.Combine(rootPath, "tree.txt");

        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            OutputPath = outputPath,
        };

        command.Execute(null!, settings);

        Assert.True(fileSystem.File.Exists(outputPath));
        var fileContents = fileSystem.File.ReadAllText(outputPath);
        Assert.Contains("Program.cs", fileContents);
        Assert.Contains("└──", fileContents);
        Assert.Contains("Exported tree to", console.Output);
    }

    [Fact]
    public void TreeCommand_Should_WriteJsonExport_When_OutputSpecified()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var outputPath = fileSystem.Path.Combine(rootPath, "tree.json");

        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            ExportFormat = TreeExportFormat.Json,
            OutputPath = outputPath,
        };

        command.Execute(null!, settings);

        var fileContents = fileSystem.File.ReadAllText(outputPath);
        Assert.Contains("\"Program.cs\"", fileContents);
        Assert.Contains("Exported tree to", console.Output);
    }

    [Fact]
    public void TreeCommand_Should_ExportJson_When_FormatIsJson()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        fileSystem.AddFile(fileSystem.Path.Combine(rootPath, "notes.txt"), new MockFileData("hello"));

        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            ExportFormat = TreeExportFormat.Json,
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.Contains("\"Root\"", output);
        Assert.Contains("\"Name\"", output);
        Assert.Contains("\"Warnings\"", output);
    }

    [Fact]
    public void TreeCommand_Should_ExportMarkdown_When_FormatIsMarkdown()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        fileSystem.AddFile(fileSystem.Path.Combine(rootPath, "notes.txt"), new MockFileData("hello"));

        var console = new TestConsole();
        var command = new TreeCommand(console, fileSystem);
        var settings = new TreeSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            ExportFormat = TreeExportFormat.Markdown,
        };

        command.Execute(null!, settings);

        var output = console.Output;
        Assert.StartsWith("#", output.TrimStart());
        Assert.Contains("- README.md", output);
    }
}
