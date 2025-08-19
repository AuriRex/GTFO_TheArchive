using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting that should not be saved to config.
/// </summary>
public class NotSavedFeatureSetting : FeatureSetting
{
    /// <inheritdoc/>
    public NotSavedFeatureSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
    }

    /// <summary>
    /// Does not actually save the value. (<see cref="NotSavedFeatureSetting"/>)
    /// </summary>
    /// <param name="value">A value</param>
    /// <returns>The passed value</returns>
    public override object SetValue(object value)
    {
        return value;
    }
}