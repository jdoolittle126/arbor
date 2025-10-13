using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arbor.Core.TreeBuilding;
using Spectre.Console;
using TreeNode = Arbor.Core.TreeBuilding.TreeNode;

namespace Arbor.Cli.Interactive;

internal sealed class InteractiveSession
{
    private readonly IAnsiConsole _console;
    private readonly TreeNode _root;
    private readonly TreeOptions _options;
    private readonly Queue<string> _scriptCommands;
    private readonly HashSet<string> _selected = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<TreeNode> _breadcrumb = new();

    private TreeNode _current;

    public InteractiveSession(IAnsiConsole console, TreeNode root, TreeOptions options, IEnumerable<string> script)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(script);

        _console = console;
        _root = root;
        _options = options;
        _current = root;
        _scriptCommands = new Queue<string>(script.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public InteractiveSessionResult Run()
    {
        var renderer = new InteractiveSessionRenderer(_console, _options);

        while (true)
        {
            var children = _current.Children;
            renderer.Render(_current, _selected, children);

            var command = GetNextCommand();
            if (command is null)
            {
                return InteractiveSessionResult.Quit(_selected);
            }

            var trimmed = command.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            var parts = trimmed.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var verb = parts[0].ToLowerInvariant();
            var argument = parts.Length > 1 ? parts[1] : string.Empty;

            switch (verb)
            {
                case "help":
                case "?":
                    RenderHelp();
                    break;
                case "ls":
                case "list":
                    // already rendered; loop again to refresh view
                    break;
                case "open":
                case "cd":
                    var firstIndex = ParseIndices(argument, children.Count).FirstOrDefault(i => i >= 0);
                    if (firstIndex >= 0)
                    {
                        EnterChild(children[firstIndex]);
                    }
                    break;
                case "up":
                case "back":
                    NavigateUp();
                    break;
                case "select":
                    var indices = ParseIndices(argument, children.Count);
                    if (indices.Count == 0)
                    {
                        _console.MarkupLine("[yellow]No valid indices provided.[/]");
                        break;
                    }

                    foreach (var index in indices)
                    {
                        ToggleSelection(children[index]);
                    }
                    break;
                case "select-all":
                    SelectAll(children);
                    break;
                case "clear":
                    _selected.Clear();
                    break;
                case "export":
                    if (_selected.Count == 0)
                    {
                        _console.MarkupLine("[yellow]No nodes selected. Use `select <index>` before exporting.[/]");
                        break;
                    }

                    var finalSelection = new HashSet<string>(_selected, StringComparer.OrdinalIgnoreCase);
                    var filterCommand = FilterCommandEmitter.CreateFilterCommand(finalSelection, _options, _root.RelativePath);
                    return InteractiveSessionResult.Export(finalSelection, filterCommand);
                case "quit":
                case "exit":
                    return InteractiveSessionResult.Quit(_selected);
                default:
                    _console.MarkupLine($"[yellow]Unknown command[/]: {verb}. Type `help` for options.");
                    break;
            }
        }
    }

    private void RenderHelp()
    {
        _console.MarkupLine("[blue]Commands:[/]");
        _console.MarkupLine("  [grey]select <index>[/] - toggle selection for an entry");
        _console.MarkupLine("  [grey]open <index>|cd <index>[/] - enter a directory");
        _console.MarkupLine("  [grey]up[/] - move to the parent directory");
        _console.MarkupLine("  [grey]select-all[/] - select all visible entries");
        _console.MarkupLine("  [grey]clear[/] - clear all selections");
        _console.MarkupLine("  [grey]export[/] - export the current selection");
        _console.MarkupLine("  [grey]quit[/] - exit without exporting");
    }

    private string? GetNextCommand()
    {
        if (_scriptCommands.Count > 0)
        {
            var next = _scriptCommands.Dequeue();
            _console.MarkupLine($"[grey]> {Markup.Escape(next)}[/]");
            return next;
        }

        try
        {
            return _console.Prompt(new TextPrompt<string>("[green]interactive>[/] ").AllowEmpty());
        }
        catch (IOException)
        {
            return null;
        }
    }

    private void EnterChild(TreeNode child)
    {
        if (child.Kind != NodeKind.Directory)
        {
            ToggleSelection(child);
            return;
        }

        _breadcrumb.Push(_current);
        _current = child;
    }

    private void NavigateUp()
    {
        if (_breadcrumb.Count == 0)
        {
            _console.MarkupLine("[yellow]Already at root.[/]");
            return;
        }

        _current = _breadcrumb.Pop();
    }

    private void ToggleSelection(TreeNode node)
    {
        var key = Normalize(node.RelativePath);
        if (_selected.Contains(key))
        {
            _selected.Remove(key);
        }
        else
        {
            _selected.Add(key);
        }
    }

    private void SelectAll(IReadOnlyList<TreeNode> children)
    {
        foreach (var child in children)
        {
            _selected.Add(Normalize(child.RelativePath));
        }
    }

    private static IReadOnlyList<int> ParseIndices(string argument, int count)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            return Array.Empty<int>();
        }

        var separators = new[] { ',', ' ' };
        var tokens = argument.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var indices = new List<int>();

        foreach (var token in tokens)
        {
            if (int.TryParse(token, out var index) && index >= 0 && index < count)
            {
                indices.Add(index);
            }
        }

        return indices;
    }

    private static string Normalize(string path) =>
        string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');
}
