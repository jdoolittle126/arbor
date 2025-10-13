using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.Cli.Commands;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using TreeNode = Arbor.Core.TreeBuilding.TreeNode;

namespace Arbor.Cli.Interactive;

internal static class InteractiveStatusPanel
{
    public static void Render(IAnsiConsole console, TreeNode current, int selectedCount, TreeOptions options, BoxBorder border)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(options);

        var panel = new Panel(BuildMarkup(current, selectedCount, options))
        {
            Header = new PanelHeader("Interactive Mode"),
            Border = border,
            Expand = true,
        };

        console.Write(panel);
    }

    private static Markup BuildMarkup(TreeNode current, int selectedCount, TreeOptions options)
    {
        var path = string.IsNullOrEmpty(current.RelativePath) ? current.Name : current.RelativePath;
        var lines = new List<string>
        {
            $"[blue]Path:[/] {Markup.Escape(path)}",
            $"[blue]Selected:[/] {selectedCount}",
            BuildFilterSummary(options),
            "[blue]Commands:[/]"
        };

        lines.AddRange(InteractiveKeybindings.GetSummary().Split('\n').Select(line => $"  [grey]{Markup.Escape(line)}[/]"));

        return new Markup(string.Join('\n', lines));
    }

    private static string BuildFilterSummary(TreeOptions options)
    {
        var parts = new List<string>();
        if (options.IncludeFiles)
        {
            parts.Add("files=on");
        }
        var metadataSummary = DescribeMetadata(options.Metadata);
        if (!string.IsNullOrEmpty(metadataSummary))
        {
            parts.Add(metadataSummary);
        }
        if (options.ConnectorStyle == ConnectorStyle.Ascii)
        {
            parts.Add("ascii");
        }
        if (options.MaxDepth.HasValue)
        {
            parts.Add($"depth={options.MaxDepth.Value}");
        }
        AppendList(parts, "include-dir", options.NormalizedIncludeDirectories);
        AppendList(parts, "exclude-dir", options.NormalizedExcludeDirectories);
        AppendList(parts, "include", options.NormalizedIncludePatterns);
        AppendList(parts, "exclude", options.NormalizedExcludePatterns);
        AppendList(parts, "ext", options.NormalizedExtensions);

        return parts.Count == 0
            ? "[blue]Filters:[/] (none)"
            : "[blue]Filters:[/] " + string.Join(" | ", parts.Select(Markup.Escape));
    }

    private static string? DescribeMetadata(MetadataFields fields)
    {
        if (fields == MetadataFields.None)
        {
            return null;
        }

        var (useDefault, additional) = MetadataFieldsParser.Describe(fields);
        var tokens = new List<string>();
        if (useDefault)
        {
            tokens.Add("size,time");
        }

        tokens.AddRange(additional);

        return tokens.Count == 0 ? null : $"details={string.Join(',', tokens)}";
    }

    private static void AppendList(List<string> parts, string label, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        parts.Add($"{label}={string.Join(',', values)}");
    }
}
