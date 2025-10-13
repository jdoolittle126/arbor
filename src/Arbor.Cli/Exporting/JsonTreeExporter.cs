using System;
using System.IO;
using System.Text.Json;
using Arbor.Core.TreeBuilding;
using Spectre.Console;

namespace Arbor.Cli.Exporting;

public sealed class JsonTreeExporter : ITreeExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public void Export(DirectoryTreeResult result, TreeOptions options, IAnsiConsole console, TextWriter? writer)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(console);

        var payload = JsonSerializer.Serialize(result, Options);

        if (writer is not null)
        {
            writer.Write(payload);
            writer.Flush();
            return;
        }

        console.WriteLine(payload);
    }
}
