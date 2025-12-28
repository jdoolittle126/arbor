# Arbor CLI

Arbor is a cross-platform tree command that aims to modernize the default Windows `tree` utility. It renders colorful
directory graphs, understands glob and extension filters, and can surface file metadata such as sizes and timestamps.
The CLI is built on .NET 10, Spectre.Console, and a functional core that is unit-tested against an in-memory filesystem.

## Quick Start

- Restore dependencies: `dotnet restore Arbor.sln`
- Render the working directory: `dotnet run --project src/Arbor.Cli -- tree`
- Include files and default metadata: `dotnet run --project src/Arbor.Cli -- tree -f -d`
- Explore interactively: `dotnet run --project src/Arbor.Cli -- interactive --files`
- Filter output:
  `dotnet run --project src/Arbor.Cli -- tree -f --include "src/**/*.cs" --exclude "*.Designer.cs" --ext json`
- Target directories only: `dotnet run --project src/Arbor.Cli -- tree --include-dir src --exclude-dir tests`
- Export to JSON: `dotnet run --project src/Arbor.Cli -- tree -f --export json > tree.json`
- Print the CLI version: `dotnet run --project src/Arbor.Cli -- version`

## Feature Highlights

- Connector strategies swap between Unicode and ASCII guides while keeping the same traversal logic.
- Wildcard and extension filters operate on normalized relative paths, allowing fine-grained control over file
  visibility.
- Metadata renderers attach human-readable sizes and UTC timestamps without polluting the functional tree model.
- Exporters emit JSON or Markdown so you can pipe Arbor output into documentation or automation flows.
- An interactive mode lets you browse the tree, select nodes, and export just the files you care about.
- Advanced filters cover directories as well as files, and empty branches are hidden once everything is filtered out.
- Metadata output is configurable—add size/timestamp with `-d`, or extend with
  `--details-field perm --details-field owner` for richer context.
- Extensive tests use `System.IO.Abstractions.TestingHelpers` to mimic filesystem layouts without touching disk.

## Contributing

1. Use conventional commits (`feat:`, `fix:`, `docs:`) so upcoming GitVersion automation can derive semantic versions.
2. Run `dotnet test --project tests/Arbor.Cli.Tests/Arbor.Cli.Tests.csproj` locally; tests are fast and should stay green across Linux and Windows.
3. When adding features, prefer extending the core domain models and strategies, so we can keep Spectre-specific code thin
   and testable.

## Roadmap

See the [roadmap here!](ROADMAP.md)

Questions, bug reports, and suggestions are welcome. Open an issue or start a discussion to collaborate on Arbor’s
evolution.
