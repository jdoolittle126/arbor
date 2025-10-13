using System;

namespace Arbor.Core.TreeBuilding;

public sealed record NodeMetadata(
    long? SizeBytes = null,
    DateTimeOffset? LastModifiedUtc = null,
    string? Permissions = null,
    string? Owner = null,
    string? Group = null)
{
    public static NodeMetadata Empty { get; } = new();
}
