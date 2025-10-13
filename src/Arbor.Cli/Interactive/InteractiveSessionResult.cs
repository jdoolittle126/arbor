using System.Collections.Generic;

namespace Arbor.Cli.Interactive;

internal sealed record InteractiveSessionResult(InteractiveSessionStatus Status, HashSet<string> Selected, string? SuggestedCommand)
{
    public static InteractiveSessionResult Export(HashSet<string> selected, string? suggestedCommand) =>
        new(InteractiveSessionStatus.Export, selected, suggestedCommand);

    public static InteractiveSessionResult Quit(HashSet<string> selected) =>
        new(InteractiveSessionStatus.Quit, selected, null);
}

internal enum InteractiveSessionStatus
{
    Export,
    Quit,
}
