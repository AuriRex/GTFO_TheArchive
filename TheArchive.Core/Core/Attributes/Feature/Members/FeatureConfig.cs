using System;

namespace TheArchive.Core.Attributes.Feature.Members;

/// <summary>
/// Identifies a property defined in a <see cref="FeaturesAPI.Feature"/> type to hold config data determined by the properties type.<br/>
/// This properties' data will be serialized and deserialized automatically.
/// </summary>
/// <remarks>
/// <list>
/// <item>There may be multiple different properties decorated with the <c>FeatureConfig</c> Attribute in one <c>Feature</c>.</item>
/// <item>If possible, keep your custom settings class nested inside of your <c>Feature</c> type.<br/>
/// (For features with a lot of code, consider splitting it up using the <c>partial</c> modifier.)
/// </item>
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
///         public string MyCustomString { get; set; } = "Default Value";
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FeatureConfig : Attribute
{
}