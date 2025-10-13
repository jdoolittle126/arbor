using System;
using System.Collections.Generic;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using TreeNode = Arbor.Core.TreeBuilding.TreeNode;

namespace Arbor.Cli.Interactive;

internal sealed class InteractiveSessionRenderer
{
    private readonly IAnsiConsole _console;
    private readonly TreeOptions _options;
    private readonly bool _unicodeCapable;
    private readonly string _selectedMark;
    private readonly string _unselectedMark;

    private readonly BoxBorder _border;

    public InteractiveSessionRenderer(IAnsiConsole console, TreeOptions options)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(options);

        _console = console;
        _options = options;
        _unicodeCapable = console.Profile.Capabilities.Unicode;

        _selectedMark = _unicodeCapable ? "[green]✔[/]" : "[green]x[/]";
        _unselectedMark = _unicodeCapable ? "[grey]·[/]" : "[grey].[/]";
        _border = _unicodeCapable ? BoxBorder.Rounded : BoxBorder.Ascii;
    }

    public void Render(TreeNode current, IReadOnlyCollection<string> selected, IReadOnlyList<TreeNode> children)
    {
        InteractiveStatusPanel.Render(_console, current, selected.Count, _options, _border);
        RenderChildren(children, selected);
    }

    private void RenderChildren(IReadOnlyList<TreeNode> children, IReadOnlyCollection<string> selected)
    {
        if (children.Count == 0)
        {
            _console.MarkupLine("[grey](no children)[/]");
            return;
        }

        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var key = Normalize(child.RelativePath);
            var isSelected = selected.Contains(key);
            var selectedMark = isSelected ? _selectedMark : _unselectedMark;
            var label = child.Kind == NodeKind.Directory ? $"[yellow]{child.Name}[/]" : child.Name;
            _console.MarkupLine($"{i,2}. {selectedMark} {label}");
        }
    }

    private static string Normalize(string path) =>
        string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');
}
