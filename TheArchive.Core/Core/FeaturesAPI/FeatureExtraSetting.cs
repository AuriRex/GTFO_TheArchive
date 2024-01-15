using System;
using System.IO;
using System.Reflection;

namespace TheArchive.Core.FeaturesAPI;

[AttributeUsage(AttributeTargets.Property)]
public class FeatureExtraSetting : Attribute
{
    internal string FullPath { get; set; }

    internal string PathUnderSettings { get; set; }

    internal string Alias { get; set; }

    internal bool SaveOnQuit { get; set; }

    internal PropertyInfo PropertyInfo { get; set; }

    internal Feature Feature { get; set; }

    public FeatureExtraSetting(string path, bool saveOnQuit = true, string alias = null)
    {
        PathUnderSettings = path;
        Alias = alias;
        SaveOnQuit = saveOnQuit;
    }

    internal void Setup(Feature feature, PropertyInfo propertyInfo)
    {
        Feature = feature;
        FullPath = Path.Combine(Path.GetDirectoryName(feature.FeatureInternal.ArchiveModule.GetType().Assembly.Location), "Settings", $"{PathUnderSettings}.json");
        PropertyInfo = propertyInfo;
    }

    internal void Load()
    {
        var root = Path.GetDirectoryName(FullPath);
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        if (!File.Exists(FullPath)) return;
        PropertyInfo.SetValue(Feature, JsonConvert.DeserializeObject(File.ReadAllText(FullPath), PropertyInfo.PropertyType, ArchiveMod.JsonSerializerSettings));
    }

    internal void Save(bool force = false)
    {
        if (Feature.IsApplicationQuitting && SaveOnQuit || force)
        {
            var root = Path.GetDirectoryName(FullPath);
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);
            File.WriteAllText(FullPath, JsonConvert.SerializeObject(PropertyInfo.GetValue(Feature), ArchiveMod.JsonSerializerSettings));
        }
    }
}
