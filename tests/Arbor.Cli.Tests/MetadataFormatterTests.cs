using Arbor.Cli.Exporting;
using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Tests;

public sealed class MetadataFormatterTests
{
    [Fact]
    public void FormatPlainInline_Should_FormatSizeAndTimestamp()
    {
        var metadata = new NodeMetadata(
            SizeBytes: 1024,
            LastModifiedUtc: new DateTimeOffset(2024, 1, 2, 3, 4, 0, TimeSpan.Zero));
        var fields = MetadataFields.Size | MetadataFields.Modified;

        var result = MetadataFormatter.FormatPlainInline(metadata, fields);

        Assert.Equal(" (1 KB, 2024-01-02 03:04 UTC)", result);
    }

    [Fact]
    public void FormatPlainInline_Should_ReturnEmpty_WhenNoSegments()
    {
        var result = MetadataFormatter.FormatPlainInline(NodeMetadata.Empty, MetadataFields.Size | MetadataFields.Modified);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void FormatMarkup_Should_WrapSegmentsInGreyMarkup()
    {
        var metadata = new NodeMetadata(Owner: "[admin]");

        var result = MetadataFormatter.FormatMarkup(metadata, MetadataFields.Owner);

        Assert.Contains("[grey]", result);
        Assert.Contains("owner=", result);
    }
}
