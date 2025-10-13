using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbor.Core.TreeBuilding;

public sealed record TreeNode(
    string Name,
    NodeKind Kind,
    string RelativePath,
    NodeMetadata Metadata,
    IReadOnlyList<TreeNode> Children)
{
    public static TreeNode Directory(string name, string relativePath, NodeMetadata metadata, IEnumerable<TreeNode> children) =>
        new(name, NodeKind.Directory, relativePath, metadata, children.ToList());

    public static TreeNode File(string name, string relativePath, NodeMetadata metadata) =>
        new(name, NodeKind.File, relativePath, metadata, Array.Empty<TreeNode>());
}
