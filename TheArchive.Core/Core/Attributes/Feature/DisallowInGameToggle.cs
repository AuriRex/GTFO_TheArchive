using System;

namespace TheArchive.Core.Attributes.Feature;

/// <summary>
/// Prevents users from toggling this <see cref="FeaturesAPI.Feature"/>s enabled state via the Mod Settings menu.<br/>
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate your <c>Feature</c> types.</item>
/// </list>
/// </remarks>
/// <seealso cref="FeaturesAPI.Feature"/>
/// <example><code>
/// [DisallowInGameToggle]
/// public class MyFeature : Feature
/// {
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Class)]
public class DisallowInGameToggle : Attribute
{
}