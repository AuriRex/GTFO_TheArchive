using System;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class FeatureSetting
    {
        public FeatureSettingsHelper Helper { get; }
        public PropertyInfo Prop { get; }
        public string DisplayName { get; }
        public string Identifier { get; }
        public Type Type { get; }
        public string DEBUG_Path { get; }

        private readonly object _instance;

        public FeatureSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "")
        {
            Helper = featureSettingsHelper;
            Prop = prop;
            Type = prop?.GetMethod?.ReturnType;
            DisplayName = $"> {prop?.GetCustomAttribute<FSDisplayName>()?.DisplayName ?? prop.Name}";
            Identifier = prop?.GetCustomAttribute<FSIdentifier>()?.Identifier ?? ($"{prop.PropertyType.FullName}_{prop.Name}");
            _instance = instance;
            DEBUG_Path = debug_path;
        }

        public virtual void SetValue(object value)
        {
            Prop.SetValue(_instance, value);
            FeatureManager.Instance.OnFeatureSettingChanged(this);
        }

        public virtual object GetValue()
        {
            return Prop.GetValue(_instance, Array.Empty<object>());
        }
    }
}
