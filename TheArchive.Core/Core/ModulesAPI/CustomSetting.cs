using System;
using System.IO;
using System.Reflection;
using TheArchive.Utilities;

namespace TheArchive.Core.ModulesAPI;

public class CustomSetting<T> : ICustomSetting where T : new()
{
    internal string FullPath { get; private set; }

    internal string FilePath { get; private set; }

    public T Value { get; set; }

    private Action<T> AfterLoad { get; set; }

    public bool SaveOnQuit { get; private set; }

    private LoadingTime LoadingTime;

    LoadingTime ICustomSetting.LoadingTime => LoadingTime;

    public CustomSetting(string path, T defaultValue, Action<T> afterLoad = null, LoadingTime loadingTime = LoadingTime.Immediately, bool saveOnQuit = true)
    {
        FilePath = path;
        FullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Settings", $"{FilePath}.json");
        Value = defaultValue;
        AfterLoad = afterLoad;
        SaveOnQuit = saveOnQuit;
        LoadingTime = loadingTime;
        CustomSettingManager.RegisterModuleSetting(this);
    }

    public void Load()
    {
        var root = Path.GetDirectoryName(FullPath);
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        if (File.Exists(FullPath))
        {
            Value = JsonConvert.DeserializeObject<T>(File.ReadAllText(FullPath), ArchiveMod.JsonSerializerSettings);
        }
        Action<T> afterLoad = AfterLoad;
        if (afterLoad != null) afterLoad(Value);
    }

    public void Save()
    {
        var root = Path.GetDirectoryName(FullPath);
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        string json = string.Empty;
        if (File.Exists(FullPath))
            json = File.ReadAllText(FullPath);
        var rjson = JsonConvert.SerializeObject(Value, ArchiveMod.JsonSerializerSettings);
        if (json.HashString() != rjson.HashString())
            File.WriteAllText(FullPath, rjson);
    }
}

public interface ICustomSetting
{
    void Load();

    void Save();

    LoadingTime LoadingTime { get; }

    bool SaveOnQuit { get; }
}

public enum LoadingTime
{
    None,
    Immediately,
    AfterGameDataInited
}