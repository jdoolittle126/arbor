using System;

namespace Arbor.Core.TreeBuilding;

[Flags]
public enum MetadataFields
{
    None = 0,
    Size = 1 << 0,
    Modified = 1 << 1,
    Permissions = 1 << 2,
    Owner = 1 << 3,
    Group = 1 << 4,
}
