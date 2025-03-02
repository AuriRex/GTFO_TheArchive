using System.Reflection;
using TheArchive.Core.Attributes.Feature.Settings;

namespace TheArchive.Core.FeaturesAPI.Settings;

public class SubmenuSetting : FeatureSetting
{
    public FeatureSettingsHelper SettingsHelper { get; private set; }

    public bool UseDynamicMenu { get; private set; }

    public SubmenuSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object host, string debug_path = "") : base(featureSettingsHelper, prop, prop.GetValue(host), debug_path)
    {
        SettingsHelper = new FeatureSettingsHelper(featureSettingsHelper.Feature, prop);
        SettingsHelper.SetupViaInstanceOnHost(host, prop.GetValue(host));

        UseDynamicMenu = prop.GetCustomAttribute<FSUseDynamicSubmenu>() != null;
    }
}