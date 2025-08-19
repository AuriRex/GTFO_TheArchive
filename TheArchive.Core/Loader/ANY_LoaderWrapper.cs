using System;

namespace TheArchive.Loader;

/// <summary>
/// A wrapper for different mod loaders.
/// </summary>
public static partial class LoaderWrapper
{
    /// <summary>
    /// Check if a type is an Il2Cpp type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns><c>True</c> if a type is an Il2Cpp type.</returns>
    public static bool IsIL2CPPType(Type type)
    {
        if (!IsGameIL2CPP())
            return false;

        return ArchiveMod.IL2CPP_BaseType.IsAssignableFrom(type);
    }
}