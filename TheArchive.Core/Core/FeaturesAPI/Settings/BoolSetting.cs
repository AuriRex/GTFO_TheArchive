using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings;

public class BoolSetting : FeatureSetting
{

    public BoolSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
    {

    }
}