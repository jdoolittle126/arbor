using Spectre.Console;

namespace Arbor.Cli.Rendering;

public interface IConnectorStrategy
{
    TreeGuide Guide { get; }
}
