using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class NotSavedFeatureSetting : FeatureSetting
    {
        public NotSavedFeatureSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {
        }

        public override object SetValue(object value)
        {
            return value;
        }
    }
}
