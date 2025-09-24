using System;

namespace TheArchive.Core.Attributes;

/// <summary>
/// Specifies a custom display name for the assembly.
/// </summary>
/// <remarks>
/// If this attribute is not used, the assembly's actual name will be used as the display name.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public class ModSettingsDisplayName : Attribute
{
    /// <summary>
    /// The custom display name for the assembly.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Creates a new instance of the ArchiveAssemblyNameOverride attribute.
    /// </summary>
    /// <param name="displayName">The custom display name to use for the assembly.</param>
    public ModSettingsDisplayName(string displayName)
    {
        DisplayName = displayName;
    }
}