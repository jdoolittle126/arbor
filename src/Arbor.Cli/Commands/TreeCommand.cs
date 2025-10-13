using System;
using Arbor.Cli.Exporting;
using Arbor.Cli.Rendering;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using Spectre.Console.Cli;
using System.IO;
using System.IO.Abstractions;

namespace Arbor.Cli.Commands;

public sealed class TreeCommand : Command<TreeSettings>
{
    private readonly IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly DirectoryTreeBuilder _builder;

    public TreeCommand()
        : this(AnsiConsole.Console, new FileSystem())
    {
    }

    internal TreeCommand(IAnsiConsole console)
        : this(console, new FileSystem())
    {
    }

    internal TreeCommand(IAnsiConsole console, IFileSystem fileSystem)
        : this(console, fileSystem, new DirectoryTreeBuilder(fileSystem))
    {
    }

    internal TreeCommand(IAnsiConsole console, IFileSystem fileSystem, DirectoryTreeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(builder);

        _console = console;
        _fileSystem = fileSystem;
        _builder = builder;
    }

    public override int Execute(CommandContext context, TreeSettings settings)
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

        var exporter = TreeExporterFactory.Create(settings.ExportFormat);
        return TreeExportExecutor.Export(exporter, result, options, settings.OutputPath, _console, _fileSystem);
    }
}
