using System;
using System.Reflection;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.Localization;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature settings instance that manages a single property in your custom settings types.
/// </summary>
public class FeatureSetting
{
    /// <summary>
    /// The settings helper that's managing this setting.
    /// </summary>
    public FeatureSettingsHelper Helper { get; }
    /// <summary>
    /// The property that reflects this settings value.
    /// </summary>
    public PropertyInfo Prop { get; }
    /// <summary>
    /// A rundown hint.
    /// </summary>
    public RundownFlags RundownHint { get; }
    /// <summary>
    /// The display name of this feature setting.
    /// </summary>
    public string DisplayName { get; }
    /// <summary>
    /// The description of this feature setting.
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// The identifier string of this feature
    /// </summary>
    /// <remarks>
    /// <list>
    /// <item>Uses the properties <c>FSIdentifier</c> attribute, else the default format is <c>$"{PropertyType.FullName}_{prop.Name}"</c></item>
    /// </list>
    /// </remarks>
    public string Identifier { get; }
    /// <summary>
    /// Is this setting read only?<br/>
    /// Applies to child settings as well.
    /// </summary>
    public bool Readonly { get; }
    /// <summary>
    /// Is this setting read only?
    /// </summary>
    public bool TopLevelReadonly { get; }
    /// <summary>
    /// The reflected type.
    /// </summary>
    public Type Type { get; }
    /// <summary>
    /// Debug path
    /// </summary>
    public string DEBUG_Path { get; }
    /// <summary>
    /// The games CM_SettingsItem instance of this setting.
    /// </summary>
    public object CM_SettingsItem { get; internal set; }

    /// <summary>
    /// Insert a separator above this setting?
    /// </summary>
    public bool SeparatorAbove { get; private set; }
    /// <summary>
    /// Insert a spacer above this setting?
    /// </summary>
    public bool SpacerAbove { get; private set; }
    /// <summary>
    /// The header to insert above this setting.
    /// </summary>
    public FSHeader HeaderAbove { get; private set; }
    /// <summary>
    /// Should this setting be hidden in mod settings?
    /// </summary>
    public bool HideInModSettings { get; private set; }
    /// <summary>
    /// Does this setting require a game restart to apply?
    /// </summary>
    public bool RequiresRestart { get; private set; }

    private object WrappedInstance { get; set; }

    /// <summary>
    /// FeatureSettings constructor
    /// </summary>
    /// <param name="featureSettingsHelper">The managing settings helper.</param>
    /// <param name="prop">The property holding the instance</param>
    /// <param name="instance">The instance</param>
    /// <param name="debugPath">A debug path</param>
    public FeatureSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "")
    {
        Helper = featureSettingsHelper;
        Prop = prop;
        Type = prop.GetMethod?.ReturnType;
        RequiresRestart = prop.GetCustomAttribute<RequiresRestart>() != null;

        if (featureSettingsHelper.Localization.TryGetFSText($"{prop.DeclaringType!.FullName}.{prop.Name}", FSType.FSDisplayName, out var displayName))
            DisplayName = $">{(RequiresRestart ? " <color=red>[!]</color>" : "")} {displayName}";
        else
            DisplayName = $">{(RequiresRestart ? " <color=red>[!]</color>" : "")} {prop.GetCustomAttribute<FSDisplayName>()?.DisplayName ?? prop.Name}";
        if (featureSettingsHelper.Localization.TryGetFSText($"{prop.DeclaringType.FullName}.{prop.Name}", FSType.FSDescription, out var description))
            Description = description;
        else
            Description = prop.GetCustomAttribute<FSDescription>()?.Description;
        HeaderAbove = prop.GetCustomAttribute<FSHeader>();
        if (HeaderAbove != null && featureSettingsHelper.Localization.TryGetFSText($"{prop.DeclaringType.FullName}.{prop.Name}", FSType.FSHeader, out var headerText))
            HeaderAbove = new(headerText, HeaderAbove.Color, HeaderAbove.Bold);

        Identifier = prop.GetCustomAttribute<FSIdentifier>()?.Identifier ?? ($"{prop.PropertyType.FullName}_{prop.Name}");
        RundownHint = prop.GetCustomAttribute<FSRundownHint>()?.Rundowns ?? RundownFlags.None;

        SeparatorAbove = prop.GetCustomAttribute<FSSeparator>() != null;
        SpacerAbove = prop.GetCustomAttribute<FSSpacer>() != null;

        HideInModSettings = prop.GetCustomAttribute<FSHide>() != null;
        Readonly = prop.GetCustomAttribute<FSReadOnly>()?.RecursiveReadOnly ?? false;
        TopLevelReadonly = !(prop.GetCustomAttribute<FSReadOnly>()?.RecursiveReadOnly ?? true);

        WrappedInstance = instance;
        DEBUG_Path = debugPath;
    }

    /// <summary>
    /// Sets the value on the settings properties instance and calls <see cref="FeaturesAPI.Feature.OnFeatureSettingChanged"/> if the feature is enabled.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <returns>The passed value.</returns>
    public virtual object SetValue(object value)
    {
        Prop.SetValue(WrappedInstance, value);
        Helper.IsDirty = true;
        FeatureManager.Instance.OnFeatureSettingChanged(this);
        return value;
    }

    /// <summary>
    /// Gets the value on the settings properties instance.
    /// </summary>
    /// <returns>The current value</returns>
    public virtual object GetValue()
    {
        return Prop.GetValue(WrappedInstance);
    }
}