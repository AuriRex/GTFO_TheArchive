using System;

namespace TheArchive.Core.Attributes.Feature;

/// <summary>
/// Enables this <see cref="FeaturesAPI.Feature"/> in the config by default (on first launch with this Feature loaded).<br/>
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate your <c>Feature</c> types.</item>
/// </list>
/// </remarks>
/// <seealso cref="FeaturesAPI.Feature"/>
/// <example><code>
/// [EnableFeatureByDefault]
/// public class MyFeature : Feature
/// {
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Class)]
public class EnableFeatureByDefault : Attribute
{
}