// This file is licensed under the LGPL 2.1 LICENSE
// See LICENSE_BepInEx in the projects root folder
// Original code from https://github.com/BepInEx/BepInEx

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TheArchive.Core.Bootstrap;

/// <summary>
///     A cached assembly.
/// </summary>
/// <typeparam name="T"></typeparam>
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class CachedAssembly<T> where T : ICacheable
{
    /// <summary>
    ///     List of cached items inside the assembly.
    /// </summary>
    public List<T> CacheItems { get; set; }

    /// <summary>
    ///     Hash of the assembly. Used to verify that the assembly hasn't been changed.
    /// </summary>
    public string Hash { get; set; }
}
