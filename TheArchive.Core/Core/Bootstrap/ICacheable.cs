using System.IO;

namespace TheArchive.Core.Bootstrap;

public interface ICacheable
{
    void Save(BinaryWriter bw);

    void Load(BinaryReader br);
}
