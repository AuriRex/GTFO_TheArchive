using System;

namespace TheArchive.Core.Attributes.Feature;

/// <summary>
/// Marks the <see cref="FeaturesAPI.Feature"/> as requiring a restart for the enabled state to toggle.
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate your <c>Feature</c> types.</item>
/// </list>
/// </remarks>
/// <seealso cref="FeaturesAPI.Feature"/>
/// <example><code>
/// [RequiresRestart]
/// public class MyFeature : Feature
/// {
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Property)]
public class RequiresRestart : Attribute
{
}