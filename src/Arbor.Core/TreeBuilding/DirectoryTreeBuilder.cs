using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Arbor.Core.TreeBuilding;

public sealed class DirectoryTreeBuilder
{
    private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;
    private readonly IFileSystem _fileSystem;

    public DirectoryTreeBuilder()
        : this(new FileSystem())
    {
    }

    public DirectoryTreeBuilder(IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        _fileSystem = fileSystem;
    }

    public DirectoryTreeResult Build(string rootPath, TreeOptions options)
    {
        ArgumentNullException.ThrowIfNull(rootPath);
        ArgumentNullException.ThrowIfNull(options);

        var fullPath = _fileSystem.Path.GetFullPath(rootPath);
        var rootDirectory = _fileSystem.DirectoryInfo.New(fullPath);

        if (!rootDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {fullPath}");
        }

        var warnings = new List<string>();
        var rootNode = BuildDirectory(rootDirectory, depth: 0, relativePath: string.Empty, isRoot: true)
            ?? CreateFallbackRoot(rootDirectory);
        return new DirectoryTreeResult(rootNode, warnings);

        TreeNode? BuildDirectory(IDirectoryInfo directory, int depth, string relativePath, bool isRoot = false)
        {
            var directoryRelativePath = isRoot ? string.Empty : relativePath;
            var name = isRoot ? directory.FullName : directory.Name;

            var directoryAllowed = isRoot || options.ShouldIncludeDirectory(directoryRelativePath);

            if (!directoryAllowed)
            {
                return null;
            }

            if (!options.ShouldIncludeLevel(depth))
            {
                return TreeNode.Directory(name, directoryRelativePath, CreateDirectoryMetadata(directory), Array.Empty<TreeNode>());
            }

            var hasDepthBudget = options.ShouldIncludeLevel(depth + 1);
            var directoryChildren = new List<TreeNode>();

            if (hasDepthBudget)
            {
                foreach (var childDirectory in SafeEnumerateDirectories(directory))
                {
                    var childRelative = CombineRelativePath(directoryRelativePath, childDirectory.Name);
                    var childNode = BuildDirectory(childDirectory, depth + 1, childRelative);
                    if (childNode is not null)
                    {
                        directoryChildren.Add(childNode);
                    }
                }
            }

            var fileChildren = new List<TreeNode>();
            if (options.IncludeFiles && hasDepthBudget)
            {
                foreach (var file in SafeEnumerateFiles(directory))
                {
                    var fileRelativePath = CombineRelativePath(directoryRelativePath, file.Name);
                    if (!options.ShouldIncludeFile(fileRelativePath))
                    {
                        continue;
                    }

                    fileChildren.Add(TreeNode.File(file.Name, fileRelativePath, CreateFileMetadata(file)));
                }
            }

            var children = directoryChildren.Concat(fileChildren).ToList();

            var explicitlyIncluded = !isRoot && options.NormalizedIncludeDirectories.Count > 0 && options.ShouldIncludeDirectory(directoryRelativePath);
            var keepEmpty = isRoot || !hasDepthBudget || !options.IncludeFiles || explicitlyIncluded;

            if (!isRoot && children.Count == 0 && hasDepthBudget && !keepEmpty)
            {
                return null;
            }

            return TreeNode.Directory(name, directoryRelativePath, CreateDirectoryMetadata(directory), children);
        }

        TreeNode CreateFallbackRoot(IDirectoryInfo directory) =>
            TreeNode.Directory(directory.FullName, string.Empty, CreateDirectoryMetadata(directory), Array.Empty<TreeNode>());

        IEnumerable<IDirectoryInfo> SafeEnumerateDirectories(IDirectoryInfo directory)
        {
            IDirectoryInfo[] directories;
            try
            {
                directories = directory.GetDirectories();
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                warnings.Add($"Skipping {directory.FullName} (access denied).");
                return Enumerable.Empty<IDirectoryInfo>();
            }

            return directories
                .OrderBy(d => d.Name, NameComparer)
                .ToArray();
        }

        IEnumerable<IFileInfo> SafeEnumerateFiles(IDirectoryInfo directory)
        {
            IFileInfo[] files;
            try
            {
                files = directory.GetFiles();
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                warnings.Add($"Skipping files in {directory.FullName} (access denied).");
                return Enumerable.Empty<IFileInfo>();
            }

            return files
                .OrderBy(f => f.Name, NameComparer)
                .ToArray();
        }

        string CombineRelativePath(string parentRelativePath, string childName)
        {
            if (string.IsNullOrEmpty(parentRelativePath))
            {
                return childName;
            }

            return _fileSystem.Path.Combine(parentRelativePath, childName);
        }

        NodeMetadata CreateDirectoryMetadata(IDirectoryInfo directoryInfo)
        {
            long? size = null;
            DateTimeOffset? modified = options.HasMetadata(MetadataFields.Modified)
                ? ToUtcOffset(directoryInfo.LastWriteTimeUtc)
                : null;
            var (permissions, owner, group) = MetadataCollector.GetMetadata(directoryInfo.FullName, options.Metadata);
            return new NodeMetadata(size, modified, permissions, owner, group);
        }

        NodeMetadata CreateFileMetadata(IFileInfo fileInfo)
        {
            long? size = options.HasMetadata(MetadataFields.Size) ? fileInfo.Length : null;
            DateTimeOffset? modified = options.HasMetadata(MetadataFields.Modified)
                ? ToUtcOffset(fileInfo.LastWriteTimeUtc)
                : null;
            var (permissions, owner, group) = MetadataCollector.GetMetadata(fileInfo.FullName, options.Metadata);
            return new NodeMetadata(size, modified, permissions, owner, group);
        }

        DateTimeOffset ToUtcOffset(DateTime value)
        {
            var utc = value.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => value,
            };

            return new DateTimeOffset(utc, TimeSpan.Zero);
        }
    }
}
