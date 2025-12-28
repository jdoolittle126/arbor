using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.Cli.Commands;
using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Interactive;

internal static class FilterCommandEmitter
{
    public static string? CreateFilterCommand(IEnumerable<string> selectedPaths, TreeOptions originalOptions, TreeNode root, string? rootPath)
    {
        ArgumentNullException.ThrowIfNull(selectedPaths);
        ArgumentNullException.ThrowIfNull(originalOptions);
        ArgumentNullException.ThrowIfNull(root);

        var directoryIndex = BuildDirectoryIndex(root);

        var normalized = selectedPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Normalize)
            .ToList();

        if (normalized.Count == 0)
        {
            return null;
        }

        var includeDirPatterns = new HashSet<string>(originalOptions.NormalizedIncludeDirectories, StringComparer.OrdinalIgnoreCase);
        var includeFilePatterns = new HashSet<string>(originalOptions.NormalizedIncludePatterns, StringComparer.OrdinalIgnoreCase);

        foreach (var path in normalized)
        {
            if (directoryIndex.Contains(path))
            {
                AddDirectoryAndAncestors(includeDirPatterns, path);
                continue;
            }

            includeFilePatterns.Add(path);

            var lastSlash = path.LastIndexOf('/');
            if (lastSlash > 0)
            {
                var parentPath = path[..lastSlash];
                AddDirectoryAndAncestors(includeDirPatterns, parentPath);
            }
        }

        var arguments = new List<string> { "tree" };
        if (!string.IsNullOrWhiteSpace(rootPath))
        {
            arguments.Add(Escape(rootPath));
        }

        if (originalOptions.IncludeFiles)
        {
            arguments.Add("--files");
        }

        var (useDefaultDetails, additionalDetails) = MetadataFieldsParser.Describe(originalOptions.Metadata);
        if (useDefaultDetails)
        {
            arguments.Add("--details");
        }

        foreach (var detail in additionalDetails)
        {
            arguments.Add("--details-field");
            arguments.Add(detail);
        }

        if (originalOptions.ConnectorStyle == ConnectorStyle.Ascii)
        {
            arguments.Add("--ascii");
        }

        if (originalOptions.MaxDepth is { } depth)
        {
            arguments.Add("--depth");
            arguments.Add(depth.ToString());
        }

        foreach (var pattern in includeFilePatterns.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            arguments.Add("--include");
            arguments.Add(Escape(pattern));
        }

        foreach (var pattern in originalOptions.NormalizedExcludePatterns.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            arguments.Add("--exclude");
            arguments.Add(Escape(pattern));
        }

        foreach (var pattern in includeDirPatterns.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            arguments.Add("--include-dir");
            arguments.Add(Escape(pattern));
        }

        foreach (var pattern in originalOptions.NormalizedExcludeDirectories.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            arguments.Add("--exclude-dir");
            arguments.Add(Escape(pattern));
        }

        foreach (var extension in originalOptions.NormalizedExtensions.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            arguments.Add("--ext");
            arguments.Add(Escape(extension));
        }

        return string.Join(' ', arguments);
    }

    private static HashSet<string> BuildDirectoryIndex(TreeNode root)
    {
        var directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<TreeNode>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node.Kind != NodeKind.Directory)
            {
                continue;
            }

            directories.Add(Normalize(node.RelativePath));
            foreach (var child in node.Children)
            {
                stack.Push(child);
            }
        }

        return directories;
    }

    private static void AddDirectoryAndAncestors(HashSet<string> includeDirPatterns, string path)
    {
        var trimmed = path.Trim('/');
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        var segments = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = segments[0];
        includeDirPatterns.Add(current);

        for (var i = 1; i < segments.Length; i++)
        {
            current = string.Concat(current, "/", segments[i]);
            includeDirPatterns.Add(current);
        }
    }

    private static string Normalize(string path) =>
        string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.Replace('\\', '/').Trim().Trim('/');

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Contains(' ')
            ? $"\"{value}\""
            : value;
    }
}
