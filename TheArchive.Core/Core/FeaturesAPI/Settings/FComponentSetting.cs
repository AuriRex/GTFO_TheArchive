using System.Reflection;
using TheArchive.Utilities;

namespace TheArchive.Core.FeaturesAPI.Settings;

public class FComponentSetting<T> : NotSavedFeatureSetting where T : class
{
    public T FComponent { get; private set; }

    public FComponentSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debug_path = "") : base(featureSettingsHelper, prop, instance, debug_path)
    {
        FComponent = (T) prop.GetValue(instance);
    }
}