using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
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

        private static IArchiveLogger _logger = LoaderWrapper.CreateArSubLoggerInstance(nameof(FeatureSettingsHelper), ConsoleColor.DarkYellow);

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

            _logger.Debug($"Populate: {path}");

            foreach (var prop in typeToCheck.GetProperties())
            {
                var propPath = $"{path}.{prop.Name}";
                var type = prop?.GetMethod?.ReturnType;

                if (prop?.SetMethod == null) continue;
                if (prop?.GetCustomAttribute<FSIgnore>() != null) continue;

                if (!type.IsValueType && !typeof(IList).IsAssignableFrom(type) && type != typeof(string))
                {
                    PopulateThing(type, prop.GetValue(instance), propPath);
                    continue;
                }
                // Add to dict?
                FeatureSetting setting;
                switch(type.Name)
                {
                    case nameof(SColor):
                        setting = new ColorSetting(this, prop, instance, propPath);
                        break;
                    case nameof(Boolean):
                        setting = new BoolSetting(this, prop, instance, propPath);
                        break;
                    case nameof(String):
                        setting = new StringSetting(this, prop, instance, propPath);
                        break;
                    default:
                        if(typeof(IList).IsAssignableFrom(type) && type.GenericTypeArguments.Length >  0 && type.GenericTypeArguments[0].IsEnum)
                        {
                            setting = new EnumListSetting(this, prop, instance, propPath);
                            break;
                        }
                        if (type.IsEnum)
                        {
                            setting = new EnumSetting(this, prop, instance, propPath);
                            break;
                        }
                        setting = new FeatureSetting(this, prop, instance, propPath);
                        break;
                }

                Settings.Add(setting);

                _logger.Debug($"Setting Added: {propPath} | {setting.GetType().Name}");
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
