using System;
using System.Reflection;
using TheArchive.Core.FeaturesAPI.Components;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting handling the <see cref="FButton"/> component.
/// </summary>
public class ButtonSetting : FComponentSetting<FButton>
{
    /// <summary>
    /// The button text.
    /// </summary>
    public string ButtonText => FComponent.ButtonText;
    
    /// <summary>
    /// The buttons ID.
    /// </summary>
    /// <remarks>
    /// <list>
    /// <item>Use the <c>FSIdentifier</c> attribute to set a custom identifier.</item>
    /// <item>The default fallback button id is: <c>$"{DeclaringType.FullName}.{prop.Name}"</c>.</item>
    /// </list>
    /// </remarks>
    public string ButtonID => FComponent.ButtonID;

    /// <summary>
    /// This Callback fires even if the Feature is disabled whenever the button is pressed!
    /// </summary>
    public Action Callback => FComponent.Callback;

    /// <summary>
    /// If the submenu this setting belongs to should be refreshed after the button is pressed.
    /// </summary>
    public bool RefreshSubMenu => FComponent.RefreshSubMenu;

    /// <inheritdoc/>
    public ButtonSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        var propID = $"{prop.DeclaringType!.FullName}.{prop.Name}";
        
        if (string.IsNullOrWhiteSpace(FComponent.ButtonID))
        {
            FComponent.ButtonID = propID;
        }
        
        if (featureSettingsHelper.Localization.TryGetFSText(propID, Localization.FSType.FSButtonText, out var text))
        {
            FComponent.ButtonText = text;
        }
    }
}