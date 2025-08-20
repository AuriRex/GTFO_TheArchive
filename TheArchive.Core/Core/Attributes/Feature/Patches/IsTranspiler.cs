using System;

namespace TheArchive.Core.Attributes.Feature.Patches;

/// <summary>
/// Identifies a method defined in a <c>ArchivePatch</c> type to be the transpiler method.<br/>
/// Transpiles the IL of the original method.<br/>
/// <a href="https://github.com/BepInEx/HarmonyX/wiki/Transpiler-helpers">HarmonyX Transpiler Helpers Documentation</a>
/// </summary>
/// <seealso cref="ArchivePatch"/>
/// <seealso href="https://github.com/BepInEx/HarmonyX/wiki">HarmonyX Wiki</seealso>
/// <seealso href="https://harmony.pardeike.net/articles/intro.html">Harmony 2 Wiki</seealso>
/// <remarks>
/// <list>
/// <item>Used to decorate a method on a <see cref="ArchivePatch"/> type.</item>
/// <item>The method <b>must</b> be <c>static</c>.</item>
/// <item>Alternatively, name your method <c>Transpiler</c> to avoid having to use this attribute.</item>
/// <item><b>Only the first valid method on your type will be used!</b><br/>
/// Validity in this case includes matching <see cref="RundownConstraint"/> and <see cref="BuildConstraint"/></item>
/// <item><b>Can not be used on IL2CPP (game) types!</b></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class IsTranspiler : Attribute
{
}