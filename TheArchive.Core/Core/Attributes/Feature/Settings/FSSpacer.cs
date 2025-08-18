using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Inserts an empty space above this feature setting in the mod settings menu.
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
///         // Inserts an empty space above this setting.
///         [FSSpacer]
///         public float MyCustomFloat { get; set; } = 0.2f;
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSSpacer : Attribute
{
}