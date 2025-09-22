using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using TheArchive.Core.Localization.Services;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Loader;
using UnityEngine;

namespace TheArchive.Core.FeaturesAPI;

/// <summary>
/// Handles the creation of FeatureSettings objects.<br/>
/// There is usually no real reason to interact with this directly.
/// </summary>
public class FeatureSettingsHelper
{
    private PropertyInfo Property { get; }
    /// <summary>
    /// Display Name
    /// </summary>
    public string DisplayName { get; private set; }
    internal string TypeName => SettingType?.Name;
    internal string PropertyName => Property?.Name;
    internal Type SettingType { get; }
    internal object Instance { get; private set; }
    /// <summary>
    /// The parent Feature of this settings helper
    /// </summary>
    public Feature Feature { get; protected init; }
    internal FeatureSettingsHelper ParentHelper { get; init; }

    private bool _isDirty;
    /// <summary>
    /// If this settings helper has any unsaved changes
    /// </summary>
    public bool IsDirty
    {
        get
        {
            if(ParentHelper != null)
            {
                return ParentHelper.IsDirty;
            }
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

    /// <summary>
    /// Set of all the loaded FeatureSettings
    /// </summary>
    public HashSet<FeatureSetting> Settings { get; } = new();

    internal FeatureSettingsHelper(Feature feature, PropertyInfo settingsProperty)
    {
        Feature = feature;
        Property = settingsProperty;
        SettingType = settingsProperty?.GetMethod?.ReturnType ?? throw new ArgumentNullException(nameof(settingsProperty), $"Settings Property must implement a get method!");
        SetDisplayName(settingsProperty);
    }

    /// <summary>
    /// Empty constructor
    /// </summary>
    protected FeatureSettingsHelper() { }

    /// <summary>
    /// Checks the members of the provided type for compatible types and initializes all found <c>FeatureSetting</c>s.
    /// </summary>
    /// <param name="typeToCheck">The type to check.</param>
    /// <param name="instance">The actual instance of the settings property.</param>
    /// <param name="path">A debug path.</param>
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
            var type = prop.GetMethod?.ReturnType;

            if (type == null) continue;

            if (prop.GetCustomAttribute<FSIgnore>() != null) continue;

            if (prop.SetMethod == null)
            {
                continue;
            }

            var shouldInline = prop.GetCustomAttribute<FSInline>() != null;

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

    /// <summary>
    /// Debug log
    /// </summary>
    /// <param name="msg">Message to log.</param>
    protected static void DebugLog(string msg)
    {
        if(ForceEnableDebugLogging || !Feature.GameDataInited)
            _logger.Debug(msg);
    }

    private void SetDisplayName(PropertyInfo settingsProperty)
    {
        var fsDisplayName = settingsProperty.GetCustomAttribute<FSDisplayName>(true);
        
        DisplayName = fsDisplayName?.DisplayName ?? settingsProperty.Name;
        
        if (fsDisplayName == null)
            return;
        
        var propID = $"{settingsProperty.DeclaringType!.FullName}.{settingsProperty.Name}";
        if (Localization.TryGetFSText(propID, FSType.FSDisplayName, out var text))
        {
            DisplayName = text;
        }
    }

    internal void RefreshDisplayName()
    {
        SetDisplayName(Property);
    }

    internal virtual void SetupViaFeatureInstance(object configInstance) => SetupViaInstanceOnHost(Feature, configInstance);

    internal virtual void SetupViaInstanceOnHost(object host, object configInstance)
    {
        if (configInstance == null)
        {
            _logger.Warning($"Config instance ({SettingType.FullName}) is null! This should not happen! Resetting values ... :(");
            configInstance = Activator.CreateInstance(SettingType);
        }
        
        Instance = configInstance;
        Property.SetValue(host, configInstance);
        Settings.Clear();
        PopulateSettings(SettingType, Instance, string.Empty);
    }

    /// <summary>
    /// Get the settings instance from this Feature.
    /// </summary>
    /// <returns>The settings properties instance.</returns>
    public virtual object GetFeatureInstance() => GetInstanceOnHost(Feature);

    /// <summary>
    /// Get the settings instance from an arbitrary host.
    /// </summary>
    /// <param name="host">The object whose type contains the settings property.</param>
    /// <returns>The settings properties instance.</returns>
    public virtual object GetInstanceOnHost(object host)
    {
        return Instance ?? Property.GetValue(host);
    }

    internal FeatureLocalizationService Localization => Feature.FeatureInternal.Localization;
}