using System;

namespace TheArchive.Core.Attributes.Feature;

/// <summary>
/// This attribute specifies other features that are incompatible with this feature.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class FeatureIncompatiblity : Attribute
{
    /// <summary>
    /// Marks this <see cref="FeaturesAPI.Feature" /> as incompatible with another feature.
    /// If the other feature enabled, this feature will not be enabled and a warning will be shown.
    /// </summary>
    /// <param name="IncompatibilityGUID">The GUID of the referenced feature.</param>
    public FeatureIncompatiblity(string IncompatibilityGUID)
    {
        this.IncompatibilityGUID = IncompatibilityGUID;
    }

    /// <summary>
    /// The GUID of the referenced feature.
    /// </summary>
    public string IncompatibilityGUID { get; }
}
