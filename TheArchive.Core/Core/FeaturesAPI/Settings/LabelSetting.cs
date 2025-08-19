using System.Reflection;
using TheArchive.Core.FeaturesAPI.Components;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting handling the <see cref="FLabel"/> component.
/// </summary>
public class LabelSetting : FComponentSetting<FLabel>
{
    /// <summary>
    /// The labels text
    /// </summary>
    public string LabelText => FComponent.LabelText;
    
    /// <summary>
    /// The labels ID
    /// </summary>
    /// <remarks>
    /// <list>
    /// <item>Use the <c>FSIdentifier</c> attribute to set a custom identifier</item>
    /// <item>The default fallback label id is: <c>$"{DeclaringType.FullName}.{prop.Name}"</c></item>
    /// </list>
    /// </remarks>
    public string LabelID => FComponent.LabelID;

    /// <inheritdoc/>
    public LabelSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        var propID = $"{prop.DeclaringType!.FullName}.{prop.Name}";

        if (string.IsNullOrWhiteSpace(FComponent.LabelID))
        {
            FComponent.LabelID = propID;
        }
        
        if (featureSettingsHelper.Localization.TryGetFSText(propID, Localization.FSType.FSLabelText, out var text))
        {
            FComponent.LabelText = text;
        }
    }
}