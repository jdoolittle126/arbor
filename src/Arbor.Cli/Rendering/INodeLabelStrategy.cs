using Arbor.Core.TreeBuilding;
using Spectre.Console.Rendering;
using DomainTreeNode = Arbor.Core.TreeBuilding.TreeNode;

namespace Arbor.Cli.Rendering;

public interface INodeLabelStrategy
{
    IRenderable CreateRootLabel(DomainTreeNode root);

    IRenderable CreateLabel(DomainTreeNode node);
}
