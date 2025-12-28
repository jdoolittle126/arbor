using Arbor.Cli;
using System;
using System.IO;

namespace Arbor.Cli.IntegrationTests;

public sealed class CliIntegrationTests
{
    [Fact]
    public void ArborCli_Should_RunDefaultCommand()
    {
        using var scenario = PhysicalTreeScenario.Create();
        var app = ArborCommandAppFactory.Create();

        var args = ArborCommandAppHost.NormalizeArguments(new[] { scenario.RootPath, "-f" });
        var exitCode = ArborCommandAppHost.Run(app, args);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void ArborCli_Should_HandleTreeAlias()
    {
        using var scenario = PhysicalTreeScenario.Create();
        var app = ArborCommandAppFactory.Create();

        var args = ArborCommandAppHost.NormalizeArguments(new[] { "tree", scenario.RootPath, "-f" });
        var exitCode = ArborCommandAppHost.Run(app, args);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void ArborCli_Should_SupportVersionCommand()
    {
        using var scenario = PhysicalTreeScenario.Create();
        var app = ArborCommandAppFactory.Create();

        var args = ArborCommandAppHost.NormalizeArguments(new[] { "version" });
        var exitCode = ArborCommandAppHost.Run(app, args);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void ArborCli_Should_ExportJson_ToFile()
    {
        using var scenario = PhysicalTreeScenario.Create();
        var app = ArborCommandAppFactory.Create();

        var outputPath = Path.Combine(scenario.RootPath, "tree.json");
        var args = ArborCommandAppHost.NormalizeArguments(new[]
        {
            "tree",
            scenario.RootPath,
            "--export",
            "json",
            "--output",
            outputPath,
            "-f",
        });
        var exitCode = ArborCommandAppHost.Run(app, args);

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(outputPath));
        var contents = File.ReadAllText(outputPath);
        Assert.Contains("\"Root\"", contents);
    }

    [Fact]
    public void ArborCli_Should_ExportSelection_FromInteractiveSession()
    {
        using var scenario = PhysicalTreeScenario.Create();
        var app = ArborCommandAppFactory.Create();
        var outputPath = Path.Combine(scenario.RootPath, "interactive-selection.txt");

        var args = ArborCommandAppHost.NormalizeArguments(new[]
        {
            "interactive",
            scenario.RootPath,
            "--files",
            "--output",
            outputPath,
            "--script",
            "select 0",
            "--script",
            "export",
        });
        var exitCode = ArborCommandAppHost.Run(app, args);

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(outputPath));
        var contents = File.ReadAllText(outputPath);
        Assert.Contains("src", contents);
        Assert.Contains("Program.cs", contents);
    }
}

file sealed class PhysicalTreeScenario : IDisposable
{
    private PhysicalTreeScenario(string rootPath)
    {
        RootPath = rootPath;
    }

    public string RootPath { get; }

    public static PhysicalTreeScenario Create()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "Arbor Cli Tests", Guid.NewGuid().ToString("N"));
        var srcPath = Path.Combine(rootPath, "src");
        var testsPath = Path.Combine(rootPath, "tests");
        var nestedPath = Path.Combine(testsPath, "Nested");

        Directory.CreateDirectory(srcPath);
        Directory.CreateDirectory(testsPath);
        Directory.CreateDirectory(nestedPath);

        File.WriteAllText(Path.Combine(rootPath, "README.md"), string.Empty);
        File.WriteAllText(Path.Combine(srcPath, "Program.cs"), string.Empty);
        File.WriteAllText(Path.Combine(testsPath, "UnitTest.cs"), string.Empty);

        return new PhysicalTreeScenario(rootPath);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }
}
