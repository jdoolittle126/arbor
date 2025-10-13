using Spectre.Console;
using Spectre.Console.Rendering;
using DomainTreeNode = Arbor.Core.TreeBuilding.TreeNode;
using DomainNodeKind = Arbor.Core.TreeBuilding.NodeKind;

namespace Arbor.Cli.Rendering;

public sealed class BasicNodeLabelStrategy : INodeLabelStrategy
{
    public IRenderable CreateRootLabel(DomainTreeNode root)
    {
        var escaped = Markup.Escape(root.Name);
        return new Markup($"[bold]{escaped}[/]");
    }

    public IRenderable CreateLabel(DomainTreeNode node)
    {
        var escaped = Markup.Escape(node.Name);
        return node.Kind == DomainNodeKind.Directory
            ? new Markup($"[yellow]{escaped}[/]")
            : new Text(escaped);
    }
}
