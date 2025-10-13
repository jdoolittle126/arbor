using System.Collections.Generic;

namespace Arbor.Core.TreeBuilding;

public sealed record DirectoryTreeResult(TreeNode Root, IReadOnlyList<string> Warnings);
