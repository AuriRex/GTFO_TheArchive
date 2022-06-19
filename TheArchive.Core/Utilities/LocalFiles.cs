using MelonLoader;
using Newtonsoft.Json;
using System;
using System.IO;
using TheArchive.Core;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;

namespace TheArchive.Utilities
{
    public class LocalFiles
    {
        private static int _buildNumber = -1;
        public static int BuildNumber
        {
            get
            {
                if(_buildNumber == -1)
                {
                    try
                    {
                        //CellBuildData.GetRevision()
                        _buildNumber = (int) ((
                            ImplementationManager.FindTypeInCurrentAppDomain("CellBuildData")
                            ?.GetMethod("GetRevision", ArchivePatcher.AnyBindingFlags)
                            ?.Invoke(null, null)
                            ) ?? -1);

                        if (_buildNumber <= 0)
                        {
                            var buildNumFilePath = Path.Combine(MelonUtils.GameDirectory, "revision.txt");

                            if (!File.Exists(buildNumFilePath))
                            {
                                throw new Exception($"File doesn't exist: \"{buildNumFilePath}\"");
                            }

                            var buildStringRaw = File.ReadAllLines(buildNumFilePath)[0];
                            buildStringRaw = buildStringRaw.Replace(" ", ""); // remove the trailing space
                            _buildNumber = int.Parse(buildStringRaw);
                        }

                        if (_buildNumber <= 0)
                            throw new Exception("Build / Revision number couldn't be found ...");
                    }
                    catch(Exception ex)
                    {
                        _buildNumber = 0;
                        ArchiveLogger.Error($"Couldn't load the current build / revision number from revisions.txt! - {ex}: {ex.Message}\n{ex.StackTrace}");
                    }
                    
                }
                return _buildNumber;
            }
        }


        private static string _dataBlockDumpPath = null;
        public static string DataBlockDumpPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dataBlockDumpPath))
                {
                    var path = string.IsNullOrWhiteSpace(ArchiveMod.Settings.CustomFileSaveLocation) ? MelonUtils.UserDataDirectory : ArchiveMod.Settings.CustomFileSaveLocation;

                    _dataBlockDumpPath = Path.Combine(path, $"DataBlocks/{ArchiveMod.CurrentRundown}_Build_{BuildNumber}/");
                    if (!Directory.Exists(_dataBlockDumpPath))
                        Directory.CreateDirectory(_dataBlockDumpPath);
                }

                return _dataBlockDumpPath;
            }
        }

        private static string _savePath = null;
        /// <summary>
        /// Default is <c>"GTFO/UserData/"</c>, can be overridden by using <see cref="ArchiveSettings.CustomFileSaveLocation"/>
        /// </summary>
        public static string SaveDirectoryPath
        {
            get
            {
                if(string.IsNullOrEmpty(_savePath))
                {
                    _savePath = string.IsNullOrWhiteSpace(ArchiveMod.Settings.CustomFileSaveLocation) ? MelonUtils.UserDataDirectory : ArchiveMod.Settings.CustomFileSaveLocation;
                }
                return _savePath;
            }
        }

        private static string _rundownspecificSavePath = null;
        public static string RundownSpecificSaveDirectoryPath
        {
            get
            {
                if (ArchiveMod.CurrentRundown == Utils.RundownID.RundownUnitialized)
                    throw new InvalidOperationException("Only get this after GameData has been initialized!");

                if (string.IsNullOrEmpty(_rundownspecificSavePath))
                {
                    _rundownspecificSavePath = Path.Combine(SaveDirectoryPath, $"{ArchiveMod.CurrentRundown}_Data/");

                    if (!Directory.Exists(_rundownspecificSavePath))
                        Directory.CreateDirectory(_rundownspecificSavePath);
                }

                return _rundownspecificSavePath;
            }
        }

        private static string _otherConfigsPath = null;
        public static string OtherConfigsDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_otherConfigsPath))
                {
                    var path = string.IsNullOrWhiteSpace(ArchiveMod.Settings.CustomFileSaveLocation) ? MelonUtils.UserDataDirectory : ArchiveMod.Settings.CustomFileSaveLocation;
                    _otherConfigsPath = Path.Combine(path, "OtherConfigs/");

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
                    var path = string.IsNullOrWhiteSpace(ArchiveMod.Settings.CustomFileSaveLocation) ? MelonUtils.UserDataDirectory : ArchiveMod.Settings.CustomFileSaveLocation;
                    _featureConfigsPath = Path.Combine(path, "FeatureSettings/");

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
                    _filesPath = Path.Combine(RundownSpecificSaveDirectoryPath, "Files/");
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
                    _settingsPath = Path.Combine(RundownSpecificSaveDirectoryPath, $"GTFO_Settings.json");
                return _settingsPath;
            }
        }

        private static string _favoritesPath = null;
        public static string FavoritesPath
        {
            get
            {
                if (string.IsNullOrEmpty(_favoritesPath))
                    _favoritesPath = Path.Combine(RundownSpecificSaveDirectoryPath, $"GTFO_Favorites.json");
                return _favoritesPath;
            }
        }

        private static string _botFavoritesPath = null;
        public static string BotFavoritesPath
        {
            get
            {
                if (string.IsNullOrEmpty(_botFavoritesPath))
                    _botFavoritesPath = Path.Combine(RundownSpecificSaveDirectoryPath, $"GTFO_BotFavorites.json");
                return _botFavoritesPath;
            }
        }

        private static string _boostersPath = null;
        public static string BoostersPath
        {
            get
            {
                if (string.IsNullOrEmpty(_boostersPath))
                    _boostersPath = Path.Combine(RundownSpecificSaveDirectoryPath, $"Booster_Data.json");
                return _boostersPath;
            }
        }

        private static string _vanityItemsPath = null;
        public static string VanityItemsPath
        {
            get
            {
                if (string.IsNullOrEmpty(_vanityItemsPath))
                    _vanityItemsPath = Path.Combine(RundownSpecificSaveDirectoryPath, $"VanityItems_Data.json");
                return _vanityItemsPath;
            }
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

        private static string _localRundownProgressionJSON = string.Empty;
        public static string LocalRundownProgressionJSON { get => _localRundownProgressionJSON; }

        public const string kRundownProgressionFileName = "RundownProgression_primary.json";

        public static void LoadRundownFourAndUpLocalRundownProgressionIfNecessary()
        {
            if (string.IsNullOrEmpty(_localRundownProgressionJSON))
            {
                _localRundownProgressionJSON = LoadFromFilesDir(kRundownProgressionFileName);
            }
        }

        public static void SaveRundownFourAndUpLocalRundownProgression(string json)
        {
            SaveToFilesDir(kRundownProgressionFileName, json);
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

            File.WriteAllText(path, JsonConvert.SerializeObject(configInstance, ArchiveMod.JsonSerializerSettings));
        }
    }
}
