using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

[AttributeUsage(AttributeTargets.Property)]
public class FSReadOnly : Attribute
{
    public bool RecursiveReadOnly { get; private set; } = true;
    public FSReadOnly(bool recursive = true)
    {
        RecursiveReadOnly = recursive;
    }
}