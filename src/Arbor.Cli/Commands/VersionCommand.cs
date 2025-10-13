using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Arbor.Cli.Commands;

public sealed class VersionCommand : Command
{
    private readonly IAnsiConsole _console;

    public VersionCommand()
        : this(AnsiConsole.Console)
    {
    }

    public VersionCommand(IAnsiConsole console)
    {
        ArgumentNullException.ThrowIfNull(console);
        _console = console;
    }

    public override int Execute(CommandContext context)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "unknown";

        _console.MarkupLine($"Arbor CLI version [green]{Markup.Escape(version)}[/]");
        return 0;
    }
}
