using System;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class FeatureSetting
    {
        public FeatureSettingsHelper FeatureSettingsHelper { get; }
        public PropertyInfo Prop { get; }
        public string DisplayName { get; }
        public Type Type { get; }
        public string DEBUG_Path { get; }

        private readonly object _instance;

        public FeatureSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "")
        {
            FeatureSettingsHelper = featureSettingsHelper;
            Prop = prop;
            Type = prop?.GetMethod?.ReturnType;
            DisplayName = $"> {prop?.GetCustomAttribute<FSDisplayName>()?.DisplayName ?? prop.Name}";
            _instance = instance;
            DEBUG_Path = debug_path;
        }


        public virtual void SetValue(object value)
        {
            Prop.SetValue(_instance, value);
        }

        public virtual object GetValue()
        {
            return Prop.GetValue(_instance, Array.Empty<object>());
        }
    }
}
