using Arbor.Cli;
using Arbor.Cli.Commands;
using Arbor.Cli.Tests.Support;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using Spectre.Console.Cli.Help;
using Spectre.Console.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Arbor.Cli.Tests;

public sealed class CliIntegrationTests
{
    [Fact]
    public void ArborCli_Should_RunDefaultCommand()
    {
        using var scenario = PhysicalTreeScenario.Create();
        var console = new TestConsole();
        using var registrar = BuildRegistrar(console, new FileSystem());
        var app = ArborCommandAppFactory.Create(registrar);

        var args = ArborCommandAppHost.NormalizeArguments(new[] { scenario.RootPath, "-f" });
        var exitCode = ArborCommandAppHost.Run(app, args);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void ArborCli_Should_HandleTreeAlias()
    {
        using var scenario = PhysicalTreeScenario.Create();
        var console = new TestConsole();
        using var registrar = BuildRegistrar(console, new FileSystem());
        var app = ArborCommandAppFactory.Create(registrar);

        var args = ArborCommandAppHost.NormalizeArguments(new[] { "tree", scenario.RootPath, "-f" });
        var exitCode = ArborCommandAppHost.Run(app, args);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void ArborCli_Should_SupportVersionCommand()
    {
        using var scenario = PhysicalTreeScenario.Create();
        var console = new TestConsole();
        using var registrar = BuildRegistrar(console, new FileSystem());
        var app = ArborCommandAppFactory.Create(registrar);

        var args = ArborCommandAppHost.NormalizeArguments(new[] { "version" });
        var exitCode = ArborCommandAppHost.Run(app, args);

        Assert.Equal(0, exitCode);
    }

    private static TestTypeRegistrar BuildRegistrar(IAnsiConsole console, IFileSystem fileSystem)
    {
        var registrar = new TestTypeRegistrar();
        registrar.RegisterInstance(typeof(IAnsiConsole), console);
        registrar.RegisterInstance(typeof(IFileSystem), fileSystem);
        registrar.RegisterLazy(typeof(TreeCommand), () => new TreeCommand(console, fileSystem, new DirectoryTreeBuilder(fileSystem)));
        registrar.RegisterLazy(typeof(VersionCommand), () => new VersionCommand(console));
        registrar.Register(typeof(DirectoryTreeBuilder), typeof(DirectoryTreeBuilder));
        registrar.RegisterInstance(typeof(IEnumerable<IHelpProvider>), Array.Empty<IHelpProvider>());
        return registrar;
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
        var rootPath = Path.Combine(Path.GetTempPath(), "ArborCliTests", Guid.NewGuid().ToString("N"));
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
