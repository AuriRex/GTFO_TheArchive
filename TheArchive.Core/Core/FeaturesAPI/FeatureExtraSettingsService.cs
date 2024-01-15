using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TheArchive.Core.FeaturesAPI;

internal class FeatureExtraSettingsService : IFeatureExtraSettingService
{
    public void Setup(Feature feature)
    {
        _feature = feature;
        _featureExtraSettings.Clear();
        var props = _feature.GetType().GetProperties().Where(p => p.GetCustomAttribute<FeatureExtraSetting>(true) != null);
        foreach (var prop in props)
        {
            if ((!prop.SetMethod?.IsStatic ?? true) || (!prop.GetMethod?.IsStatic ?? true))
                continue;

            var setting = prop.GetCustomAttribute<FeatureExtraSetting>();
            _featureExtraSettings[string.IsNullOrEmpty(setting.Alias) ? setting.CustomPath : setting.Alias] = setting;
            setting.Setup(_feature, prop);
        }
    }

    public void LoadAllSettings()
    {
        foreach (var setting in _featureExtraSettings.Values)
        {
            setting.Load();
        }
    }

    public void SaveAllSettings()
    {
        foreach (var setting in _featureExtraSettings.Values)
        {
            setting.Save();
        }
    }

    public void Load(string name)
    {
        if (_featureExtraSettings.TryGetValue(name, out var setting))
        {
            setting.Load();
        }
    }

    public void Save(string name)
    {
        if (_featureExtraSettings.TryGetValue(name, out var setting))
        {
            setting.Save();
        }
    }

    private Feature _feature;

    private readonly Dictionary<string, FeatureExtraSetting> _featureExtraSettings = new();
}

public interface IFeatureExtraSettingService
{
    void Load(string name);

    void Save(string name);
}

