using System;
using JetBrains.Annotations;

namespace TheArchive.Core.Attributes.Feature.Patches;

/// <summary>
/// Describes what method to patch.<br/>
/// A tool built on top of Harmony; See: <a href="https://github.com/BepInEx/HarmonyX/wiki">HarmonyX Documentation</a>
/// </summary>
/// <seealso href="https://github.com/BepInEx/HarmonyX/wiki">HarmonyX Wiki</seealso>
/// <seealso href="https://harmony.pardeike.net/articles/intro.html">Harmony 2 Wiki</seealso>
/// <remarks>
/// <list>
/// <item>Must be used on a <c>nested static class</c> in your <c>Feature</c> type.</item>
/// </list>
/// </remarks>
/// <example><code>
/// public class MyFeature : Feature
/// {
///     ...
///     [ArchivePatch(typeof(TypeToPatch), nameof(TypeToPatch.MethodToPatch))]
///     internal static class MyPatchType
///     {
///         public static void Prefix()
///         {
///
///         }
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
public class ArchivePatch : Attribute
{
    /// <summary>
    /// Used on Prefix patches as a more readable substitute for plain <c>return false</c>.<br/>
    /// The original method the patch is targeting is NOT going to run.
    /// </summary>
    public const bool SKIP_OG = false;
    /// <summary>
    /// Used on Prefix patches as a more readable substitute for plain <c>return true</c>.<br/>
    /// The original method the patch is targeting IS going to run.
    /// </summary>
    public const bool RUN_OG = true;

    /// <summary>
    /// If the patch target type has been set.
    /// </summary>
    public bool HasType => Type != null;

    /// <summary>
    /// The patch target type containing the method to patch.
    /// </summary>
    public Type Type { get; internal set; }

    /// <summary>
    /// The name of the method to patch.
    /// </summary>
    public string MethodName { get; internal set; }

    /// <summary>
    /// The parameter types of the method to patch.<br/>
    /// (Optional)
    /// </summary>
    public Type[] ParameterTypes { get; internal set; }

    /// <summary>
    /// The kind of method to patch. (Method, Constructor, Getter, Setter)<br/>
    /// <seealso cref="PatchMethodType"/>
    /// </summary>
    public PatchMethodType MethodType { get; internal set; }

    /// <summary>
    /// The patch priority, lower is later.<br/>
    /// <c>400</c> is 'Normal' priority.
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Describes what method to patch.<br/>
    /// A tool built on top of Harmony; See: <a href="https://github.com/BepInEx/HarmonyX/wiki">HarmonyX Documentation</a>
    /// </summary>
    /// <seealso href="https://github.com/BepInEx/HarmonyX/wiki">HarmonyX Wiki</seealso>
    /// <seealso href="https://harmony.pardeike.net/articles/intro.html">Harmony 2 Wiki</seealso>
    /// <remarks>
    /// Must be used on a <c>nested static class</c> in your <c>Feature</c> type.<br/>
    /// Type must be provided via a method called <c>Type</c> <i>or</i> marked with the <see cref="IsTypeProvider"/> Attribute inside of your type!
    /// </remarks>
    /// <param name="methodName">The method name to patch</param>
    /// <param name="parameterTypes">Method parameters to distinguish between overloads</param>
    /// <param name="patchMethodType">Method type</param>
    /// <param name="priority">Patch priority</param>
    /// <example><code>
    /// public class MyFeature : Feature
    /// {
    ///     ...
    ///     [ArchivePatch(nameof(TypeToPatch.MethodToPatch))]
    ///     internal static class MyPatchType
    ///     {
    ///         public static Type Type() => typeof(TypeToPatch);
    /// 
    ///         public static void Prefix()
    ///         {
    ///
    ///         }
    ///     }
    /// }
    /// </code></example>
    public ArchivePatch(string methodName, Type[] parameterTypes = null, PatchMethodType patchMethodType = PatchMethodType.Method, int priority = -1) : this(null, methodName, parameterTypes, patchMethodType, priority)
    {

    }

    /// <summary>
    /// Describes what method to patch.<br/>
    /// A tool built on top of Harmony; See: <a href="https://github.com/BepInEx/HarmonyX/wiki">HarmonyX Documentation</a>
    /// </summary>
    /// <seealso href="https://github.com/BepInEx/HarmonyX/wiki">HarmonyX Wiki</seealso>
    /// <seealso href="https://harmony.pardeike.net/articles/intro.html">Harmony 2 Wiki</seealso>
    /// <remarks>
    /// Must be used on a <c>nested static class</c> in your <c>Feature</c> type.
    /// </remarks>
    /// <param name="type">The type the method is on</param>
    /// <param name="methodName">The method name to patch</param>
    /// <param name="parameterTypes">Method parameters to distinguish between overloads</param>
    /// <param name="patchMethodType">Method type</param>
    /// <param name="priority">Patch priority</param>
    /// <example><code>
    /// public class MyFeature : Feature
    /// {
    ///     ...
    ///
    ///     [ArchivePatch(typeof(TypeToPatch), nameof(TypeToPatch.MethodToPatch))]
    ///     internal static class MyPatchType
    ///     {
    ///         public static void Prefix()
    ///         {
    ///
    ///         }
    ///     }
    /// }
    /// </code></example>
    public ArchivePatch(Type type, string methodName, Type[] parameterTypes = null, PatchMethodType patchMethodType = PatchMethodType.Method, int priority = -1)
    {
        Type = type;
        MethodName = methodName;
        ParameterTypes = parameterTypes;
        MethodType = patchMethodType;
        Priority = priority;

        if(patchMethodType == PatchMethodType.Constructor)
        {
            MethodName = ".ctor";
        }
    }

    /// <summary>
    /// The kind of method.
    /// <list>
    /// <item>Method</item>
    /// <item>Getter</item>
    /// <item>Setter</item>
    /// <item>Constructor</item>
    /// </list>
    /// </summary>
    public enum PatchMethodType
    {
        /// <summary>
        /// A regular method
        /// </summary>
        Method,
        /// <summary>
        /// Getter method on a property
        /// </summary>
        Getter,
        /// <summary>
        /// Setter method on a property
        /// </summary>
        Setter,
        /// <summary>
        /// A constructor body
        /// </summary>
        Constructor
    }
}