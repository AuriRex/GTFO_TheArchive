using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting that handles string input fields.
/// </summary>
public class StringSetting : FeatureSetting
{
    /// <summary>
    /// The maximum allowed input string length.
    /// </summary>
    public int MaxInputLength { get; }

    /// <inheritdoc/>
    public StringSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        MaxInputLength = prop.GetCustomAttribute<FSMaxLength>()?.MaxLength ?? 50;
    }
}