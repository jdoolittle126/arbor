using System;
using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Rendering;

public static class ConnectorStrategyFactory
{
    public static IConnectorStrategy FromOptions(TreeOptions options) => options.ConnectorStyle switch
    {
        ConnectorStyle.Ascii => new AsciiConnectorStrategy(),
        ConnectorStyle.Unicode => new UnicodeConnectorStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(options.ConnectorStyle), options.ConnectorStyle, "Unsupported connector style"),
    };
}
