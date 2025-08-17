using System;

namespace TheArchive.Core.Attributes.Feature.Members;

/// <summary>
/// Identifies a method defined in a <see cref="FeaturesAPI.Feature"/> type to be the <see cref="FeaturesAPI.Feature.OnAreaCull"/> method.
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate a method on a <see cref="FeaturesAPI.Feature"/> type.</item>
/// <item>The method <b>must NOT</b> be <c>static</c> and have two parameters, first of which has to be of either type <c>object</c> or <c>LG_Area</c> and the second parameter of type <c>bool</c>.</item>
/// <item>Alternatively, name your method <c>OnAreaCull</c> to avoid having to use this attribute.</item>
/// <item><b>Only the first valid property on your type will be used!</b><br/>
/// Validity in this case includes matching <see cref="RundownConstraint"/> and <see cref="BuildConstraint"/></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
[Obsolete("Feature not implemented.")]
public class IsAreaCullUpdateMethod : Attribute
{
}