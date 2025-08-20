using System.Reflection;
using TheArchive.Utilities;

namespace TheArchive.Core.FeaturesAPI.Settings;

/// <summary>
/// A feature setting base handling the different feature settings components.
/// </summary>
public class FComponentSetting<T> : NotSavedFeatureSetting where T : class
{
    /// <summary>
    /// The component instance (<typeparamref name="T"/>)
    /// </summary>
    public T FComponent { get; private set; }

    /// <inheritdoc/>
    public FComponentSetting(FeatureSettingsHelper featureSettingsHelper, PropertyInfo prop, object instance, string debugPath = "") : base(featureSettingsHelper, prop, instance, debugPath)
    {
        FComponent = (T) prop.GetValue(instance);
    }
}