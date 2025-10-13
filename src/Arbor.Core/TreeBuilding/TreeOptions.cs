using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbor.Core.TreeBuilding;

public sealed record TreeOptions(
    bool IncludeFiles = false,
    int? MaxDepth = null,
    ConnectorStyle ConnectorStyle = ConnectorStyle.Unicode,
    MetadataFields Metadata = MetadataFields.None,
    IReadOnlyList<string>? IncludeFilePatterns = null,
    IReadOnlyList<string>? ExcludeFilePatterns = null,
    IReadOnlyList<string>? IncludeExtensions = null,
    IReadOnlyList<string>? IncludeDirectoryPatterns = null,
    IReadOnlyList<string>? ExcludeDirectoryPatterns = null)
{
    private static readonly IReadOnlyList<string> EmptyPatterns = Array.Empty<string>();

    public static TreeOptions Default { get; } = new();

    public IReadOnlyList<string> NormalizedIncludePatterns =>
        NormalizePatterns(IncludeFilePatterns);

    public IReadOnlyList<string> NormalizedExcludePatterns =>
        NormalizePatterns(ExcludeFilePatterns);

    public IReadOnlyList<string> NormalizedExtensions =>
        NormalizeExtensions(IncludeExtensions);

    public IReadOnlyList<string> NormalizedIncludeDirectories =>
        NormalizePatterns(IncludeDirectoryPatterns);

    public IReadOnlyList<string> NormalizedExcludeDirectories =>
        NormalizePatterns(ExcludeDirectoryPatterns);

    public bool ShouldIncludeLevel(int depth) =>
        !MaxDepth.HasValue || depth <= MaxDepth.Value;

    public bool HasMetadata(MetadataFields field) => (Metadata & field) == field;

    public bool ShouldIncludeFile(string relativePath)
    {
        var normalized = NormalizePath(relativePath);

        if (NormalizedExcludePatterns.Count > 0 &&
            NormalizedExcludePatterns.Any(pattern => WildcardMatcher.IsMatch(normalized, pattern)))
        {
            return false;
        }

        if (NormalizedExtensions.Count > 0 &&
            !NormalizedExtensions.Any(extension => normalized.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (NormalizedIncludePatterns.Count == 0)
        {
            return true;
        }

        return NormalizedIncludePatterns.Any(pattern => WildcardMatcher.IsMatch(normalized, pattern));
    }

    public bool ShouldIncludeDirectory(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            return true;
        }

        var normalized = NormalizePath(relativePath);

        if (NormalizedExcludeDirectories.Count > 0 &&
            NormalizedExcludeDirectories.Any(pattern => WildcardMatcher.IsMatch(normalized, pattern)))
        {
            return false;
        }

        if (NormalizedIncludeDirectories.Count == 0)
        {
            return true;
        }

        return NormalizedIncludeDirectories.Any(pattern => WildcardMatcher.IsMatch(normalized, pattern));
    }

    private static IReadOnlyList<string> NormalizePatterns(IReadOnlyList<string>? patterns)
    {
        if (patterns is null || patterns.Count == 0)
        {
            return EmptyPatterns;
        }

        return patterns
            .Where(pattern => !string.IsNullOrWhiteSpace(pattern))
            .Select(pattern => NormalizePath(pattern))
            .ToArray();
    }

    private static IReadOnlyList<string> NormalizeExtensions(IReadOnlyList<string>? extensions)
    {
        if (extensions is null || extensions.Count == 0)
        {
            return EmptyPatterns;
        }

        return extensions
            .Where(extension => !string.IsNullOrWhiteSpace(extension))
            .Select(extension => extension.StartsWith(".", StringComparison.Ordinal) ? extension.ToLowerInvariant() : "." + extension.ToLowerInvariant())
            .ToArray();
    }

    private static string NormalizePath(string path) =>
        path.Replace('\\', '/').Trim();
}
