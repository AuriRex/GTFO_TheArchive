using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting for managing booleans.
/// </summary>
public class BoolSetting : FeatureSetting
{
    /// <inheritdoc/>
    public BoolSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {

    }
}