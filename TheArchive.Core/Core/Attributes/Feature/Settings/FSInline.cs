using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

[AttributeUsage(AttributeTargets.Property)]
/// <summary>
/// Forces a Properties Type to be merged into it's parent Types submenu in the settings menu instead of creating a new one.
/// </summary>
public class FSInline : Attribute
{

}