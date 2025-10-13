using Spectre.Console;

namespace Arbor.Cli.Rendering;

public sealed class UnicodeConnectorStrategy : IConnectorStrategy
{
    public TreeGuide Guide => TreeGuide.Line;
}
