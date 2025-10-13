using System;

namespace Arbor.Cli.Exporting;

public static class TreeExporterFactory
{
    public static ITreeExporter Create(TreeExportFormat format) => format switch
    {
        TreeExportFormat.Text => new TextTreeExporter(),
        TreeExportFormat.Json => new JsonTreeExporter(),
        TreeExportFormat.Markdown => new MarkdownTreeExporter(),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format."),
    };
}
