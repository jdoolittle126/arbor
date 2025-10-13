using Arbor.Cli.Commands;
using Spectre.Console.Testing;

namespace Arbor.Cli.Tests;

public sealed class VersionCommandTests
{
    [Fact]
    public void VersionCommand_Should_OutputAssemblyVersion()
    {
        var console = new TestConsole();
        var command = new VersionCommand(console);

        var exitCode = command.Execute(null!);

        Assert.Equal(0, exitCode);
        Assert.Contains("Arbor CLI version", console.Output);
    }
}
