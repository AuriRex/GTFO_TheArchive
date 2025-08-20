using System;

namespace TheArchive.Core.Attributes.Feature;

/// <summary>
/// This <see cref="FeaturesAPI.Feature"/>s enabled state will not be saved to config.<br/>
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate your <c>Feature</c> types.</item>
/// </list>
/// </remarks>
/// <seealso cref="FeaturesAPI.Feature"/>
/// <example><code>
/// [DoNotSaveToConfig]
/// public class MyFeature : Feature
/// {
///     ...
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Class)]
public class DoNotSaveToConfig : Attribute
{
}