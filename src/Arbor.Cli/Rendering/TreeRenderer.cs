using System;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using DomainTreeNode = Arbor.Core.TreeBuilding.TreeNode;
using SpectreTreeNode = Spectre.Console.TreeNode;

namespace Arbor.Cli.Rendering;

public sealed class TreeRenderer
{
    private readonly IConnectorStrategy _connectorStrategy;
    private readonly INodeLabelStrategy _labelStrategy;

    private TreeRenderer(IConnectorStrategy connectorStrategy, INodeLabelStrategy labelStrategy)
    {
        ArgumentNullException.ThrowIfNull(connectorStrategy);
        ArgumentNullException.ThrowIfNull(labelStrategy);

        _connectorStrategy = connectorStrategy;
        _labelStrategy = labelStrategy;
    }

    public static TreeRenderer ForOptions(TreeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var connectorStrategy = ConnectorStrategyFactory.FromOptions(options);
        INodeLabelStrategy labelStrategy = options.Metadata == MetadataFields.None
            ? new BasicNodeLabelStrategy()
            : new MetadataNodeLabelStrategy(options.Metadata);

        return new TreeRenderer(connectorStrategy, labelStrategy);
    }

    public Tree Render(DomainTreeNode root)
    {
        var tree = new Tree(_labelStrategy.CreateRootLabel(root))
        {
            Guide = _connectorStrategy.Guide,
        };

        foreach (var child in root.Children)
        {
            AddChild(tree, child);
        }

        return tree;
    }

    private void AddChild(Tree tree, DomainTreeNode node)
    {
        if (node.Kind == NodeKind.File)
        {
            tree.AddNode(_labelStrategy.CreateLabel(node));
            return;
        }

        var directoryNode = tree.AddNode(_labelStrategy.CreateLabel(node));
        foreach (var child in node.Children)
        {
            AddChild(directoryNode, child);
        }
    }

    private void AddChild(SpectreTreeNode parent, DomainTreeNode node)
    {
        if (node.Kind == NodeKind.File)
        {
            parent.AddNode(_labelStrategy.CreateLabel(node));
            return;
        }

        var directoryNode = parent.AddNode(_labelStrategy.CreateLabel(node));
        foreach (var child in node.Children)
        {
            AddChild(directoryNode, child);
        }
    }
}
