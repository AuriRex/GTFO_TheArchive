using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Disallows the user from altering the value of a setting.
/// </summary>
/// <remarks>
/// <list>
/// <item>Use on a member of a type that's used by the feature settings system. (<c>[FeatureConfig]</c>)</item>
/// </list>
/// </remarks>
/// <example><code>
/// public class MyFeature : Feature
/// {
///     [FeatureConfig]
///     public static MyCustomSettings Settings { get; set; }
///
///     public class MyCustomSettings
///     {
///         // Setting can't be altered via the mod settings menu.
///         [FSReadOnly]
///         public string MyCustomString { get; set; } = "Something";
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSReadOnly : Attribute
{
    /// <summary>
    /// Should this recursively apply to child settings as well?
    /// </summary>
    public bool RecursiveReadOnly { get; }
    
    /// <summary>
    /// Disallows the user from altering the value of a setting.
    /// </summary>
    /// <param name="recursive">Apply to child settings as well?</param>
    public FSReadOnly(bool recursive = true)
    {
        RecursiveReadOnly = recursive;
    }
}