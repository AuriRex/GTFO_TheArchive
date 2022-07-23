using System;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Utilities;

namespace TheArchive.Core.FeaturesAPI
{
    public class FeatureSettingsHelper
    {
        internal PropertyInfo Property { get; private set; }
        public string DisplayName { get; private set; }
        internal string TypeName => SettingType?.Name;
        internal string PropertyName => Property?.Name;
        internal Type SettingType { get; private set; }
        internal object Instance { get; set; }
        private readonly Feature _feature;

        public HashSet<FeatureSetting> Settings { get; private set; } = new HashSet<FeatureSetting>();

        internal FeatureSettingsHelper(Feature feature, PropertyInfo settingsProperty)
        {
            _feature = feature;
            Property = settingsProperty;
            SettingType = settingsProperty?.GetMethod?.ReturnType ?? throw new ArgumentNullException($"Settings Property must implement a get method!");
            DisplayName = settingsProperty.GetCustomAttribute<FSDisplayName>()?.DisplayName;
        }

        private void PopulateThing(Type typeToCheck, object instance, string path = "")
        {
#warning TODO
            if (typeToCheck.IsValueType) return;

            if (string.IsNullOrWhiteSpace(path))
            {
                path = typeToCheck.FullName;
            }

            ArchiveLogger.Debug($"[{nameof(FeatureSettingsHelper)}] Populate: {path}");

            foreach (var prop in typeToCheck.GetProperties())
            {
                var propPath = $"{path}.{prop.Name}";
                var type = prop?.GetMethod?.ReturnType;

                if (prop?.SetMethod == null) continue;

                if (!type.IsValueType)
                {
                    PopulateThing(type, prop.GetValue(instance), propPath);
                    continue;
                }
                // Add to dict?
                Settings.Add(new FeatureSetting(this, prop, instance, propPath));
                ArchiveLogger.Debug($"[{nameof(FeatureSettingsHelper)}] Setting Added: {propPath}");
            }
        }

        internal void SetInstance(object configInstance)
        {
            Instance = configInstance;
            Property.SetValue(_feature, configInstance);
            Settings.Clear();
            PopulateThing(SettingType, Instance, string.Empty);
        }

        public object GetInstance()
        {
            return Instance ?? Property.GetValue(_feature);
        }
    }
}
