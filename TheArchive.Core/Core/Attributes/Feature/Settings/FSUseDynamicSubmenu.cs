using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

/// <summary>
/// Mark a submenu as being dynamic.<br/>
/// The submenu is going to update each time it gets opened.
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
///         // The submenu will be re-created each time it is entered instead of only once.
///         // Without this being a dynamic submenu you would have to manually update
///         // the `Counter`s value through code.
///         [FSUseDynamicSubmenu]
///         public MyDynamicSubmenu DynamicSubmenuExample { get; set; } = new();
///
///         public class MyDynamicSubmenu
///         {
///             public int Counter { get; set; }
///         }
///     }
///
///     private override void Update()
///     {
///         // The Counter value is going to increase by 1 every frame and every time
///         // the submenu is opened you get a snapshot of the value at that moment.
///         // You probably don't want to do this though,
///         // this is just an example to show how it behaves.
///         Settings.DynamicSubmenuExample.Counter += 1;
///     }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class FSUseDynamicSubmenu : Attribute
{
}