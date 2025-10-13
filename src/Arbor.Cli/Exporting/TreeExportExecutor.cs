using System;
using System.IO;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using System.IO.Abstractions;

namespace Arbor.Cli.Exporting;

internal static class TreeExportExecutor
{
    public static int Export(ITreeExporter exporter, DirectoryTreeResult result, TreeOptions options, string? outputPath, IAnsiConsole console, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(exporter);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(fileSystem);

        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            string fullPath;
            try
            {
                fullPath = fileSystem.Path.GetFullPath(outputPath);
            }
            catch (Exception exception)
            {
                console.MarkupLine($"[red]Invalid output path[/]: {exception.Message}");
                return -1;
            }

            var directory = fileSystem.Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !fileSystem.Directory.Exists(directory))
            {
                fileSystem.Directory.CreateDirectory(directory);
            }

            using var stream = fileSystem.File.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new StreamWriter(stream);
            exporter.Export(result, options, console, writer);
            console.MarkupLine($"[green]Exported tree to {Markup.Escape(fullPath)}[/]");
            return 0;
        }

        exporter.Export(result, options, console, writer: null);
        return 0;
    }
}
