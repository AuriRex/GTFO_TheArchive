using System;

namespace TheArchive.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ForceDisable : Attribute
{
    public string Justification { get; set; }
    
    public ForceDisable(string justification)
    {
        Justification = justification;
    }
}