using System;
using Arbor.Cli.Commands;
using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Tests;

public sealed class MetadataFieldsParserTests
{
    [Fact]
    public void Parse_Should_ReturnDefault_WhenRequested()
    {
        var result = MetadataFieldsParser.Parse(Array.Empty<string>(), includeDefault: true);
        Assert.Equal(MetadataFields.Size | MetadataFields.Modified, result);
    }

    [Fact]
    public void Parse_Should_HandleSpecificFields()
    {
        var result = MetadataFieldsParser.Parse(new[] { "perm,owner" }, includeDefault: false);
        Assert.Equal(MetadataFields.Permissions | MetadataFields.Owner, result);
    }

    [Fact]
    public void Describe_ShouldReturnDefaultAndAdditional()
    {
        var fields = MetadataFields.Size | MetadataFields.Modified | MetadataFields.Owner;
        var (useDefault, additional) = MetadataFieldsParser.Describe(fields);

        Assert.True(useDefault);
        Assert.Contains("owner", additional);
    }

    [Fact]
    public void Parse_Should_ReturnNone_WhenNoneSpecified()
    {
        var result = MetadataFieldsParser.Parse(new[] { "none", "size" }, includeDefault: true);

        Assert.Equal(MetadataFields.None, result);
    }

    [Fact]
    public void Parse_Should_ReturnAll_WhenAllSpecified()
    {
        var result = MetadataFieldsParser.Parse(new[] { "all" }, includeDefault: false);

        Assert.True(result.HasFlag(MetadataFields.Size));
        Assert.True(result.HasFlag(MetadataFields.Modified));
        Assert.True(result.HasFlag(MetadataFields.Permissions));
        Assert.True(result.HasFlag(MetadataFields.Owner));
        Assert.True(result.HasFlag(MetadataFields.Group));
    }

    [Fact]
    public void Describe_Should_ReturnEmpty_WhenNone()
    {
        var (useDefault, additional) = MetadataFieldsParser.Describe(MetadataFields.None);

        Assert.False(useDefault);
        Assert.Empty(additional);
    }
}
