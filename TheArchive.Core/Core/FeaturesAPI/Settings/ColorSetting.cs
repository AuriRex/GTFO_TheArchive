using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting for managing colors.
/// </summary>
public class ColorSetting : FeatureSetting
{
    /// <inheritdoc/>
    public ColorSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
    }
}