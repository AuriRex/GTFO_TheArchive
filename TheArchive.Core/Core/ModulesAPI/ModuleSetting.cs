using System;
using System.IO;
using System.Reflection;

namespace TheArchive.Core.ModulesAPI;

public class ModuleSetting<T> : IModuleSetting where T : new()
{
    internal string FullPath { get; private set; }

    internal string FilePath { get; set; }

    public T Value { get; set; }

    private Action AfterLoad { get; set; }

    public bool SaveOnQuit { get; private set; }

    private WhenToLoad m_whenToLoad;

    WhenToLoad IModuleSetting.LoadTime => m_whenToLoad;

    public ModuleSetting(string path, T defaultValue, Action afterLoad = null, WhenToLoad whenToLoad = WhenToLoad.Immediately, bool saveOnQuit = true)
    {
        FilePath = path;
        FullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Settings", $"{FilePath}.json");
        Value = defaultValue;
        AfterLoad = afterLoad;
        SaveOnQuit = saveOnQuit;
        m_whenToLoad = whenToLoad;
        ModuleSettingManager.RegisterModuleSetting(this);
    }

    public void Load()
    {
        var root = Path.GetDirectoryName(FullPath);
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        if (File.Exists(FullPath))
        {
            Value = JsonConvert.DeserializeObject<T>(File.ReadAllText(FullPath), ArchiveMod.JsonSerializerSettings);
        }
        Action afterLoad = AfterLoad;
        if (afterLoad != null) afterLoad();
    }

    public void Save()
    {
        var root = Path.GetDirectoryName(FullPath);
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        File.WriteAllText(FullPath, JsonConvert.SerializeObject(Value, ArchiveMod.JsonSerializerSettings));
    }
}

public interface IModuleSetting
{
    void Load();

    void Save();

    WhenToLoad LoadTime { get; }

    bool SaveOnQuit { get; }
}

public enum WhenToLoad
{
    None,
    Immediately,
    AfterGameDataInited
}