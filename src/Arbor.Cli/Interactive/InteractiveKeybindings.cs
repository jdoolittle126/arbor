namespace Arbor.Cli.Interactive;

internal static class InteractiveKeybindings
{
    public static string GetSummary() =>
        "select <idx>[,<idx>...]  toggle entries\n" +
        "open <idx>               enter directory\n" +
        "up                       go to parent\n" +
        "select-all               select visible entries\n" +
        "clear                    clear all selections\n" +
        "export                   export selection\n" +
        "quit                     exit session";
}
