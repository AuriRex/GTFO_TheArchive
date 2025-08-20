using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting for managing a selection of different enum values.
/// </summary>
public class EnumListSetting : FeatureSetting
{
    /// <summary>
    /// All the enum names.
    /// </summary>
    public string[] Options { get; }
    
    /// <summary>
    /// Maps (localized) name to the enum type.
    /// </summary>
    public Dictionary<string, object> Map { get; } = new();
    
    /// <summary>
    /// Maps enum type to the (localized) name.
    /// </summary>
    public Dictionary<object, string> ReversedMap { get; } = new();
    
    /// <summary>
    /// The enum type.
    /// </summary>
    public Type EnumType { get; }
    
    /// <inheritdoc/>
    public EnumListSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        EnumType = Type.GenericTypeArguments[0];
        Options = Enum.GetNames(EnumType);

        foreach (var option in Options)
        {
            var enumValue = Enum.Parse(EnumType, option);
            if (featureSettingsHelper.Localization.TryGetFSEnumText(EnumType, out var dic) && dic.TryGetValue(option, out var text))
            {
                Map.Add(text, enumValue);
                ReversedMap.Add(enumValue, text);
                continue;
            }

            Map.Add(option, enumValue);
            ReversedMap.Add(enumValue, option);
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
    /// Get the list of all selected enum values.
    /// </summary>
    /// <returns>List of selected enum values.</returns>
    public IList GetList()
    {
        return GetValue() as IList;
    }

    /// <summary>
    /// Toggle an enum value in the list.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>If the value is now present in the list</returns>
    public bool ToggleInList(object value)
    {
        if (RemoveFromList(value))
            return false;
        AddToList(value);
        return true;
    }

    /// <summary>
    /// Add an enum value to the list.
    /// </summary>
    /// <param name="value">The value to add.</param>
    public void AddToList(object value)
    {
        GetList().Add(value);
        FeatureManager.Instance.OnFeatureSettingChanged(this);
        Helper.IsDirty = true;
    }

    /// <summary>
    /// Remove an enum value from the list.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns><c>True</c> if the value has been removed.</returns>
    public bool RemoveFromList(object value)
    {
        var list = GetList();
        if (!list.Contains(value))
            return false;
        list.Remove(value);
        FeatureManager.Instance.OnFeatureSettingChanged(this);
        Helper.IsDirty = true;
        return true;
    }

    /// <summary>
    /// Get an array containing all currently selected enum values.
    /// </summary>
    /// <returns>An array containing all currently selected enum values.</returns>
    public object[] CurrentSelectedValues()
    {
        var list = GetList();
        var array = new object[list.Count];
        list.CopyTo(array, 0);
        return array;
    }

    /// <summary>
    /// Get an array containing all the currently selected (localized) enum names.
    /// </summary>
    /// <returns>An array containing all the currently selected (localized) enum names.</returns>
    public string[] CurrentSelectedValuesName()
    {
        var list = GetList();
        var resultList = new List<string>(list.Count);
        foreach (var item in list)
        {
            resultList.Add(ReversedMap[item]);
        }
        return resultList.ToArray();
    }
}