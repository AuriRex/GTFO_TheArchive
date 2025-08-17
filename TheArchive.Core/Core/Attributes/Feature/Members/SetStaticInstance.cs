using System;

namespace TheArchive.Core.Attributes.Feature.Members;

/// <summary>
/// Sets the value of the decorated <c>TFeature</c> property to the instance of the <see cref="FeaturesAPI.Feature"/>.<br/>
/// Use the parents (Feature) type as this properties type.
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate a property on a <see cref="FeaturesAPI.Feature"/> type.</item>
/// <item>The property <b>must</b> be <c>static</c>.</item>
/// <item>Alternatively, name your property <c>Instance</c> or <c>Self</c> to avoid having to use this attribute.</item>
/// <item><b>Only the first valid property on your type will be used!</b></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class SetStaticInstance : Attribute
{
}