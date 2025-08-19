using System;
using System.Reflection;
using UnityEngine;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting for handling custom keybinds.
/// </summary>
public class KeySetting : FeatureSetting
{
    /// <summary>
    /// The key button text
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Invoked whenever the key text gets updated.
    /// </summary>
    public Action<string> KeyTextUpdated;

    /// <inheritdoc/>
    public KeySetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        Key = prop.GetValue(instance)?.ToString();
    }

    /// <inheritdoc/>
    public override object SetValue(object value)
    {
        UpdateKeyText(value.ToString());
        return base.SetValue(value);
    }

    /// <summary>
    /// Gets the current KeyCode value that is set.
    /// </summary>
    /// <returns>The current KeyCode.</returns>
    public KeyCode GetCurrent()
    {
        return (KeyCode) GetValue();
    }

    /// <summary>
    /// Update the key button text.
    /// </summary>
    /// <param name="keyText">The new button text.</param>
    public void UpdateKeyText(string keyText)
    {
        Key = keyText;
        KeyTextUpdated?.Invoke(Key);
    }
}