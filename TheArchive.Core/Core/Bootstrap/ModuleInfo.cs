using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheArchive.Core.Attributes;

namespace TheArchive.Core.Bootstrap;

public class ModuleInfo : ICacheable
{
    public ArchiveModule Metadata { get; internal set; }

    public IEnumerable<ArchiveDependency> Dependencies { get; internal set; }

    public IEnumerable<ArchiveIncompatibility> Incompatibilities { get; internal set; }

    public string Location { get; internal set; }

    public object Instance { get; internal set; }

    public string TypeName { get; internal set; }

    internal Version TargettedTheArchiveVersion { get; set; }

    public void Save(BinaryWriter bw)
    {
        bw.Write(TypeName);
        bw.Write(Location);
        bw.Write(Metadata.GUID);
        bw.Write(Metadata.Name);
        bw.Write(Metadata.Version.ToString());
        List<ArchiveDependency> depList = Dependencies.ToList();
        bw.Write(depList.Count);
        foreach (ArchiveDependency bepInDependency in depList)
        {
            ((ICacheable)bepInDependency).Save(bw);
        }
        List<ArchiveIncompatibility> incList = Incompatibilities.ToList();
        bw.Write(incList.Count);
        foreach (ArchiveIncompatibility bepInIncompatibility in incList)
        {
            ((ICacheable)bepInIncompatibility).Save(bw);
        }
        bw.Write(TargettedTheArchiveVersion.ToString(4));
    }

    public void Load(BinaryReader br)
    {
        TypeName = br.ReadString();
        Location = br.ReadString();
        Metadata = new ArchiveModule(br.ReadString(), br.ReadString(), br.ReadString());
        int depCount = br.ReadInt32();
        List<ArchiveDependency> depList = new List<ArchiveDependency>(depCount);
        for (int j = 0; j < depCount; j++)
        {
            ArchiveDependency dep = new ArchiveDependency(string.Empty, ArchiveDependency.DependencyFlags.HardDependency);
            ((ICacheable)dep).Load(br);
            depList.Add(dep);
        }
        Dependencies = depList;
        int incCount = br.ReadInt32();
        List<ArchiveIncompatibility> incList = new List<ArchiveIncompatibility>(incCount);
        for (int k = 0; k < incCount; k++)
        {
            ArchiveIncompatibility inc = new ArchiveIncompatibility(string.Empty);
            ((ICacheable)inc).Load(br);
            incList.Add(inc);
        }
        Incompatibilities = incList;
        TargettedTheArchiveVersion = new Version(br.ReadString());
    }

    public override string ToString()
    {
        string text = "{0} {1}";
        ArchiveModule metadata = Metadata;
        object obj = metadata != null ? metadata.Name : null;
        ArchiveModule metadata2 = Metadata;
        return string.Format(text, obj, metadata2 != null ? metadata2.Version : null);
    }
}
