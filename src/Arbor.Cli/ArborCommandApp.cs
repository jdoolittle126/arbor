using System;
using System.Linq;
using Arbor.Cli.Commands;
using Spectre.Console.Cli;

namespace Arbor.Cli;

public static class ArborCommandAppFactory
{
    public static CommandApp Create(ITypeRegistrar? typeRegistrar = null)
    {
        var app = typeRegistrar is null
            ? new CommandApp()
            : new CommandApp(typeRegistrar);

        app.Configure(Configure);

        return app;
    }

    public static void Configure(IConfigurator config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.SetApplicationName("arbor");
        config.AddCommand<TreeCommand>("tree")
            .WithDescription("Render a directory structure similar to the Windows tree command.")
            .WithExample("tree")
            .WithExample("tree", "-f")
            .WithExample("tree", "--depth", "2");
        config.AddCommand<InteractiveCommand>("interactive")
            .WithDescription("Launch an interactive session for selecting and exporting nodes.")
            .WithExample("interactive")
            .WithExample("interactive", "--files");
        config.AddCommand<VersionCommand>("version")
            .WithDescription("Display the Arbor CLI version.")
            .WithExample("version");
    }
}

public static class ArborCommandAppHost
{
    public static int Run(CommandApp app, string[] args)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(args);

        var normalizedArguments = NormalizeArguments(args);
        return app.Run(normalizedArguments);
    }

    public static string[] NormalizeArguments(string[] args)
    {
        if (args.Length == 0)
        {
            return ["tree"];
        }

        var command = args[0];

        if (string.Equals(command, "tree", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(command, "version", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(command, "interactive", StringComparison.OrdinalIgnoreCase))
        {
            return args;
        }
        

        return new[] { "tree" }.Concat(args).ToArray();
    }
}
