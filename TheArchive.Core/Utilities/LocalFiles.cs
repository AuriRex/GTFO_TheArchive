using System;
using System.IO;
using TheArchive.Core;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Loader;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Utilities
{
    public class LocalFiles
    {

        public const string GTFO_SETTINGS_JSON = "GTFO_Settings.json";
        public const string GTFO_FAVORITES_JSON = "GTFO_Favorites.json";
        public const string GTFO_BOT_FAVORITES_JSON = "GTFO_BotFavorites.json";


        private static string _modLocalLowPath = null;
        public static string ModLocalLowPath
        {
            get
            {
                if(_modLocalLowPath == null)
                {
                    
                    _modLocalLowPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "GTFO_TheArchive");
                    if (!Directory.Exists(_modLocalLowPath))
                        Directory.CreateDirectory(_modLocalLowPath);
                }
                return _modLocalLowPath;
            }
        }

        private static string _modDefaultGameLogsAndCachePath = null;
        public static string ModDefaultGameLogsAndCachePath
        {
            get
            {
                if (_modDefaultGameLogsAndCachePath == null)
                {
                    _modDefaultGameLogsAndCachePath = Path.Combine(ModLocalLowPath, "GameLogsAndCache");
                    if (!Directory.Exists(_modDefaultGameLogsAndCachePath))
                        Directory.CreateDirectory(_modDefaultGameLogsAndCachePath);
                }
                return _modDefaultGameLogsAndCachePath;
            }
        }

        private static string _modDefaultSaveDataPath = null;
        public static string ModDefaultSaveDataPath
        {
            get
            {
                if (_modDefaultSaveDataPath == null)
                {
                    _modDefaultSaveDataPath = Path.Combine(ModLocalLowPath, "SaveData");
                    if (!Directory.Exists(_modLocalLowPath))
                        Directory.CreateDirectory(_modLocalLowPath);
                }
                return _modDefaultSaveDataPath;
            }
        }

        private static string _dataBlockDumpPath = null;
        public static string DataBlockDumpPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dataBlockDumpPath))
                {
                    _dataBlockDumpPath = Path.Combine(SaveDirectoryPath, "DataBlocks", $"Build_{BuildDB.BuildNumber}_{ArchiveMod.CurrentRundown}");
                    if (!Directory.Exists(_dataBlockDumpPath))
                        Directory.CreateDirectory(_dataBlockDumpPath);
                }
                return _dataBlockDumpPath;
            }
        }

        private static string _savePath = null;
        /// <summary>
        /// Default is <see cref="ModDefaultSaveDataPath"/> (LocalLow/GTFO_TheArchive/), can be overridden by using <see cref="ArchiveSettings.CustomFileSaveLocation"/>
        /// </summary>
        public static string SaveDirectoryPath
        {
            get
            {
                if(string.IsNullOrEmpty(_savePath))
                {
                    _savePath = string.IsNullOrWhiteSpace(ArchiveMod.Settings.CustomFileSaveLocation) ? ModDefaultSaveDataPath : ArchiveMod.Settings.CustomFileSaveLocation;
                    if (!Directory.Exists(_savePath))
                        Directory.CreateDirectory(_savePath);
                }
                return _savePath;
            }
        }

        private static string _gameLogsAndCacheSavePath = null;
        public static string GameLogsAndCachePath
        {
            get
            {
                if (_gameLogsAndCacheSavePath == null)
                {
                    _gameLogsAndCacheSavePath = string.IsNullOrWhiteSpace(ArchiveMod.Settings.CustomLogsAndCacheLocation) ? ModDefaultGameLogsAndCachePath : ArchiveMod.Settings.CustomLogsAndCacheLocation;
                    if (!Directory.Exists(_gameLogsAndCacheSavePath))
                        Directory.CreateDirectory(_gameLogsAndCacheSavePath);
                }
                return _gameLogsAndCacheSavePath;
            }
        }

        private static string _versionSpecificLogsAndCachePath = null;
        public static string VersionSpecificLogsAndCachePath
        {
            get
            {
                if (string.IsNullOrEmpty(_versionSpecificLogsAndCachePath))
                {
                    _versionSpecificLogsAndCachePath = Path.Combine(LocalFiles.GameLogsAndCachePath, $"{((int)ArchiveMod.CurrentRundown).ToString().PadLeft(2, '0')}_{ArchiveMod.CurrentRundown}_Data", "appdata");
                    if (!Directory.Exists(_versionSpecificLogsAndCachePath))
                        Directory.CreateDirectory(_versionSpecificLogsAndCachePath);
                }
                return _versionSpecificLogsAndCachePath;
            }
        }

        private static string _versionSpecificSavePath = null;
        public static string VersionSpecificSaveDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_versionSpecificSavePath))
                {
                    if(LoaderWrapper.IsModInstalled(ArchiveMod.MTFO_GUID))
                    {
#warning TODO: Not this
                        _versionSpecificSavePath = Path.Combine(SaveDirectoryPath, "Modded", $"TODO_USE_MTFO_FOLDER_HERE_Data");
                    }
                    else
                    {
                        _versionSpecificSavePath = GetVersionSpecificSaveDirectoryPath(ArchiveMod.CurrentRundown);
                    }

                    if (!Directory.Exists(_versionSpecificSavePath))
                    {
                        Directory.CreateDirectory(_versionSpecificSavePath);
                        try
                        {
                            if(!CopyMostRecentSaveFiles(ArchiveMod.CurrentRundown - 1, ArchiveMod.CurrentRundown))
                            {
                                ArchiveLogger.Notice("Creating new game settings file(s)!");
                            }
                        }
                        catch(Exception ex)
                        {
                            ArchiveLogger.Warning($"Caught an exception while trying to copy over older settings files: {ex}: {ex.Message}");
                            ArchiveLogger.Debug(ex.StackTrace);
                        }
                    }
                }

                return _versionSpecificSavePath;
            }
        }

        internal static bool CopyFromBaseGameLocation(RundownID copyTo)
        {
            var defaultBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "10 Chambers Collective", "GTFO");

            var baseSettings = Path.Combine(defaultBasePath, "GTFO_Settings.txt");
            var baseFavorites = Path.Combine(defaultBasePath, "GTFO_Favorites.txt");
            var baseBotFavorites = Path.Combine(defaultBasePath, "GTFO_BotFavorites.txt");

            if (File.Exists(baseSettings))
            {
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

            return false;
        }

        internal static bool CopyMostRecentSaveFiles(RundownID copyFrom, RundownID copyTo, int maxStep = 3)
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

            if (!ArchiveMod.IsPlayingModded)
            {
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
            }

            return true;
        }

        public static string GetVersionSpecificSaveDirectoryPath(RundownID rundown)
        {
            return Path.Combine(SaveDirectoryPath, $"{((int)rundown).ToString().PadLeft(2, '0')}_{rundown}_Data");
        }

        private static string _otherConfigsPath = null;
        public static string OtherConfigsDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_otherConfigsPath))
                {
                    _otherConfigsPath = Path.Combine(SaveDirectoryPath, "OtherConfigs");

                    if (!Directory.Exists(_otherConfigsPath))
                        Directory.CreateDirectory(_otherConfigsPath);
                }
                return _otherConfigsPath;
            }
        }

        private static string _featureConfigsPath = null;
        public static string FeatureConfigsDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_featureConfigsPath))
                {
                    _featureConfigsPath = Path.Combine(SaveDirectoryPath, "FeatureSettings");

                    if (!Directory.Exists(_featureConfigsPath))
                        Directory.CreateDirectory(_featureConfigsPath);
                }
                return _featureConfigsPath;
            }
        }

        private static string _filesPath = null;
        public static string FilesDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_filesPath))
                {
                    _filesPath = Path.Combine(VersionSpecificSaveDirectoryPath, "Files");
                    if (!Directory.Exists(_filesPath))
                        Directory.CreateDirectory(_filesPath);
                }
                return _filesPath;
            }
        }

        private static string _settingsPath = null;
        public static string SettingsPath
        {
            get
            {
                if (string.IsNullOrEmpty(_settingsPath))
                    _settingsPath = Path.Combine(VersionSpecificSaveDirectoryPath, GTFO_SETTINGS_JSON);
                return _settingsPath;
            }
        }

        public static string GetSettingsPath(RundownID rundown) => Path.Combine(GetVersionSpecificSaveDirectoryPath(rundown), GTFO_SETTINGS_JSON);

        private static string _favoritesPath = null;
        public static string FavoritesPath
        {
            get
            {
                if (string.IsNullOrEmpty(_favoritesPath))
                    _favoritesPath = Path.Combine(VersionSpecificSaveDirectoryPath, GTFO_FAVORITES_JSON);
                return _favoritesPath;
            }
        }

        public static string GetFavoritesPath(RundownID rundown) => Path.Combine(GetVersionSpecificSaveDirectoryPath(rundown), GTFO_FAVORITES_JSON);


        private static string _botFavoritesPath = null;
        public static string BotFavoritesPath
        {
            get
            {
                if (string.IsNullOrEmpty(_botFavoritesPath))
                    _botFavoritesPath = Path.Combine(VersionSpecificSaveDirectoryPath, GTFO_BOT_FAVORITES_JSON);
                return _botFavoritesPath;
            }
        }

        public static string GetBotFavoritesPath(RundownID rundown) => Path.Combine(GetVersionSpecificSaveDirectoryPath(rundown), GTFO_BOT_FAVORITES_JSON);


        private static string _boostersPath = null;
        public static string BoostersPath
        {
            get
            {
                if (string.IsNullOrEmpty(_boostersPath))
                    _boostersPath = Path.Combine(VersionSpecificSaveDirectoryPath, $"Booster_Data.json");
                return _boostersPath;
            }
        }

        private static string _vanityItemsPath = null;
        public static string VanityItemsPath
        {
            get
            {
                if (string.IsNullOrEmpty(_vanityItemsPath))
                    _vanityItemsPath = Path.Combine(VersionSpecificSaveDirectoryPath, $"VanityItems_Data.json");
                return _vanityItemsPath;
            }
        }

        private static string _vanityItemsLayerDropsPath = null;
        public static string VanityItemsLayerDropsPath
        {
            get
            {
                if (string.IsNullOrEmpty(_vanityItemsLayerDropsPath))
                    _vanityItemsLayerDropsPath = Path.Combine(VersionSpecificSaveDirectoryPath, $"VanityItemsLayerDrops_Data.json");
                return _vanityItemsLayerDropsPath;
            }
        }

        private static string _progressionPath = null;
        private static string LocalProgressionBasePathNoExtension
        {
            get
            {
                if (string.IsNullOrEmpty(_progressionPath))
                    _progressionPath = Path.Combine(VersionSpecificSaveDirectoryPath, $"RundownProgression_Data");
                return _progressionPath;
            }
        }

        public static string GetLocalProgressionPathForKey(string rundownKey)
        {
            return $"{LocalProgressionBasePathNoExtension}_{rundownKey}.json";
        }

        public static void SaveToFilesDir(string filename, string jsonOrSomething)
        {

            string path = Path.Combine(FilesDirectoryPath, filename);
            if (string.IsNullOrEmpty(jsonOrSomething))
            {
                ArchiveLogger.Msg(ConsoleColor.DarkRed, $"Saving \"{filename}\" to disk failed because the data is null or empty! Path: {path}");
                return;
            }
            ArchiveLogger.Msg(ConsoleColor.Blue, $"Saving \"{filename}\" to disk at: {path}");
            File.WriteAllText(path, jsonOrSomething);
        }

        public static string LoadFromFilesDir(string filename, bool isJson = true)
        {
            string path = Path.Combine(FilesDirectoryPath, filename);
            ArchiveLogger.Msg(ConsoleColor.Green, $"Loading \"{filename}\" from disk at: {path}");
            if (!File.Exists(path))
                return isJson ? "{}" : string.Empty;
            return File.ReadAllText(path);
        }

        public static T LoadConfig<T>(out bool fileExists, bool saveIfNonExistent = true) where T : new()
        {
            var path = Path.Combine(OtherConfigsDirectoryPath, $"{typeof(T).Name}.json");

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
        public static T LoadConfig<T>(bool saveIfNonExistent = true) where T : new()
        {
            return LoadConfig<T>(out _, saveIfNonExistent);
        }

        public static void SaveConfig<T>(T config)
        {
            var path = Path.Combine(OtherConfigsDirectoryPath, $"{typeof(T).Name}.json");

            File.WriteAllText(path, JsonConvert.SerializeObject(config, ArchiveMod.JsonSerializerSettings));
        }

        internal static object LoadFeatureConfig(string featureIdentifier, Type configType, out bool fileExists, bool saveIfNonExistent = true)
        {
            if (string.IsNullOrWhiteSpace(featureIdentifier)) throw new ArgumentException($"Parameter {nameof(featureIdentifier)} may not be null or whitespace.");
            if (configType == null) throw new ArgumentNullException($"Parameter {nameof(configType)} may not be null.");

            var path = Path.Combine(FeatureConfigsDirectoryPath, $"{featureIdentifier}_{configType.Name}.json");

            if (!File.Exists(path))
            {
                var newT = Activator.CreateInstance(configType);
                if (saveIfNonExistent)
                {
                    SaveFeatureConfig(featureIdentifier, configType, newT);
                }
                fileExists = false;
                return newT;
            }

            fileExists = true;
            return JsonConvert.DeserializeObject(File.ReadAllText(path), configType, ArchiveMod.JsonSerializerSettings);
        }

        internal static FeatureLocalizationData LoadFeatureLocalizationText(Feature feature, bool mainModule)
        {
            string dir = Path.Combine(Path.GetDirectoryName(mainModule ? ArchiveMod.CORE_PATH : feature.FeatureInternal.OriginAssembly.Location), $"Localization");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = Path.Combine(dir, $"{feature.Identifier}_Localization.json");
            if (!File.Exists(path))
            {
                var newData = FeatureInternal.GenerateLocalization(feature);
                File.WriteAllText(path, JsonConvert.SerializeObject(newData, ArchiveMod.JsonSerializerSettings));
                return newData;
            }
            var data = JsonConvert.DeserializeObject<FeatureLocalizationData>(File.ReadAllText(path), ArchiveMod.JsonSerializerSettings);
            var json = JsonConvert.SerializeObject(data, ArchiveMod.JsonSerializerSettings);
            var rdata = FeatureInternal.GenerateLocalization(feature, data);
            var rjson = JsonConvert.SerializeObject(rdata, ArchiveMod.JsonSerializerSettings);
            if (rjson.ComputeSHA256() != json.ComputeSHA256())
                File.WriteAllText(path, rjson);
            return data;
        }

        internal static object LoadFeatureConfig(string featureIdentifier, Type configType, bool saveIfNonExistent = true)
        {
            return LoadFeatureConfig(featureIdentifier, configType, out _, saveIfNonExistent);
        }

        internal static void SaveFeatureConfig(string featureIdentifier, Type configType, object configInstance)
        {
            if (string.IsNullOrWhiteSpace(featureIdentifier)) throw new ArgumentException($"Parameter {nameof(featureIdentifier)} may not be null or whitespace.");
            if (configType == null) throw new ArgumentNullException($"Parameter {nameof(configType)} may not be null.");
            if (configInstance == null) throw new ArgumentNullException($"Parameter {nameof(configInstance)} may not be null.");

            var path = Path.Combine(FeatureConfigsDirectoryPath, $"{featureIdentifier}_{configType.Name}.json");

            ArchiveLogger.Debug($"Saving Feature Setting to: {path}");
            File.WriteAllText(path, JsonConvert.SerializeObject(configInstance, ArchiveMod.JsonSerializerSettings));
        }
    }
}
