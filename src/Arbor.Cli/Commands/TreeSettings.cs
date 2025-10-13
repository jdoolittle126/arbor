using System;
using System.ComponentModel;
using Arbor.Cli.Exporting;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Arbor.Cli.Commands;

public sealed class TreeSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("Root directory to enumerate. Defaults to the current working directory.")]
    public string? RootPath { get; init; }

    [CommandOption("-f|--files")]
    [Description("Include files in the tree output (similar to `tree /f`).")]
    public bool IncludeFiles { get; init; }

    [CommandOption("--depth <LEVEL>")]
    [Description("Limit traversal depth. 0 shows only the root, 1 includes direct children, etc.")]
    public int? MaxDepth { get; init; }

    [CommandOption("--ascii")]
    [Description("Render connectors using ASCII characters (similar to `tree /a`).")]
    public bool Ascii { get; init; }

    [CommandOption("-d|--details")]
    [Description("Display default metadata (size and timestamp).")]
    public bool UseDefaultDetails { get; init; }

    [CommandOption("--details-field <FIELD>")]
    [Description("Include specific metadata fields (size, time, perm, owner, group). Supports comma-separated values.")]
    public string[] DetailFields { get; init; } = [];

    [CommandOption("--include <GLOB>")]
    [Description("Include only files matching the provided glob pattern (supports multiple).")]
    public string[] IncludePatterns { get; init; } = [];

    [CommandOption("--exclude <GLOB>")]
    [Description("Exclude files matching the provided glob pattern (supports multiple).")]
    public string[] ExcludePatterns { get; init; } = [];

    [CommandOption("-e|--ext <EXTENSION>")]
    [Description("Filter files by extension, e.g. `--ext .cs` or `--ext json` (supports multiple).")]
    public string[] Extensions { get; init; } = [];

    [CommandOption("--include-dir <GLOB>")]
    [Description("Include only directories matching the provided glob pattern (supports multiple).")]
    public string[] IncludeDirectories { get; init; } = [];

    [CommandOption("--exclude-dir <GLOB>")]
    [Description("Exclude directories matching the provided glob pattern (supports multiple).")]
    public string[] ExcludeDirectories { get; init; } = [];

    [CommandOption("--export <FORMAT>")]
    [Description("Choose the export format: text, json, or markdown.")]
    public TreeExportFormat ExportFormat { get; init; } = TreeExportFormat.Text;

    [CommandOption("--output <PATH>")]
    [Description("Write the exported tree to a file instead of stdout.")]
    public string? OutputPath { get; init; }

    public override ValidationResult Validate() 
        => MaxDepth is < 0 
            ? ValidationResult.Error("Depth must be zero or a positive integer.") 
            : ValidationResult.Success();
}
