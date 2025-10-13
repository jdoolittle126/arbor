using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.Core.TreeBuilding;
using Spectre.Console;

namespace Arbor.Cli.Exporting;

internal static class MetadataFormatter
{
    public static string FormatPlainInline(NodeMetadata metadata, MetadataFields fields)
    {
        var segments = BuildSegments(metadata, fields);
        return segments.Count == 0 ? string.Empty : $" ({string.Join(", ", segments)})";
    }

    public static string FormatMarkup(NodeMetadata metadata, MetadataFields fields)
    {
        var segments = BuildSegments(metadata, fields);
        return segments.Count == 0
            ? string.Empty
            : $" [grey]({string.Join(", ", segments.Select(Markup.Escape))})[/]";
    }

    public static IReadOnlyList<string> BuildSegments(NodeMetadata metadata, MetadataFields fields)
    {
        var segments = new List<string>();

        if (fields.HasFlag(MetadataFields.Size) && metadata.SizeBytes.HasValue)
        {
            segments.Add(FormatSize(metadata.SizeBytes.Value));
        }

        if (fields.HasFlag(MetadataFields.Modified) && metadata.LastModifiedUtc.HasValue)
        {
            segments.Add(metadata.LastModifiedUtc.Value.UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'"));
        }

        if (fields.HasFlag(MetadataFields.Permissions) && !string.IsNullOrWhiteSpace(metadata.Permissions))
        {
            segments.Add($"perm={metadata.Permissions}");
        }

        if (fields.HasFlag(MetadataFields.Owner) && !string.IsNullOrWhiteSpace(metadata.Owner))
        {
            segments.Add($"owner={metadata.Owner}");
        }

        if (fields.HasFlag(MetadataFields.Group) && !string.IsNullOrWhiteSpace(metadata.Group))
        {
            segments.Add($"group={metadata.Group}");
        }

        return segments;
    }

    private static string FormatSize(long bytes)
    {
        const long kilo = 1024;
        const long mega = kilo * 1024;
        const long giga = mega * 1024;

        return bytes switch
        {
            < kilo => $"{bytes} B",
            < mega => $"{bytes / (double)kilo:0.##} KB",
            < giga => $"{bytes / (double)mega:0.##} MB",
            _ => $"{bytes / (double)giga:0.##} GB",
        };
    }
}
