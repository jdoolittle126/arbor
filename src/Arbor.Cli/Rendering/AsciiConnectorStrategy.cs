using Spectre.Console;

namespace Arbor.Cli.Rendering;

public sealed class AsciiConnectorStrategy : IConnectorStrategy
{
    public TreeGuide Guide => TreeGuide.Ascii;
}
