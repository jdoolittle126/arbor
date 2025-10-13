using System;
using System.IO;
using Arbor.Cli.Rendering;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using TreeNode = Arbor.Core.TreeBuilding.TreeNode;

namespace Arbor.Cli.Exporting;

public sealed class TextTreeExporter : ITreeExporter
{
    public void Export(DirectoryTreeResult result, TreeOptions options, IAnsiConsole console, TextWriter? writer)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(console);

        if (writer is null)
        {
            var renderer = TreeRenderer.ForOptions(options);
            var tree = renderer.Render(result.Root);
            console.Write(tree);
            return;
        }

        WritePlainText(result.Root, options, writer);
        writer.Flush();
    }

    private static void WritePlainText(TreeNode root, TreeOptions options, TextWriter writer)
    {
        writer.WriteLine(BuildNodeLabel(root, options));

        var children = root.Children;
        for (var i = 0; i < children.Count; i++)
        {
            WriteNode(children[i], options, indent: string.Empty, isLast: i == children.Count - 1, writer);
        }
    }

    private static void WriteNode(TreeNode node, TreeOptions options, string indent, bool isLast, TextWriter writer)
    {
        var (branch, childIndent) = options.ConnectorStyle == ConnectorStyle.Ascii
            ? (isLast ? "`-- " : "|-- ", isLast ? "    " : "|   ")
            : (isLast ? "└── " : "├── ", isLast ? "    " : "│   ");

        writer.Write(indent);
        writer.Write(branch);
        writer.WriteLine(BuildNodeLabel(node, options));

        var nextIndent = indent + childIndent;
        for (var i = 0; i < node.Children.Count; i++)
        {
            WriteNode(node.Children[i], options, nextIndent, i == node.Children.Count - 1, writer);
        }
    }

    private static string BuildNodeLabel(TreeNode node, TreeOptions options)
    {
        if (options.Metadata == MetadataFields.None)
        {
            return node.Name;
        }

        var suffix = MetadataFormatter.FormatPlainInline(node.Metadata, options.Metadata);
        return string.Concat(node.Name, suffix);
    }
}
