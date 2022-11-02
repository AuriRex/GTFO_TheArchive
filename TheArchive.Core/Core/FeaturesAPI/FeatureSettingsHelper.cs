using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Loader;

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
        public Feature Feature { get; protected set; }

        private static readonly IArchiveLogger _logger = LoaderWrapper.CreateArSubLoggerInstance(nameof(FeatureSettingsHelper), ConsoleColor.DarkYellow);

        public HashSet<FeatureSetting> Settings { get; private set; } = new HashSet<FeatureSetting>();

        internal FeatureSettingsHelper(Feature feature, PropertyInfo settingsProperty)
        {
            Feature = feature;
            Property = settingsProperty;
            SettingType = settingsProperty?.GetMethod?.ReturnType ?? throw new ArgumentNullException($"Settings Property must implement a get method!");
            DisplayName = settingsProperty.GetCustomAttribute<FSDisplayName>()?.DisplayName;
        }

        protected FeatureSettingsHelper() { }

        protected void PopulateSettings(Type typeToCheck, object instance, string path = "")
        {
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

                if (!type.IsValueType
                    && !typeof(IList).IsAssignableFrom(type)
                    && !typeof(IDictionary).IsAssignableFrom(type)
                    && type != typeof(string)
                    && type.GenericTypeArguments.Length <= 1)
                {
                    PopulateSettings(type, prop.GetValue(instance), propPath);
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
                    case nameof(UInt64):
                    case nameof(UInt32):
                    case nameof(UInt16):
                    case nameof(Byte):
                    case nameof(Int64):
                    case nameof(Int32):
                    case nameof(Int16):
                    case nameof(SByte):
                    case nameof(Single):
                    case nameof(Double):
                        setting = new NumberSetting(this, prop, instance, propPath);
                        break;
                    default:
                        if(typeof(IList).IsAssignableFrom(type) && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments[0].IsEnum)
                        {
                            setting = new EnumListSetting(this, prop, instance, propPath);
                            break;
                        }
                        if (typeof(IList).IsAssignableFrom(type) && type.GenericTypeArguments.Length == 1)
                        {
                            setting = new GenericListSetting(this, prop, instance, propPath);
                            break;
                        }
                        if (typeof(IDictionary).IsAssignableFrom(type) && type.GenericTypeArguments.Length == 2)
                        {
                            setting = new GenericDictionarySetting(this, prop, instance, propPath);
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

        internal virtual void SetupViaInstance(object configInstance)
        {
            Instance = configInstance;
            Property.SetValue(Feature, configInstance);
            Settings.Clear();
            PopulateSettings(SettingType, Instance, string.Empty);
        }

        public virtual object GetInstance()
        {
            return Instance ?? Property.GetValue(Feature);
        }
    }

    public class DynamicFeatureSettingsHelper : FeatureSettingsHelper
    {
        public DynamicFeatureSettingsHelper(Feature feature)
        {
            Feature = feature;
        }

        /// <summary>
        /// Initialize the <see cref="FeatureSettingsHelper.Settings"/> list and returns itself.
        /// </summary>
        /// <param name="typeToCheck"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public DynamicFeatureSettingsHelper Initialize(Type typeToCheck, object instance)
        {
            PopulateSettings(typeToCheck, instance, string.Empty);
            return this;
        }

        public void SetInstanceForAllSettings(object instance)
        {
            foreach(var setting in Settings)
            {
                setting.WrappedInstance = instance;
            }
        }

        internal override void SetupViaInstance(object objectInstance)
        {
            throw new NotImplementedException();
        }

        public override object GetInstance()
        {
            throw new NotImplementedException();
        }
    }
}
