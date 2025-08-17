using System;
using TheArchive.Interfaces;

namespace TheArchive.Core.Attributes.Feature.Members;

/// <summary>
/// Sets the value of the decorated <see cref="IArchiveLogger"/> property to the instance of this <see cref="FeaturesAPI.Feature"/>s FeatureLogger.
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate a property on a <see cref="FeaturesAPI.Feature"/> type.</item>
/// <item>The property <b>must</b> be <c>static</c>.</item>
/// <item>Alternatively, name your property <c>FeatureLogger</c> to avoid having to use this attribute.<br/>
/// (Use the <c>new</c> modifier to hide the instance member with the same name)
/// </item>
/// <item><b>Only the first valid property on your type will be used!</b></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class SetStaticLogger : Attribute
{
}