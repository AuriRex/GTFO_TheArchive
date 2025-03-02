using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TheArchive.Core.FeaturesAPI.Settings;

public class EnumSetting : FeatureSetting
{
    public string[] Options { get; }
    public Dictionary<string, object> Map { get; private set; } = new Dictionary<string, object>();
    public EnumSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
    {
        Options = Enum.GetNames(Type);

        foreach (var option in Options)
        {
            if (featureSettingsHelper.Localization.TryGetFSEnumText(Type, out var dic) && dic.TryGetValue(option, out var text))
                Map.Add(text, Enum.Parse(Type, option));
            else
                Map.Add(option, Enum.Parse(Type, option));
        }
    }

    public object GetEnumValueFor(string option)
    {
        if(Map.TryGetValue(option, out var val))
        {
            return val;
        }

        return null;
    }

    public string GetCurrentEnumKey()
    {
        var value = this.GetValue();
        return Map.FirstOrDefault(kvp => (int)kvp.Value == (int)value).Key;
    }
}