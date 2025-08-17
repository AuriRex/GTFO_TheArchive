using System;
using TheArchive.Core.Localization;

namespace TheArchive.Core.Attributes.Feature.Members;

/// <summary>
/// Sets the value of the decorated <see cref="ILocalizationService"/> property to the instance of this <see cref="FeaturesAPI.Feature"/>s LocalizationService.
/// </summary>
/// <remarks>
/// <list>
/// <item>Used to decorate a property on a <see cref="FeaturesAPI.Feature"/> type.</item>
/// <item>The property <b>must</b> be <c>static</c>.</item>
/// <item>Alternatively, name your property <c>Localization</c> to avoid having to use this attribute.</item>
/// <item><b>Only the first valid property on your type will be used!</b></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class SetStaticLocalizationService : Attribute
{
}