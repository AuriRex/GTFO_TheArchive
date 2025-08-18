using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Set the description of a feature setting.
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
///         [FSDescription("This is the text that shows up in the panel on the right side on hover.")]
///         public string MyCustomString { get; set; } = "Something";
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSDescription : Localized
{
    /// <summary>
    /// The feature settings description.
    /// </summary>
    public string Description => UntranslatedText;
    
    /// <summary>
    /// Set the description of a feature setting.
    /// </summary>
    /// <param name="description">The description of the feature setting.</param>
    public FSDescription(string description) : base(description)
    {
    }
}