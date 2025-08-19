using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting that's actually a submenu.
/// </summary>
public class SubmenuSetting : FeatureSetting
{
    /// <summary>
    /// The settings helper responsible for submenu population
    /// </summary>
    public FeatureSettingsHelper SettingsHelper { get; private set; }

    /// <summary>
    /// If a dynamic submenu should be used instead of a regular one.
    /// </summary>
    public bool UseDynamicMenu { get; private set; }

    /// <inheritdoc/>
    public SubmenuSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object host, string debugPath = "") : base(featureSettingsHelper, prop, prop.GetValue(host), debugPath)
    {
        SettingsHelper = new FeatureSettingsHelper(featureSettingsHelper.Feature, prop);
        SettingsHelper.SetupViaInstanceOnHost(host, prop.GetValue(host));

        UseDynamicMenu = prop.GetCustomAttribute<FSUseDynamicSubmenu>() != null;
    }
}