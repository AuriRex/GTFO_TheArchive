using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class ColorSetting : FeatureSetting
    {
        public ColorSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {
        }
    }
}
