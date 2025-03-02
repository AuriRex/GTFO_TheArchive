using System;

namespace TheArchive.Core.FeaturesAPI;

public class DynamicFeatureSettingsHelper : FeatureSettingsHelper
{
    public DynamicFeatureSettingsHelper(Feature feature, FeatureSettingsHelper parentHelper = null)
    {
        Feature = feature;
        ParentHelper = parentHelper;
    }

    /// <summary>
    /// Initialize the <see cref="FeatureSettingsHelper.Settings"/> list and returns itself.
    /// </summary>
    /// <param name="typeToCheck"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    public DynamicFeatureSettingsHelper Initialize(Type typeToCheck, object instance)
    {
        PopulateSettings(typeToCheck, instance, string.Empty);
        return this;
    }

    internal override void SetupViaFeatureInstance(object objectInstance)
    {
        throw new NotImplementedException();
    }

    public override object GetFeatureInstance()
    {
        throw new NotImplementedException();
    }
}