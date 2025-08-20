using System;

namespace TheArchive.Core.Attributes.Feature.Patches;

/// <summary>
/// Identifies a method defined in a <c>ArchivePatch</c> type to be the method that provides the type to patch.
/// </summary>
/// <seealso cref="ArchivePatch"/>
/// <remarks>
/// <list>
/// <item>Used to decorate a method on a <see cref="ArchivePatch"/> type.</item>
/// <item>The method <b>must</b> be <c>static</c>.</item>
/// <item>Alternatively, name your method <c>Type</c> to avoid having to use this attribute.</item>
/// <item><b>Only the first valid method on your type will be used!</b><br/>
/// Validity in this case includes matching <see cref="RundownConstraint"/> and <see cref="BuildConstraint"/></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class IsTypeProvider : Attribute
{
}