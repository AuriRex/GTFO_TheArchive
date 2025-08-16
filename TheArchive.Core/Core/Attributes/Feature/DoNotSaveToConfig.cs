using System;

namespace TheArchive.Core.Attributes.Feature;

[AttributeUsage(AttributeTargets.Class)]
public class DoNotSaveToConfig : Attribute
{
}