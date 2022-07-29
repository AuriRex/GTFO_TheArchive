using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class StringSetting : FeatureSetting
    {
        public int MaxInputLength { get; } = 50;

        public StringSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {
            MaxInputLength = prop.GetCustomAttribute<FSMaxLength>()?.MaxLength ?? 50;
        }
    }
}
