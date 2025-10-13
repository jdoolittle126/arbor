using System;
using Arbor.Cli.Exporting;
using Spectre.Console;
using Spectre.Console.Rendering;
using DomainTreeNode = Arbor.Core.TreeBuilding.TreeNode;
using DomainNodeKind = Arbor.Core.TreeBuilding.NodeKind;
using MetadataFields = Arbor.Core.TreeBuilding.MetadataFields;

namespace Arbor.Cli.Rendering;

public sealed class MetadataNodeLabelStrategy : INodeLabelStrategy
{
    private readonly MetadataFields _fields;

    public MetadataNodeLabelStrategy(MetadataFields fields)
    {
        _fields = fields;
    }

    public IRenderable CreateRootLabel(DomainTreeNode root)
    {
        var escaped = Markup.Escape(root.Name);
        return new Markup($"[bold]{escaped}[/]");
    }

    public IRenderable CreateLabel(DomainTreeNode node)
    {
        var escapedName = Markup.Escape(node.Name);
        var prefix = node.Kind == DomainNodeKind.Directory
            ? $"[yellow]{escapedName}[/]"
            : escapedName;

        var metadataSuffix = MetadataFormatter.FormatMarkup(node.Metadata, _fields);
        return new Markup(string.Concat(prefix, metadataSuffix));
    }
}
