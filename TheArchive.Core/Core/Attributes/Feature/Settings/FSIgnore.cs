using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Forces this property to be ignored by the feature settings system, resulting in it not getting an entry in the mod settings menu.
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
///         // This property won't show up in mod settings at all
///         [FSIgnore]
///         public string MyCustomString { get; set; } = "Something";
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSIgnore : Attribute
{
}