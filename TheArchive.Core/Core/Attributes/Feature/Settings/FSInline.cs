using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Forces this property's types members to be merged into its parent types submenu in the settings menu instead of creating a new submenu.
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
///         // All the properties in the `MyCustomSubType` class are going to show up
///         // as if they were part of the parent type `MyCustomSettings` instead of
///         // the system generating a new submenu.
///         [FSInline]
///         public string MyCustomSubType { get; set; } = new();
///
///         public class MyCustomSubType
///         {
///             public string Thing1 { get; set; }
///             public bool Thing1 { get; set; }
///             public int Thing1 { get; set; }
///         }
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class FSInline : Attribute
{
}