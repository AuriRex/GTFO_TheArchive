// This file is licensed under the LGPL 2.1 LICENSE
// See LICENSE_BepInEx in the projects root folder
// Original code from https://github.com/BepInEx/BepInEx

using System.IO;

namespace TheArchive.Core.Bootstrap;

/// <summary>
///     A cacheable metadata item. Can be used with <see cref="TypeLoader.LoadAssemblyCache{T}" /> and
///     <see cref="TypeLoader.SaveAssemblyCache{T}" /> to cache plugin metadata.
/// </summary>
public interface ICacheable
{
    /// <summary>
    ///     Serialize the object into a binary format.
    /// </summary>
    /// <param name="bw"></param>
    void Save(BinaryWriter bw);

    /// <summary>
    ///     Loads the object from binary format.
    /// </summary>
    /// <param name="br"></param>
    void Load(BinaryReader br);
}
