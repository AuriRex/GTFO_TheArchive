using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Utilities;

namespace TheArchive.Core
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

            PopulateThing(SettingType);
        }

        private void PopulateThing(Type typeToCheck, string path = "")
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
                    PopulateThing(type, propPath);
                    continue;
                }
                // Add to dict?
                Settings.Add(new FeatureSetting(this, prop));
                ArchiveLogger.Debug($"[{nameof(FeatureSettingsHelper)}] Setting Added: {path}");
            }
        }

        internal void SetInstance(object configInstance)
        {
            Instance = configInstance;
            Property.SetValue(_feature, configInstance);
        }

        public object GetInstance()
        {
            return Instance ?? Property.GetValue(_feature);
        }
    }
}
