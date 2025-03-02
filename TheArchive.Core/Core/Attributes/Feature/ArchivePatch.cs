using System;

namespace TheArchive.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
/// <summary>
/// A custom wrapper for Harmony patches used by the FeaturesAPI
/// </summary>
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

    public bool HasType
    {
        get
        {
            return Type != null;
        }
    }

    public Type Type { get; internal set; }

    public string MethodName { get; internal set; }

    public Type[] ParameterTypes { get; internal set; }

    public PatchMethodType MethodType { get; internal set; } = PatchMethodType.Method;

    public int Priority { get; set; }
    
    /// <summary>
    /// Describes what method to patch.
    /// <br/><br/>
    /// Type must be provided via a method marked with the <see cref="IsTypeProvider"/> Attribute inside of your type!
    /// </summary>
    /// <param name="methodName">The method name to patch</param>
    /// <param name="parameterTypes">Method parameters to distinguish between overloads</param>
    /// <param name="patchMethodType">Method type</param>
    /// /// <param name="priority">Patch priority</param>
    public ArchivePatch(string methodName, Type[] parameterTypes = null, PatchMethodType patchMethodType = PatchMethodType.Method, int priority = -1) : this(null, methodName, parameterTypes, patchMethodType, priority)
    {

    }

    /// <summary>
    /// Describes what method to patch.
    /// </summary>
    /// <param name="type">The type the method is on</param>
    /// <param name="methodName">The method name to patch</param>
    /// <param name="parameterTypes">Method parameters to distinguish between overloads</param>
    /// <param name="patchMethodType">Method type</param>
    /// <param name="priority">Patch priority</param>
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

    public enum PatchMethodType
    {
        Method,
        Getter,
        Setter,
        Constructor
    }
}