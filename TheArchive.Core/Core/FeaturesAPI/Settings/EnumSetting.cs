using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting for managing a selection of a singular enum value.
/// </summary>
public class EnumSetting : FeatureSetting
{
    /// <summary>
    /// All the enum names.
    /// </summary>
    public string[] Options { get; }
    
    /// <summary>
    /// Maps (localized) name to the enum type.
    /// </summary>
    public Dictionary<string, object> Map { get; } = new();
    
    /// <inheritdoc/>
    public EnumSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        Options = Enum.GetNames(Type);

        foreach (var option in Options)
        {
            if (featureSettingsHelper.Localization.TryGetFSEnumText(Type, out var dic)
                && dic.TryGetValue(option, out var text))
            {
                Map.Add(text, Enum.Parse(Type, option));
                continue;
            }
            
            Map.Add(option, Enum.Parse(Type, option));
        }
    }

    /// <summary>
    /// Get the enum value from the (localized) name.
    /// </summary>
    /// <param name="option">The (localized) enum name.</param>
    /// <returns>The enum value.</returns>
    public object GetEnumValueFor(string option)
    {
        return Map.GetValueOrDefault(option);
    }

    /// <summary>
    /// Get the currently selected enums (localized) name.
    /// </summary>
    /// <returns></returns>
    public string GetCurrentEnumKey()
    {
        var value = GetValue();
        return Map.FirstOrDefault(kvp => (int)kvp.Value == (int)value).Key;
    }
}