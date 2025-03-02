using System;

namespace TheArchive.Core.Attributes.Feature.Settings;

public class FSMaxLength : Attribute
{
    public int MaxLength { get; } = 50;

    public FSMaxLength(int maxLength)
    {
        MaxLength = maxLength;
    }
}