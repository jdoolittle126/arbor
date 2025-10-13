using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Interactive;

internal static class TreeSelectionPruner
{
    public static DirectoryTreeResult Prune(DirectoryTreeResult source, IReadOnlyCollection<string> selectedPaths)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selectedPaths);

        var normalized = new HashSet<string>(selectedPaths.Select(Normalize), StringComparer.OrdinalIgnoreCase);
        var rootSelected = normalized.Contains(string.Empty);
        var prunedRoot = PruneNode(source.Root, normalized, rootSelected, isRoot: true);

        return source with { Root = prunedRoot ?? source.Root };
    }

    private static TreeNode? PruneNode(TreeNode node, HashSet<string> selected, bool ancestorSelected, bool isRoot)
    {
        var key = Normalize(node.RelativePath);
        var nodeSelected = isRoot ? ancestorSelected : selected.Contains(key);

        if (ancestorSelected || nodeSelected)
        {
            return node;
        }

        if (node.Kind == NodeKind.File)
        {
            return null;
        }

        var prunedChildren = node.Children
            .Select(child => PruneNode(child, selected, ancestorSelected: false, isRoot: false))
            .OfType<TreeNode>()
            .ToList();

        return prunedChildren.Count > 0 || isRoot
            ? TreeNode.Directory(node.Name, node.RelativePath, node.Metadata, prunedChildren)
            : null;
    }

    private static string Normalize(string path) =>
        string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');
}