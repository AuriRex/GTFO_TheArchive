using System;
using System.IO;
using System.Text;
using TheArchive.Core;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Utilities;

/// <summary>
/// Paths and Files.
/// </summary>
public static class LocalFiles
{
    /// <summary>
    /// GTFO settings file name (but .json)
    /// </summary>
    public const string GTFO_SETTINGS_JSON = "GTFO_Settings.json";
    /// <summary>
    /// GTFO player favorites file name (but .json)
    /// </summary>
    public const string GTFO_FAVORITES_JSON = "GTFO_Favorites.json";
    /// <summary>
    /// GTFO bot favorites file name (but .json)
    /// </summary>
    public const string GTFO_BOT_FAVORITES_JSON = "GTFO_BotFavorites.json";

    private static string _modLocalLowPath;
    /// <summary>
    /// Path to <c>User/AppData/LocalLow/GTFO_TheArchive</c>.
    /// </summary>
    public static string ModLocalLowPath
    {
        get
        {
            if (_modLocalLowPath != null)
                return _modLocalLowPath;
            
            _modLocalLowPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "GTFO_TheArchive");
            
            if (!Directory.Exists(_modLocalLowPath))
                Directory.CreateDirectory(_modLocalLowPath);
            
            return _modLocalLowPath;
        }
    }

    private static string _modDefaultGameLogsAndCachePath;
    /// <summary>
    /// The default logs and other cache file directory path.
    /// </summary>
    public static string ModDefaultGameLogsAndCachePath
    {
        get
        {
            if (_modDefaultGameLogsAndCachePath != null)
                return _modDefaultGameLogsAndCachePath;
            
            _modDefaultGameLogsAndCachePath = Path.Combine(ModLocalLowPath, "GameLogsAndCache");
            
            if (!Directory.Exists(_modDefaultGameLogsAndCachePath))
                Directory.CreateDirectory(_modDefaultGameLogsAndCachePath);
            
            return _modDefaultGameLogsAndCachePath;
        }
    }

    private static string _modDefaultSaveDataPath;
    /// <summary>
    /// The default <c>SaveData</c> path (inside <see cref="ModLocalLowPath"/>)
    /// </summary>
    public static string ModDefaultSaveDataPath
    {
        get
        {
            if (_modDefaultSaveDataPath != null)
                return _modDefaultSaveDataPath;
            
            _modDefaultSaveDataPath = Path.Combine(ModLocalLowPath, "SaveData");
            
            if (!Directory.Exists(_modLocalLowPath))
                Directory.CreateDirectory(_modLocalLowPath);
            
            return _modDefaultSaveDataPath;
        }
    }

    private static string _dataBlockDumpPath;
    /// <summary>
    /// Path for the datablock dumping functionality.
    /// </summary>
    public static string DataBlockDumpPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_dataBlockDumpPath))
                return _dataBlockDumpPath;
            
            _dataBlockDumpPath = Path.Combine(SaveDirectoryPath, "DataBlocks", $"Build_{BuildDB.BuildNumber}_{ArchiveMod.CurrentRundown}");
            
            if (!Directory.Exists(_dataBlockDumpPath))
                Directory.CreateDirectory(_dataBlockDumpPath);
            
            return _dataBlockDumpPath;
        }
    }

    private static string _savePath;
    /// <summary>
    /// Default is <see cref="ModDefaultSaveDataPath"/> (<c>LocalLow/GTFO_TheArchive/</c>), can be overridden by using <see cref="ArchiveSettings.CustomFileSaveLocation"/>
    /// </summary>
    public static string SaveDirectoryPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_savePath))
                return _savePath;
            
            _savePath = string.IsNullOrWhiteSpace(ArchiveMod.Settings.CustomFileSaveLocation) ? ModDefaultSaveDataPath : ArchiveMod.Settings.CustomFileSaveLocation;
            
            if (!Directory.Exists(_savePath))
                Directory.CreateDirectory(_savePath);
            
            return _savePath;
        }
    }

    private static string _gameLogsAndCacheSavePath;
    /// <summary>
    /// The real logs and other cache file directory path.
    /// </summary>
    public static string GameLogsAndCachePath
    {
        get
        {
            if (_gameLogsAndCacheSavePath != null)
                return _gameLogsAndCacheSavePath;
            
            _gameLogsAndCacheSavePath = string.IsNullOrWhiteSpace(ArchiveMod.Settings.CustomLogsAndCacheLocation) ? ModDefaultGameLogsAndCachePath : ArchiveMod.Settings.CustomLogsAndCacheLocation;
            
            if (!Directory.Exists(_gameLogsAndCacheSavePath))
                Directory.CreateDirectory(_gameLogsAndCacheSavePath);
            
            return _gameLogsAndCacheSavePath;
        }
    }

    private static string _versionSpecificLogsAndCachePath;
    /// <summary>
    /// A more specific logs and other cache file directory path depending on game version.
    /// </summary>
    public static string VersionSpecificLogsAndCachePath
    {
        get
        {
            if (!string.IsNullOrEmpty(_versionSpecificLogsAndCachePath))
                return _versionSpecificLogsAndCachePath;
            
            _versionSpecificLogsAndCachePath = Path.Combine(LocalFiles.GameLogsAndCachePath, $"{((int)ArchiveMod.CurrentRundown).ToString().PadLeft(2, '0')}_{ArchiveMod.CurrentRundown}_Data", "appdata");
            
            if (!Directory.Exists(_versionSpecificLogsAndCachePath))
                Directory.CreateDirectory(_versionSpecificLogsAndCachePath);
            
            return _versionSpecificLogsAndCachePath;
        }
    }

    private static string _versionSpecificSavePath;
    /// <summary>
    /// Path to game version specific file save directory.
    /// </summary>
    public static string VersionSpecificSaveDirectoryPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_versionSpecificSavePath))
                return _versionSpecificSavePath;
            
            _versionSpecificSavePath = GetVersionSpecificSaveDirectoryPath(ArchiveMod.CurrentRundown);

            if (Directory.Exists(_versionSpecificSavePath))
                return _versionSpecificSavePath;
            
            Directory.CreateDirectory(_versionSpecificSavePath);
            
            try
            {
                if (!CopyMostRecentSaveFiles(ArchiveMod.CurrentRundown - 1, ArchiveMod.CurrentRundown))
                {
                    ArchiveLogger.Notice("Creating new game settings file(s)!");
                }
            }
            catch (Exception ex)
            {
                ArchiveLogger.Warning($"Caught an exception while trying to copy over older settings files: {ex}: {ex.Message}");
                ArchiveLogger.Debug(ex.StackTrace);
            }

            return _versionSpecificSavePath;
        }
    }

    private static bool CopyFromBaseGameLocation(RundownID copyTo)
    {
        var defaultBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "10 Chambers Collective", "GTFO");

        var baseSettings = Path.Combine(defaultBasePath, "GTFO_Settings.txt");
        var baseFavorites = Path.Combine(defaultBasePath, "GTFO_Favorites.txt");
        var baseBotFavorites = Path.Combine(defaultBasePath, "GTFO_BotFavorites.txt");

        if (!File.Exists(baseSettings))
            return false;
        
        var newSettingsFile = GetSettingsPath(copyTo);
        ArchiveLogger.Debug($"Copying vanilla game settings file! (\"{baseSettings}\" -> \"{newSettingsFile}\")");
        File.Copy(baseSettings, newSettingsFile);

        var newFavs = GetFavoritesPath(copyTo);
        var newBotFavs = GetBotFavoritesPath(copyTo);

        if (File.Exists(baseFavorites))
        {
            ArchiveLogger.Debug($"Copying vanilla game favorites file! (\"{baseFavorites}\" -> \"{newFavs}\")");
            File.Copy(baseFavorites, newFavs);
        }

        if (File.Exists(baseBotFavorites))
        {
            ArchiveLogger.Debug($"Copying vanilla game bot favorites file! (\"{baseBotFavorites}\" -> \"{newBotFavs}\")");
            File.Copy(baseBotFavorites, newBotFavs);
        }

        return true;

    }

    private static bool CopyMostRecentSaveFiles(RundownID copyFrom, RundownID copyTo, int maxStep = 3)
    {
        if (copyFrom < RundownID.RundownOne)
            return CopyFromBaseGameLocation(copyTo);

        var olderSettingsFile = GetSettingsPath(copyFrom);

        if (!File.Exists(olderSettingsFile))
        {
            if (maxStep <= 1)
            {
                return CopyFromBaseGameLocation(copyTo);
            }

            return CopyMostRecentSaveFiles(copyFrom - 1, copyTo, maxStep - 1);
        }

        var newSettingsFile = GetSettingsPath(copyTo);
        ArchiveLogger.Debug($"Copying most recent settings file! (\"{olderSettingsFile}\" -> \"{newSettingsFile}\")");
        File.Copy(olderSettingsFile, newSettingsFile);

        if (ArchiveMod.IsPlayingModded)
            return true;
        
        var newFavs = GetFavoritesPath(copyTo);
        var oldFavs = GetFavoritesPath(copyFrom);

        if (File.Exists(oldFavs))
        {
            ArchiveLogger.Debug($"Copying most recent favorites file! (\"{oldFavs}\" -> \"{newFavs}\")");
            File.Copy(oldFavs, newFavs);
        }

        var newBotFavs = GetBotFavoritesPath(copyTo);
        var oldBotFavs = GetBotFavoritesPath(copyFrom);

        if (File.Exists(oldBotFavs))
        {
            ArchiveLogger.Debug($"Copying most recent bot favorites file! (\"{oldBotFavs}\" -> \"{newBotFavs}\")");
            File.Copy(oldBotFavs, newBotFavs);
        }

        return true;
    }

    /// <summary>
    /// Get the save directory for a specific rundown ID game version.
    /// </summary>
    /// <param name="rundown">The rundown to get the save path for.</param>
    /// <returns>The path to the version specific save folder.</returns>
    public static string GetVersionSpecificSaveDirectoryPath(RundownID rundown)
    {
        return Path.Combine(SaveDirectoryPath, $"{((int)rundown).ToString().PadLeft(2, '0')}_{rundown}_Data");
    }

    private static string _otherConfigsPath;
    /// <summary>
    /// The path where misc config files go to.
    /// </summary>
    public static string OtherConfigsDirectoryPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_otherConfigsPath))
                return _otherConfigsPath;
            
            _otherConfigsPath = Path.Combine(SaveDirectoryPath, "OtherConfigs");

            if (!Directory.Exists(_otherConfigsPath))
                Directory.CreateDirectory(_otherConfigsPath);
            
            return _otherConfigsPath;
        }
    }

    private static string _featureConfigsPath;
    /// <summary>
    /// The base path for feature config files.
    /// </summary>
    public static string FeatureConfigsDirectoryPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_featureConfigsPath))
                return _featureConfigsPath;
            
            _featureConfigsPath = Path.Combine(SaveDirectoryPath, "FeatureSettings");

            if (!Directory.Exists(_featureConfigsPath))
                Directory.CreateDirectory(_featureConfigsPath);
            
            return _featureConfigsPath;
        }
    }

    private static string _filesPath;
    /// <summary>
    /// A files folder (version specific).
    /// </summary>
    [Obsolete("Legacy path.")]
    public static string FilesDirectoryPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_filesPath))
                return _filesPath;
            
            _filesPath = Path.Combine(VersionSpecificSaveDirectoryPath, "Files");
            
            if (!Directory.Exists(_filesPath))
                Directory.CreateDirectory(_filesPath);
            
            return _filesPath;
        }
    }

    private static string _settingsPath;
    /// <summary>
    /// The custom path to the version specific game settings file.
    /// </summary>
    public static string SettingsPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_settingsPath))
            {
                return _settingsPath;
            }
            
            _settingsPath = Path.Combine(VersionSpecificSaveDirectoryPath, GTFO_SETTINGS_JSON);

            return _settingsPath;
        }
    }

    /// <summary>
    /// Get the settings file for another rundown game version.
    /// </summary>
    /// <param name="rundown">The rundown to get the path for.</param>
    /// <returns>The game settings file path for a specific game version.</returns>
    public static string GetSettingsPath(RundownID rundown) => Path.Combine(GetVersionSpecificSaveDirectoryPath(rundown), GTFO_SETTINGS_JSON);

    private static string _favoritesPath;
    /// <summary>
    /// GTFO player favorites file path.
    /// </summary>
    public static string FavoritesPath
    {
        get
        {
            if (string.IsNullOrEmpty(_favoritesPath))
                _favoritesPath = Path.Combine(VersionSpecificSaveDirectoryPath, GTFO_FAVORITES_JSON);
            return _favoritesPath;
        }
    }

    /// <summary>
    /// Get the player favorites file for another rundown game version.
    /// </summary>
    /// <param name="rundown">The rundown to get the path for.</param>
    /// <returns>The player favorites file path for a specific game version.</returns>
    public static string GetFavoritesPath(RundownID rundown) => Path.Combine(GetVersionSpecificSaveDirectoryPath(rundown), GTFO_FAVORITES_JSON);


    private static string _botFavoritesPath;
    /// <summary>
    /// GTFO bot favorites file path.
    /// </summary>
    public static string BotFavoritesPath
    {
        get
        {
            if (string.IsNullOrEmpty(_botFavoritesPath))
                _botFavoritesPath = Path.Combine(VersionSpecificSaveDirectoryPath, GTFO_BOT_FAVORITES_JSON);
            return _botFavoritesPath;
        }
    }

    /// <summary>
    /// Get the bot favorites file for another rundown game version.
    /// </summary>
    /// <param name="rundown">The rundown to get the path for.</param>
    /// <returns>The bot favorites file path for a specific game version.</returns>
    public static string GetBotFavoritesPath(RundownID rundown) => Path.Combine(GetVersionSpecificSaveDirectoryPath(rundown), GTFO_BOT_FAVORITES_JSON);
    
    /// <summary>
    /// Load a miscellaneous config file from the <see cref="OtherConfigsDirectoryPath"/>.
    /// </summary>
    /// <param name="fileExists"><c>False</c> if the file was just created.</param>
    /// <param name="saveIfNonExistent">Save an empty config immediately if the file doesn't exist yet.</param>
    /// <typeparam name="T">The config file type.</typeparam>
    /// <returns>The loaded config file instance.</returns>
    public static T LoadConfig<T>(out bool fileExists, bool saveIfNonExistent = true) where T : new()
    {
        var path = Path.Combine(OtherConfigsDirectoryPath, $"{typeof(T).Name}.json");
        
        try
        {
            if (!File.Exists(path))
            {
                var newT = new T();
                if (saveIfNonExistent)
                {
                    SaveConfig(newT);
                }

                fileExists = false;
                return newT;
            }

            fileExists = true;
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), ArchiveMod.JsonSerializerSettings);
        }
        catch (Exception ex)
        {
            ArchiveLogger.Error($"An error occured while loading config file {typeof(T).Name}.json");
            ArchiveLogger.Exception(ex);
        }

        fileExists = false;
        return new T();
    }
    
    /// <summary>
    /// Load a miscellaneous config file from the <see cref="OtherConfigsDirectoryPath"/>.
    /// </summary>
    /// <param name="saveIfNonExistent">Save an empty config immediately if the file doesn't exist yet.</param>
    /// <typeparam name="T">The config file type.</typeparam>
    /// <returns>The loaded config file instance.</returns>
    public static T LoadConfig<T>(bool saveIfNonExistent = true) where T : new()
    {
        return LoadConfig<T>(out _, saveIfNonExistent);
    }

    /// <summary>
    /// Save a miscellaneous config file to the <see cref="OtherConfigsDirectoryPath"/>.
    /// </summary>
    /// <param name="config">The config instance to save.</param>
    /// <typeparam name="T">The config file type.</typeparam>
    public static void SaveConfig<T>(T config)
    {
        try
        {
            var path = Path.Combine(OtherConfigsDirectoryPath, $"{typeof(T).Name}.json");

            File.WriteAllText(path, JsonConvert.SerializeObject(config, ArchiveMod.JsonSerializerSettings));
        }
        catch (Exception ex)
        {
            ArchiveLogger.Error($"An error occured while saving config file {typeof(T).Name}.json");
            ArchiveLogger.Exception(ex);
        }
    }

    private static object LoadFeatureConfig(string moduleIdentifier, string featureIdentifier, Type configType, out bool fileExists, bool saveIfNonExistent = true)
    {
        if (string.IsNullOrWhiteSpace(featureIdentifier))
            throw new ArgumentException($"Parameter {nameof(featureIdentifier)} may not be null or whitespace.");
        if (configType == null)
            throw new ArgumentNullException(nameof(configType));

        var moduleSettingsPath = Path.Combine(FeatureConfigsDirectoryPath, moduleIdentifier);
        if (!Directory.Exists(moduleSettingsPath))
        {
            // Make sure settings are carried over when migrating from the AIO build to the split one
            if (moduleIdentifier == "TheArchive.Essentials")
            {
                var oldPath = Path.Combine(FeatureConfigsDirectoryPath, "TheArchive.IL2CPP");
                if (Directory.Exists(oldPath)) // copy Hikaria build configs
                {
                    ArchiveLogger.Msg(ConsoleColor.Green, $"Migrating old config path from \"{oldPath}\" to \"{moduleSettingsPath}\"");
                    Directory.Move(oldPath, moduleSettingsPath);
                }
                else // Copy Legacy Archive build configs lazily
                {
                    Directory.CreateDirectory(moduleSettingsPath);
                    ArchiveLogger.Msg(ConsoleColor.Green, $"Migrating old config files from \"{FeatureConfigsDirectoryPath}\" to \"{moduleSettingsPath}\"");
                    foreach (var legacyFilePath in Directory.EnumerateFiles(FeatureConfigsDirectoryPath, "*.json",
                                 SearchOption.TopDirectoryOnly))
                    {
                        var newFilePath = Path.Combine(moduleSettingsPath, Path.GetFileName(legacyFilePath));
                        ArchiveLogger.Debug($"Copying \"{legacyFilePath}\" -> \"{newFilePath}\"");
                        File.Copy(legacyFilePath, newFilePath);
                    }
                }
            }
            else Directory.CreateDirectory(moduleSettingsPath);
        }

        var path = Path.Combine(moduleSettingsPath, $"{featureIdentifier}_{configType.Name}.json");

        if (!File.Exists(path))
        {
            var newT = Activator.CreateInstance(configType);
            if (saveIfNonExistent)
            {
                SaveFeatureConfig(moduleIdentifier, featureIdentifier, configType, newT);
            }
            fileExists = false;
            return newT;
        }

        fileExists = true;
        try
        {
            return JsonConvert.DeserializeObject(File.ReadAllText(path), configType, ArchiveMod.JsonSerializerSettings);
        }
        catch
        {
            var newT = Activator.CreateInstance(configType);
            if (saveIfNonExistent)
            {
                SaveFeatureConfig(moduleIdentifier, featureIdentifier, configType, newT);
            }
            fileExists = false;
            return newT;
        }
    }

    internal static FeatureLocalizationData LoadFeatureLocalizationText(Feature feature)
    {
        var asmLocation = feature.FeatureInternal.OriginAssembly.Location;

        if (string.IsNullOrWhiteSpace(asmLocation))
        {
            ArchiveLogger.Warning($"Feature \"{feature.Name}\"'s OriginAssembly.Location is null or whitespace. (ID:{feature.Identifier})");
            ArchiveLogger.Warning("Localization on dynamic assemblies is not supported currently!");
            return new();
        }
        
        var dir = Path.Combine(Path.GetDirectoryName(asmLocation)!, "Localization");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var path = Path.Combine(dir, $"{feature.Identifier}_Localization.json");
        if (!File.Exists(path))
        {
            var newData = FeatureInternal.GenerateFeatureLocalization(feature);
            File.WriteAllText(path, JsonConvert.SerializeObject(newData, ArchiveMod.JsonSerializerSettings));
            return newData;
        }

        var data = JsonConvert.DeserializeObject<FeatureLocalizationData>(File.ReadAllText(path), ArchiveMod.JsonSerializerSettings);
        var json = JsonConvert.SerializeObject(data, ArchiveMod.JsonSerializerSettings);
        var rdata = FeatureInternal.GenerateFeatureLocalization(feature, data);
        var rjson = JsonConvert.SerializeObject(rdata, ArchiveMod.JsonSerializerSettings);
        if (rjson.HashString() != json.HashString())
            File.WriteAllText(path, rjson);
        return rdata;
    }

    internal static object LoadFeatureConfig(string moduleIdentifier, string featureIdentifier, Type configType, bool saveIfNonExistent = true)
    {
        return LoadFeatureConfig(moduleIdentifier, featureIdentifier, configType, out _, saveIfNonExistent);
    }

    private static readonly FileStreamOptions Options = new()
    {
        Access = FileAccess.Write,
        Mode = FileMode.Create,
        Options = FileOptions.WriteThrough,
    };
    
    internal static void SaveFeatureConfig(string moduleIdentifier, string featureIdentifier, Type configType, object configInstance)
    {
        if (string.IsNullOrWhiteSpace(featureIdentifier))
            throw new ArgumentException($"Parameter {nameof(featureIdentifier)} may not be null or whitespace.");
        if (configType == null)
            throw new ArgumentNullException(nameof(configType));
        if (configInstance == null)
            throw new ArgumentNullException(nameof(configInstance));

        var moduleSettingsPath = Path.Combine(FeatureConfigsDirectoryPath, moduleIdentifier);
        if (!Directory.Exists(moduleSettingsPath))
            Directory.CreateDirectory(moduleSettingsPath);

        var path = Path.Combine(moduleSettingsPath, $"{featureIdentifier}_{configType.Name}.json");

        ArchiveLogger.Debug($"Saving Feature Setting to: {path}");

        var json = JsonConvert.SerializeObject(configInstance, ArchiveMod.JsonSerializerSettings);

        try
        {
            using var sw = new StreamWriter(path, Encoding.UTF8, Options);
            sw.Write(json);
            sw.Flush();
        }
        catch (Exception ex)
        {
            ArchiveLogger.Error($"Threw an exception while trying to save file '{path}'.");
            ArchiveLogger.Exception(ex);
        }
    }
}