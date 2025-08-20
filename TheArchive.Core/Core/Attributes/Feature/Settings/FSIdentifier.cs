using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Sets the <c>Identifier</c> string of a FeatureSetting.<br/>
/// Useful for knowing which setting is which in <see cref="FeaturesAPI.Feature.OnFeatureSettingChanged"/>.
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
///         [FSIdentifier("my_custom_setting")]
///         public string MyCustomString { get; set; }
///     }
///
///     public override void OnFeatureSettingChanged(FeatureSetting setting)
///     {
///         if (setting.Identifier == "my_custom_setting")
///         {
///             // do things
///         }
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSIdentifier : Attribute
{
    internal string Identifier { get; }
    
    /// <summary>
    /// Identify a FeatureSetting.
    /// </summary>
    /// <param name="identifier">The string identifier.</param>
    public FSIdentifier(string identifier)
    {
        Identifier = identifier;
    }
}