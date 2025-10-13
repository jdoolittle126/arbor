using Arbor.Core.TreeBuilding;

namespace Arbor.Cli.Commands;

internal static class MetadataFieldsParser
{
    private const MetadataFields DefaultFields = MetadataFields.Size | MetadataFields.Modified;

    private const MetadataFields AllFields = MetadataFields.Size | MetadataFields.Modified |
                                             MetadataFields.Permissions | MetadataFields.Owner | MetadataFields.Group;

    public static MetadataFields Parse(
        IEnumerable<string> fields,
        bool includeDefault)
    {
        var result = includeDefault ? DefaultFields : MetadataFields.None;

        foreach (var token in Split(fields))
        {
            switch (token)
            {
                case "default":
                    result |= DefaultFields;
                    break;
                case "all":
                    result |= AllFields;
                    break;
                case "none":
                    return MetadataFields.None;
                case "size":
                case "bytes":
                    result |= MetadataFields.Size;
                    break;
                case "time":
                case "modified":
                case "timestamp":
                    result |= MetadataFields.Modified;
                    break;
                case "perm":
                case "perms":
                case "permissions":
                    result |= MetadataFields.Permissions;
                    break;
                case "owner":
                    result |= MetadataFields.Owner;
                    break;
                case "group":
                    result |= MetadataFields.Group;
                    break;
            }
        }

        return result;
    }

    public static (bool UseDefault, IReadOnlyList<string> Additional) Describe(MetadataFields fields)
    {
        if (fields == MetadataFields.None)
        {
            return (false, Array.Empty<string>());
        }

        var useDefault = fields.HasFlag(MetadataFields.Size) && fields.HasFlag(MetadataFields.Modified);
        var remaining = fields;
        if (useDefault)
        {
            remaining &= ~(MetadataFields.Size | MetadataFields.Modified);
        }

        var tokens = new List<string>();
        if (remaining.HasFlag(MetadataFields.Size))
        {
            tokens.Add("size");
        }

        if (remaining.HasFlag(MetadataFields.Modified))
        {
            tokens.Add("time");
        }

        if (remaining.HasFlag(MetadataFields.Permissions))
        {
            tokens.Add("perm");
        }

        if (remaining.HasFlag(MetadataFields.Owner))
        {
            tokens.Add("owner");
        }

        if (remaining.HasFlag(MetadataFields.Group))
        {
            tokens.Add("group");
        }

        return (useDefault, tokens);
    }

    private static IEnumerable<string> Split(IEnumerable<string> fields)
    {
        return from field in fields ?? []
               where !string.IsNullOrWhiteSpace(field)
               from part in field.Split([',', ';'],
                   StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               select part.ToLowerInvariant();
    }
}