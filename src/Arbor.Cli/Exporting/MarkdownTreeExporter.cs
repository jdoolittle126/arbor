using System.Text;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using TreeNode = Arbor.Core.TreeBuilding.TreeNode;

namespace Arbor.Cli.Exporting;

public sealed class MarkdownTreeExporter : ITreeExporter
{
    public void Export(DirectoryTreeResult result, TreeOptions options, IAnsiConsole console, TextWriter? writer)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(console);

        var markdown = BuildMarkdown(result, options.Metadata);

        if (writer is null)
        {
            console.WriteLine(markdown);
        }
        else
        {
            writer.Write(markdown);
            writer.Flush();
        }
    }

    private static string BuildMarkdown(DirectoryTreeResult result, MetadataFields metadataFields)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {result.Root.Name}");

        foreach (var child in result.Root.Children)
        {
            AppendNode(builder, child, indent: 0, metadataFields);
        }

        if (result.Warnings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            foreach (var warning in result.Warnings)
            {
                builder.AppendLine($"- {warning}");
            }
        }

        return builder.ToString();
    }

    private static void AppendNode(StringBuilder builder, TreeNode node, int indent, MetadataFields metadataFields)
    {
        var prefix = new string(' ', indent * 2);
        var metadataSuffix = MetadataFormatter.FormatPlainInline(node.Metadata, metadataFields);

        builder.AppendLine($"{prefix}- {node.Name}{metadataSuffix}");

        foreach (var child in node.Children)
        {
            AppendNode(builder, child, indent + 1, metadataFields);
        }
    }
}
