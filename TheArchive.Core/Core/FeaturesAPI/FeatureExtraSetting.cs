using System;
using System.IO;
using System.Reflection;

namespace TheArchive.Core.FeaturesAPI;

[AttributeUsage(AttributeTargets.Property)]
public class FeatureExtraSetting : Attribute
{
    internal string FullPath { get; set; }

    internal string CustomPath { get; set; }

    internal string Alias { get; set; }

    internal PropertyInfo PropertyInfo { get; set; }

    internal Feature Feature { get; set; }

    public FeatureExtraSetting(string path, string alias = null)
    {
        CustomPath = path;
        Alias = alias;
    }

    internal void Setup(Feature feature, PropertyInfo propertyInfo)
    {
        Feature = feature;
        FullPath = Path.Combine(Path.GetDirectoryName(feature.FeatureInternal.ArchiveModule.GetType().Assembly.Location), "Settings", $"{CustomPath}.json");
        PropertyInfo = propertyInfo;
    }

    internal void Load()
    {
        var root = Path.GetDirectoryName(FullPath);
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        if (!File.Exists(FullPath)) return;
        PropertyInfo.SetValue(Feature, JsonConvert.DeserializeObject(File.ReadAllText(FullPath), PropertyInfo.PropertyType, ArchiveMod.JsonSerializerSettings));
    }

    internal void Save()
    {
        var root = Path.GetDirectoryName(FullPath);
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        File.WriteAllText(FullPath, JsonConvert.SerializeObject(PropertyInfo.GetValue(Feature), ArchiveMod.JsonSerializerSettings));
    }
}
