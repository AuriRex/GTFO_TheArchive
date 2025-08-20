using System;

namespace TheArchive.Core.Attributes.Feature.Patches;

/// <summary>
/// Describes what constructor to patch.
/// </summary>
/// <remarks>
/// Must be used on a <c>nested static class</c> in your <c>Feature</c> type.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
internal class ArchiveConstructorPatch : ArchivePatch
{
    /// <summary>
    /// Describes what constructor to patch.
    /// </summary>
    /// <remarks>
    /// Must be used on a <c>nested static class</c> in your <c>Feature</c> type.
    /// </remarks>
    /// <param name="type">The type the constructor is on</param>
    /// <param name="parameterTypes">Constructor parameters to distinguish between overloads</param>
    public ArchiveConstructorPatch(Type type, Type[] parameterTypes = null) : base(type, null, parameterTypes, PatchMethodType.Constructor)
    {

    }
}