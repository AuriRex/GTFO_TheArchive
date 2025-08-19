using System;

namespace TheArchive.Core.Attributes;

/// <summary>
/// Override the assemblies default feature group name.
/// </summary>
/// <remarks>
/// Uses the assembly name instead if this is unused.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public class ModDefaultFeatureGroupName : Attribute
{
    /// <summary>
    /// The default group name.
    /// </summary>
    public string DefaultGroupName { get; private set; }

    /// <summary>
    /// Override the assemblies default feature group name.
    /// </summary>
    /// <param name="defaultGroupName">The default group name.</param>
    public ModDefaultFeatureGroupName(string defaultGroupName)
    {
        DefaultGroupName = defaultGroupName;
    }
}