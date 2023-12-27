using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Core.FeaturesAPI
{
    public partial class FeatureSettingsHelper
    {
        internal PropertyInfo Property { get; private set; }
        public string DisplayName { get; private set; }
        internal string TypeName => SettingType?.Name;
        internal string PropertyName => Property?.Name;
        internal Type SettingType { get; private set; }
        internal object Instance { get; set; }
        public Feature Feature { get; protected set; }
        internal FeatureSettingsHelper ParentHelper { get; set; }

        private bool _isDirty = false;
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            internal set
            {
                if(ParentHelper != null)
                {
                    ParentHelper.IsDirty = value;
                }
                _isDirty = value;
            }
        }

        private static readonly IArchiveLogger _logger = LoaderWrapper.CreateArSubLoggerInstance(nameof(FeatureSettingsHelper), ConsoleColor.DarkYellow);

        internal static bool ForceEnableDebugLogging { get; set; } = false;

        public HashSet<FeatureSetting> Settings { get; private set; } = new HashSet<FeatureSetting>();

        internal FeatureSettingsHelper(Feature feature, PropertyInfo settingsProperty)
        {
            Feature = feature;
            Property = settingsProperty;
            SettingType = settingsProperty?.GetMethod?.ReturnType ?? throw new ArgumentNullException(nameof(settingsProperty), $"Settings Property must implement a get method!");
            if (settingsProperty?.GetCustomAttribute<FSDisplayName>(true) != null)
            {
                string propID = $"{settingsProperty.DeclaringType.FullName}.{settingsProperty.Name}";
                if (feature.FeatureInternal.Localization.TryGetFSText(propID, FSType.FSDisplayName, out var text))
                {
                    DisplayName = text;
                }
            }
            else
            {
                DisplayName = settingsProperty?.GetCustomAttribute<FSDisplayName>()?.DisplayName;
            }
        }

        protected FeatureSettingsHelper() { }

        protected void PopulateSettings(Type typeToCheck, object instance, string path = "")
        {
            if (typeToCheck.IsValueType) return;

            if (string.IsNullOrWhiteSpace(path))
            {
                path = typeToCheck.FullName;
            }

            DebugLog($"Populate: {path}");

            foreach (var prop in typeToCheck.GetProperties())
            {
                var propPath = $"{path}.{prop.Name}";
                var type = prop?.GetMethod?.ReturnType;

                if (type == null || prop == null) continue;

                if (prop.GetCustomAttribute<FSIgnore>() != null) continue;

                if (prop.SetMethod == null)
                {
                    continue;
                }

                bool shouldInline = prop.GetCustomAttribute<FSInline>() != null;

                if (!type.IsValueType
                    && !typeof(IList).IsAssignableFrom(type)
                    && !typeof(IDictionary).IsAssignableFrom(type)
                    && type != typeof(string)
                    && type.GenericTypeArguments.Length <= 1
                    && shouldInline)
                {
                    PopulateSettings(type, prop.GetValue(instance), propPath);
                    continue;
                }
                // Add to dict?
                FeatureSetting setting;
                switch(type.Name)
                {
                    case nameof(FButton):
                        setting = new ButtonSetting(this, prop, instance, propPath);
                        break;
                    case nameof(FLabel):
                        setting = new LabelSetting(this, prop, instance, propPath);
                        break;
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
                        if (type == typeof(KeyCode))
                        {
                            setting = new KeySetting(this, prop, instance, propPath);
                            break;
                        }
                        if (typeof(IList).IsAssignableFrom(type) && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments[0].IsEnum)
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
                        if(!shouldInline)
                        {
                            setting = new SubmenuSetting(this, prop, instance /* = Host */, propPath);
                            break;
                        }
                        setting = new FeatureSetting(this, prop, instance, propPath);
                        break;
                }

                Settings.Add(setting);

                DebugLog($"Setting Added: {propPath} | {setting.GetType().Name}");
            }
        }

        protected void DebugLog(string msg)
        {
            if(ForceEnableDebugLogging || !Feature.GameDataInited)
                _logger.Debug(msg);
        }

        internal virtual void SetupViaFeatureInstance(object configInstance) => SetupViaInstanceOnHost(Feature, configInstance);

        internal virtual void SetupViaInstanceOnHost(object host, object configInstance)
        {
            Instance = configInstance;
            Property.SetValue(host, configInstance);
            Settings.Clear();
            PopulateSettings(SettingType, Instance, string.Empty);
        }

        public virtual object GetFeatureInstance() => GetInstanceOnHost(Feature);

        public virtual object GetInstanceOnHost(object host)
        {
            return Instance ?? Property.GetValue(host);
        }
    }
}
