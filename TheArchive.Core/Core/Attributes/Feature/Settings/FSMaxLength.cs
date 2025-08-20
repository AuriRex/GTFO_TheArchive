using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Restricts the user from entering any more than <c>maxLength</c> characters into a string input field in the mod settings menu.
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
///         // User can enter a maximum of 25 characters in the mod settings menu
///         [FSMaxLength(25)]
///         public string MyCustomString { get; set; } = "Something";
///     }
/// }
/// </code></example>
public class FSMaxLength : Attribute
{
    /// <summary>
    /// Maximum length that's allowed.
    /// </summary>
    public int MaxLength { get; }

    /// <summary>
    /// Restricts the user from entering any more than <c>maxLength</c> characters into a string input field in the mod settings menu.
    /// </summary>
    /// <param name="maxLength">Maximum length that's allowed.</param>
    public FSMaxLength(int maxLength = 50)
    {
        MaxLength = maxLength;
    }
}