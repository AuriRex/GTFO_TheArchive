using System;
using System.Reflection;
using UnityEngine;

namespace TheArchive.Core.FeaturesAPI.Settings
{
    public class KeySetting : FeatureSetting
    {
        public string Key { get; set; }

        public Action<string> KeyTextUpdated;

        public KeySetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
        {
            Key = prop.GetValue(instance).ToString();
        }

        public override object SetValue(object value)
        {
            UpdateKeyText(value.ToString());
            return base.SetValue(value);
        }

        public KeyCode GetCurrent()
        {
            return (KeyCode) GetValue();
        }

        public void UpdateKeyText(string keyText)
        {
            Key = keyText;
            KeyTextUpdated?.Invoke(Key);
        }
    }
}
