using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheArchive.Core.Bootstrap;
using TheArchive.Utilities;

namespace TheArchive.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ArchiveIncompatibility : Attribute, ICacheable
{
    public ArchiveIncompatibility(string IncompatibilityGUID)
    {
        this.IncompatibilityGUID = IncompatibilityGUID;
    }

    public string IncompatibilityGUID { get; protected set; }

    internal static IEnumerable<ArchiveIncompatibility> FromCecilType(TypeDefinition td)
    {
        return (from customAttribute in MetadataHelper.GetCustomAttributes<ArchiveIncompatibility>(td, true)
                select new ArchiveIncompatibility((string)customAttribute.ConstructorArguments[0].Value)).ToList();
    }

    public void Save(BinaryWriter bw)
    {
        bw.Write(IncompatibilityGUID);
    }

    public void Load(BinaryReader br)
    {
        IncompatibilityGUID = br.ReadString();
    }
}
