using System;
using System.ComponentModel;
using Arbor.Cli.Exporting;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Arbor.Cli.Commands;

public sealed class InteractiveSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("Root directory to explore. Defaults to the current working directory.")]
    public string? RootPath { get; init; }

    [CommandOption("-f|--files")]
    [Description("Include files in the interactive view (similar to `tree /f`).")]
    public bool IncludeFiles { get; init; }

    [CommandOption("--depth <LEVEL>")]
    [Description("Limit traversal depth for the initial tree build.")]
    public int? MaxDepth { get; init; }

    [CommandOption("--ascii")]
    [Description("Render connectors using ASCII characters in generated exports.")]
    public bool Ascii { get; init; }

    [CommandOption("-d|--details")]
    [Description("Display default metadata (size and timestamp).")]
    public bool UseDefaultDetails { get; init; }

    [CommandOption("--details-field <FIELD>")]
    [Description("Include specific metadata fields (size, time, perm, owner, group). Supports comma-separated values.")]
    public string[] DetailFields { get; init; } = Array.Empty<string>();

    [CommandOption("--include <GLOB>")]
    [Description("Pre-filter to files matching the provided glob (supports multiple).")]
    public string[] IncludePatterns { get; init; } = Array.Empty<string>();

    [CommandOption("--exclude <GLOB>")]
    [Description("Exclude files matching the provided glob (supports multiple).")]
    public string[] ExcludePatterns { get; init; } = Array.Empty<string>();

    [CommandOption("-e|--ext <EXTENSION>")]
    [Description("Filter files by extension (supports multiple).")]
    public string[] Extensions { get; init; } = Array.Empty<string>();

    [CommandOption("--include-dir <GLOB>")]
    [Description("Pre-filter to directories matching the provided glob (supports multiple).")]
    public string[] IncludeDirectories { get; init; } = Array.Empty<string>();

    [CommandOption("--exclude-dir <GLOB>")]
    [Description("Exclude directories matching the provided glob (supports multiple).")]
    public string[] ExcludeDirectories { get; init; } = Array.Empty<string>();

    [CommandOption("--export <FORMAT>")]
    [Description("Set the export format for the selected subset (text, json, markdown).")]
    public TreeExportFormat ExportFormat { get; init; } = TreeExportFormat.Text;

    [CommandOption("--output <PATH>")]
    [Description("Write the exported subset to a file instead of stdout.")]
    public string? OutputPath { get; init; }

    [CommandOption("--script <COMMAND>")]
    [Description("Provide scripted commands for non-interactive/testing scenarios.")]
    public string[] Script { get; init; } = Array.Empty<string>();

    public override ValidationResult Validate()
        => MaxDepth is < 0
            ? ValidationResult.Error("Depth must be zero or a positive integer.")
            : ValidationResult.Success();
}