using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using TheArchive.Utilities;

namespace TheArchive.Core.ModulesAPI;

/// <summary>
/// Custom settings saved per profile.
/// </summary>
/// <typeparam name="T">The settings type.</typeparam>
[PublicAPI]
public class CustomSetting<T> : ICustomSetting where T : new()
{
    /// <summary>
    /// Full path to the settings file.
    /// </summary>
    internal string FullPath { get; }

    /// <summary>
    /// File name (+ optional path prepended) without extension.<br/>
    /// This gets appended to the assembly location + "Settings" folder.
    /// </summary>
    internal string FileName { get; }

    /// <summary>
    /// The value of this custom setting.
    /// </summary>
    public T Value { get; set; }

    private Action<T> AfterLoad { get; }

    /// <inheritdoc/>
    public bool SaveOnQuit { get; }
    
    /// <inheritdoc/>
    public LoadingTime LoadingTime { get; }

    /// <summary>
    /// Creates a new custom setting instance and registers it.
    /// </summary>
    /// <param name="fileName">File name without extension! (extra folder path prefix optional!)</param>
    /// <param name="defaultValue">Default value of the setting.</param>
    /// <param name="afterLoad">Action to invoke after the setting has been loaded.</param>
    /// <param name="loadingTime">When to load the setting.</param>
    /// <param name="saveOnQuit">Should the custom setting be saved on game quit?</param>
    public CustomSetting(string fileName, T defaultValue, Action<T> afterLoad = null, LoadingTime loadingTime = LoadingTime.Immediately, bool saveOnQuit = true)
    {
        FileName = fileName;
        FullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Settings", $"{FileName}.json");
        Value = defaultValue;
        AfterLoad = afterLoad;
        SaveOnQuit = saveOnQuit;
        LoadingTime = loadingTime;
        CustomSettingManager.RegisterModuleSetting(this);
    }

    /// <inheritdoc/>
    public void Load()
    {
        var root = Path.GetDirectoryName(FullPath);
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);
        if (File.Exists(FullPath))
        {
            Value = JsonConvert.DeserializeObject<T>(File.ReadAllText(FullPath), ArchiveMod.JsonSerializerSettings);
        }

        AfterLoad?.Invoke(Value);
    }

    /// <inheritdoc/>
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

/// <summary>
/// Custom module settings interface.
/// </summary>
public interface ICustomSetting
{
    /// <summary>
    /// Load the custom setting.
    /// </summary>
    void Load();
    
    /// <summary>
    /// Save the custom setting.
    /// </summary>
    void Save();
    
    /// <summary>
    /// When to load the custom setting.
    /// </summary>
    LoadingTime LoadingTime { get; }
    
    /// <summary>
    /// Should the custom setting be saved on game quit?
    /// </summary>
    bool SaveOnQuit { get; }
}

/// <summary>
/// The loading time of a custom setting.
/// </summary>
public enum LoadingTime
{
    /// <summary> Don't load it at all. </summary>
    None,
    /// <summary> Load it as soon as possible. </summary>
    Immediately,
    /// <summary> Load it after game data has been initialized. </summary>
    AfterGameDataInited
}