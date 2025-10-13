using Arbor.Cli.Exporting;
using Arbor.Cli.Interactive;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using Spectre.Console.Cli;
using System.IO.Abstractions;

namespace Arbor.Cli.Commands;

public sealed class InteractiveCommand : Command<InteractiveSettings>
{
    private readonly IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly DirectoryTreeBuilder _builder;

    public InteractiveCommand()
        : this(AnsiConsole.Console, new FileSystem())
    {
    }

    internal InteractiveCommand(IAnsiConsole console)
        : this(console, new FileSystem())
    {
    }

    internal InteractiveCommand(IAnsiConsole console, IFileSystem fileSystem)
        : this(console, fileSystem, new DirectoryTreeBuilder(fileSystem))
    {
    }

    internal InteractiveCommand(IAnsiConsole console, IFileSystem fileSystem, DirectoryTreeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(builder);

        _console = console;
        _fileSystem = fileSystem;
        _builder = builder;
    }

    public override int Execute(CommandContext context, InteractiveSettings settings)
    {
        var rootPath = settings.RootPath ?? _fileSystem.Directory.GetCurrentDirectory();

        string fullPath;
        try
        {
            fullPath = _fileSystem.Path.GetFullPath(rootPath);
        }
        catch (Exception exception)
        {
            _console.MarkupLine($"[red]Unable to resolve path[/]: {exception.Message}");
            return -1;
        }

        if (!_fileSystem.Directory.Exists(fullPath))
        {
            _console.MarkupLine($"[red]Directory not found[/]: {fullPath}");
            return -1;
        }

        var metadata = MetadataFieldsParser.Parse(settings.DetailFields, settings.UseDefaultDetails);

        var options = TreeOptions.Default with
        {
            IncludeFiles = settings.IncludeFiles,
            MaxDepth = settings.MaxDepth,
            ConnectorStyle = settings.Ascii ? ConnectorStyle.Ascii : ConnectorStyle.Unicode,
            Metadata = metadata,
            IncludeFilePatterns = settings.IncludePatterns,
            ExcludeFilePatterns = settings.ExcludePatterns,
            IncludeExtensions = settings.Extensions,
            IncludeDirectoryPatterns = settings.IncludeDirectories,
            ExcludeDirectoryPatterns = settings.ExcludeDirectories,
        };

        DirectoryTreeResult result;
        try
        {
            result = _builder.Build(fullPath, options);
        }
        catch (DirectoryNotFoundException)
        {
            _console.MarkupLine($"[red]Directory not found[/]: {fullPath}");
            return -1;
        }

        foreach (var warning in result.Warnings)
        {
            _console.MarkupLine($"[yellow]{warning}[/]");
        }

        var session = new InteractiveSession(_console, result.Root, options, settings.Script);
        var sessionResult = session.Run();

        if (sessionResult.Status == InteractiveSessionStatus.Quit || sessionResult.Selected.Count == 0)
        {
            if (sessionResult.Selected.Count == 0)
            {
                _console.MarkupLine("[yellow]No nodes selected. Exiting without export.[/]");
            }

            return 0;
        }

        var pruned = TreeSelectionPruner.Prune(result, sessionResult.Selected);
        var exporter = TreeExporterFactory.Create(settings.ExportFormat);
        var exitCode = TreeExportExecutor.Export(exporter, pruned, options, settings.OutputPath, _console, _fileSystem);

        if (exitCode == 0 && !string.IsNullOrWhiteSpace(sessionResult.SuggestedCommand))
        {
            _console.MarkupLine("[blue]Re-run this selection with:[/]");
            _console.MarkupLine($"[grey]{Markup.Escape(sessionResult.SuggestedCommand)}[/]");
        }

        return exitCode;
    }
}
