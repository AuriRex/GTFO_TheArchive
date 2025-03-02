using System;

namespace TheArchive.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
/// <summary>
/// A custom wrapper for Harmony patches used by the FeaturesAPI<br/>
/// Shorthand for constructor patching
/// </summary>
internal class ArchiveConstructorPatch : ArchivePatch
{
    /// <summary>
    /// Describes what constructor to patch.
    /// </summary>
    /// <param name="type">The type the constructor is on</param>
    /// <param name="parameterTypes">Constructor parameters to distinguish between overloads</param>
    public ArchiveConstructorPatch(Type type, Type[] parameterTypes = null) : base(type, null, parameterTypes, PatchMethodType.Constructor)
    {

    }
}