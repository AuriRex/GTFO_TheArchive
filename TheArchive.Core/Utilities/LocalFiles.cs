using MelonLoader;
using System;
using System.IO;

namespace TheArchive.Utilities
{
    public class LocalFiles
    {
        /*
         * Global.RundownIdToLoad
         * 17 = RD#001
         * 19 = RD#002
         * 22 = RD#003
         * 25 = RD#004
         * 26 = RD#005
         */

        private static string _dataBlockDumpPath = null;
        public static string DataBlockDumpPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dataBlockDumpPath))
                {
                    _dataBlockDumpPath = Path.Combine(MelonLoader.MelonUtils.UserDataDirectory, $"DataBlocks_Rundown_{ArchiveMod.CurrentRundown}/");
                    if (!Directory.Exists(_dataBlockDumpPath))
                        Directory.CreateDirectory(_dataBlockDumpPath);
                }

                return _dataBlockDumpPath;
            }
        }

        private static string _savePath = null;
        public static string SaveDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_savePath))
                {
                    _savePath = Path.Combine(MelonLoader.MelonUtils.UserDataDirectory, $"{ArchiveMod.CurrentRundown}_Data/");
                    if (!Directory.Exists(_savePath))
                        Directory.CreateDirectory(_savePath);
                }

                return _savePath;
            }
        }

        private static string _filesPath = null;
        public static string FilesDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_filesPath))
                {
                    _filesPath = Path.Combine(SaveDirectoryPath, "Files/");
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
                    _settingsPath = Path.Combine(SaveDirectoryPath, $"GTFO_Settings.json");
                return _settingsPath;
            }
        }

        private static string _favoritesPath = null;
        public static string FavoritesPath
        {
            get
            {
                if (string.IsNullOrEmpty(_favoritesPath))
                    _favoritesPath = Path.Combine(SaveDirectoryPath, $"GTFO_Favorites.json");
                return _favoritesPath;
            }
        }

        private static string _boostersPath = null;
        public static string BoostersPath
        {
            get
            {
                if (string.IsNullOrEmpty(_boostersPath))
                    _boostersPath = Path.Combine(SaveDirectoryPath, $"Booster_Data.json");
                return _boostersPath;
            }
        }

        public static void SaveToFilesDir(string filename, string jsonOrSomething)
        {

            string path = Path.Combine(FilesDirectoryPath, filename);
            if (string.IsNullOrEmpty(jsonOrSomething))
            {
                MelonLogger.Msg(ConsoleColor.DarkRed, $"Saving \"{filename}\" to disk failed because the data is null or empty! Path: {path}");
                return;
            }
            MelonLogger.Msg(ConsoleColor.Blue, $"Saving \"{filename}\" to disk at: {path}");
            File.WriteAllText(path, jsonOrSomething);
        }

        public static string LoadFromFilesDir(string filename, bool isJson = true)
        {
            string path = Path.Combine(FilesDirectoryPath, filename);
            MelonLogger.Msg(ConsoleColor.Green, $"Loading \"{filename}\" from disk at: {path}");
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
    }
}
