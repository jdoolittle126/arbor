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
}
