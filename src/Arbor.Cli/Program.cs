using System;
using Arbor.Cli;
using Spectre.Console;

var app = ArborCommandAppFactory.Create();

try
{
    return ArborCommandAppHost.Run(app, args);
}
catch (Exception exception)
{
    AnsiConsole.MarkupLine($"[red]Unhandled error:[/] {exception.Message}");
    return -99;
}
