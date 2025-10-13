using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using Mono.Unix;

namespace Arbor.Core.TreeBuilding;

internal static class MetadataCollector
{
    public static (string? Permissions, string? Owner, string? Group) GetMetadata(string path, MetadataFields fields)
    {
        if (fields == MetadataFields.None)
        {
            return (null, null, null);
        }

        try
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? GetWindowsMetadata(path, fields) 
                : GetUnixMetadata(path, fields);
        }
        catch
        {
            return (null, null, null);
        }
    }

    private static (string? Permissions, string? Owner, string? Group) GetUnixMetadata(string path, MetadataFields fields)
    {
        var info = new UnixFileInfo(path);
        string? permissions = null;
        string? owner = null;
        string? group = null;

        if (fields.HasFlag(MetadataFields.Permissions))
        {
            permissions = info.FileAccessPermissions.ToString();
        }

        if (fields.HasFlag(MetadataFields.Owner))
        {
            owner = info.OwnerUser?.UserName;
        }

        if (fields.HasFlag(MetadataFields.Group))
        {
            group = info.OwnerGroup?.GroupName;
        }

        return (permissions, owner, group);
    }

    [SupportedOSPlatform("windows")]
    private static (string? Permissions, string? Owner, string? Group) GetWindowsMetadata(string path, MetadataFields fields)
    {
        FileSystemSecurity? security = null;
        string? permissions = null;
        string? owner = null;
        string? group = null;

        if (fields.HasFlag(MetadataFields.Permissions) || fields.HasFlag(MetadataFields.Owner) || fields.HasFlag(MetadataFields.Group))
        {
            security = File.GetAttributes(path) switch
            {
                FileAttributes.Directory => new DirectoryInfo(path).GetAccessControl(),
                _ => new FileInfo(path).GetAccessControl(),
            };
        }

        if (fields.HasFlag(MetadataFields.Permissions) && security is not null)
        {
            var rules = security.GetAccessRules(includeExplicit: true, includeInherited: true, typeof(NTAccount));
            permissions = rules?.Count > 0 ? $"ACL:{rules.Count}" : null;
        }

        if (fields.HasFlag(MetadataFields.Owner) && security is not null)
        {
            owner = security.GetOwner(typeof(NTAccount))?.Value;
        }

        if (fields.HasFlag(MetadataFields.Group) && security is not null)
        {
            group = security.GetGroup(typeof(NTAccount))?.Value;
        }

        return (permissions, owner, group);
    }
}
