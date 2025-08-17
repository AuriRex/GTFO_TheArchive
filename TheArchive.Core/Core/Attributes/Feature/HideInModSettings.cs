using System;

namespace TheArchive.Core.Attributes.Feature;

/// <summary>
/// Hides this <see cref="FeaturesAPI.Feature"/> in the Mod Settings menu unless developer mode is turned on.
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate your <c>Feature</c> types.</item>
/// </list>
/// </remarks>
/// <seealso cref="FeaturesAPI.Feature"/>
/// <example><code>
/// [HideInModSettings]
/// public class MyFeature : Feature
/// {
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Class)]
public class HideInModSettings : Attribute
{
}