using System;

namespace TheArchive.Core.Attributes.Feature;

[AttributeUsage(AttributeTargets.Property)]
public class RequiresRestart : Attribute
{
}