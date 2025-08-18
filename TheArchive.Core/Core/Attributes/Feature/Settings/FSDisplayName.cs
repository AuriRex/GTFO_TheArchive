using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Set the display name of a feature setting.
/// </summary>
/// <remarks>
/// <list>
/// <item>Use on a member of a type that's used by the feature settings system. (<c>[FeatureConfig]</c>)</item>
/// <item>Accepts TextMeshPro rich text tags.</item>
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
///         [FSDisplayName("My &lt;b&gt;Custom&lt;/b&gt; String")]
///         public string MyCustomString { get; set; } = "Something";
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSDisplayName : Localized
{
    /// <summary>
    /// The feature settings display name.
    /// </summary>
    public string DisplayName => UntranslatedText;
    
    /// <summary>
    /// Set the display name of a feature setting.
    /// </summary>
    /// <param name="displayName">The display name of the feature setting.</param>
    public FSDisplayName(string displayName) : base(displayName)
    {
    }
}