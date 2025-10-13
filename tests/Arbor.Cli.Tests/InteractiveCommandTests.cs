using Arbor.Cli.Commands;
using Arbor.Cli.Tests.Support;
using Spectre.Console.Testing;

namespace Arbor.Cli.Tests;

public sealed class InteractiveCommandTests
{
    [Fact]
    public void InteractiveCommand_Should_ExportSelection_ToFile()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var outputPath = fileSystem.Path.Combine(rootPath, "selection.txt");
        var console = new TestConsole();
        var command = new InteractiveCommand(console, fileSystem);

        var settings = new InteractiveSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            OutputPath = outputPath,
            Script = new[] { "select 0", "export" },
        };

        var exitCode = command.Execute(null!, settings);

        Assert.Equal(0, exitCode);
        Assert.True(fileSystem.File.Exists(outputPath));
        var contents = fileSystem.File.ReadAllText(outputPath);
        Assert.Contains("src", contents);
        Assert.Contains("Program.cs", contents);
    }

    [Fact]
    public void InteractiveCommand_Should_HandleQuitWithoutSelection()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new InteractiveCommand(console, fileSystem);

        var settings = new InteractiveSettings
        {
            RootPath = rootPath,
            Script = new[] { "quit" },
        };

        var exitCode = command.Execute(null!, settings);

        Assert.Equal(0, exitCode);
        Assert.Contains("No nodes selected", console.Output);
    }

    [Fact]
    public void InteractiveCommand_Should_SuggestFilterCommand_AfterExport()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new InteractiveCommand(console, fileSystem);

        var settings = new InteractiveSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            Script = new[] { "select 0", "export" },
        };

        var exitCode = command.Execute(null!, settings);

        Assert.Equal(0, exitCode);
        Assert.Contains("Re-run this selection with:", console.Output);
        Assert.Contains("tree", console.Output);
    }

    [Fact]
    public void InteractiveCommand_Should_SelectMultipleEntries_InSingleCommand()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new InteractiveCommand(console, fileSystem);

        var settings = new InteractiveSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            Script = new[] { "select 0,1", "export" },
        };

        var exitCode = command.Execute(null!, settings);

        Assert.Equal(0, exitCode);
        Assert.Contains("Re-run this selection", console.Output);
    }

    [Fact]
    public void InteractiveCommand_Should_PreserveMetadataOptions_InSuggestedCommand()
    {
        var (fileSystem, rootPath) = TestFileSystemFactory.Create();
        var console = new TestConsole();
        var command = new InteractiveCommand(console, fileSystem);

        var settings = new InteractiveSettings
        {
            RootPath = rootPath,
            IncludeFiles = true,
            UseDefaultDetails = true,
            DetailFields = new[] { "owner" },
            Script = new[] { "select 0", "export" },
        };

        var exitCode = command.Execute(null!, settings);

        Assert.Equal(0, exitCode);
        Assert.Contains("--details", console.Output);
        Assert.Contains("--details-field owner", console.Output);
    }
}
