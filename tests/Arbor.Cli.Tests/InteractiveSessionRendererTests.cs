using Arbor.Cli.Interactive;
using Arbor.Core.TreeBuilding;
using Spectre.Console.Testing;

namespace Arbor.Cli.Tests;

public sealed class InteractiveSessionRendererTests
{
    [Fact]
    public void Render_Should_NotThrow_When_UnicodeDisabled()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Unicode = false;

        var node = TreeNode.Directory("root", string.Empty, NodeMetadata.Empty, Array.Empty<TreeNode>());
        var renderer = new InteractiveSessionRenderer(console, TreeOptions.Default);

        renderer.Render(node, Array.Empty<string>(), Array.Empty<TreeNode>());

        Assert.Contains("Commands", console.Output);
    }
}
