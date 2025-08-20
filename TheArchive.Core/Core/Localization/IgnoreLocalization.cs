using System;

namespace TheArchive.Core.Localization;

/// <summary>
/// Do not localize this feature setting.
/// </summary>
/// <remarks>
/// Also works on the features <c>Name</c> and <c>Description</c> property.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreLocalization : Attribute
{
}