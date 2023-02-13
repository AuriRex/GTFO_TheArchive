using MelonLoader.Utils;
using System;
using System.IO;
using System.Reflection;

namespace TheArchive.Loader
{
    internal static class CoreModLoader
    {
        public static string BaseInstallPath { get; internal set; }

        public const string MOD_ASSEMBLY = "TheArchive.Core.dll";

        public static string GameRootDirectory
        {
            get
            {
#if MelonLoader
                return MelonEnvironment.GameRootDirectory;
#endif
#if BepInEx
                return BepInEx.Paths.GameRootPath;
#endif
#pragma warning disable CS0162 // Unreachable code detected
                return "./";
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        public static string ModsPath
        {
            get
            {
                string mods;
#if MelonLoader
                mods = "Mods/";
#endif
#if BepInEx
                mods = "BepInEx/plugins/";
#endif
                if (string.IsNullOrWhiteSpace(mods))
                    mods = "Mods/";

                if (!Directory.Exists(mods))
                    Directory.CreateDirectory(mods);

                return Path.Combine(GameRootDirectory, mods);
            }
        }

        private static string _modLocalLowPath = null;
        public static string ModLocalLowPath
        {
            get
            {
                if (_modLocalLowPath == null)
                {

                    _modLocalLowPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "GTFO_TheArchive");
                    if (!Directory.Exists(_modLocalLowPath))
                        Directory.CreateDirectory(_modLocalLowPath);
                }
                return _modLocalLowPath;
            }
        }


        private static string _modLocalLowAssemblyPath = null;
        public static string ModLocalLowAssemblyPath
        {
            get
            {
                if (_modLocalLowAssemblyPath == null)
                {
                    _modLocalLowAssemblyPath = Path.Combine(ModLocalLowPath, "Assemblies");
                    if (!Directory.Exists(_modLocalLowAssemblyPath))
                        Directory.CreateDirectory(_modLocalLowAssemblyPath);
                }
                return _modLocalLowAssemblyPath;
            }
        }

#if NET6_0
        public const string NET = "net6/";
#elif NETFRAMEWORK
        public const string NET = "net472/";
#else
        public const string NET = "OH_NO_THIS_SHOULDNT_HAPPEN/";
#endif

        private static Assembly _asm;

        public static void LoadMainModASM()
        {
            var gamePath = Path.Combine(ModsPath, MOD_ASSEMBLY);
            if (File.Exists(gamePath))
            {
                BaseInstallPath = ModsPath;
                Console.WriteLine(BaseInstallPath);
                _asm = Assembly.LoadFile(gamePath);
                return;
            }

            var localLowNetPath = Path.Combine(ModLocalLowAssemblyPath, NET);
            var localLowPath = Path.Combine(localLowNetPath, MOD_ASSEMBLY);
            if (File.Exists(localLowPath))
            {
                BaseInstallPath = localLowNetPath;
                Console.WriteLine(BaseInstallPath);
                _asm = Assembly.LoadFile(localLowPath);
                return;
            }

            Console.WriteLine($"[ERROR]!! Could not find Core mod dll at GamePath:\"{gamePath}\" or LocalLowPath:\"{localLowNetPath}\".");
            throw new FileNotFoundException($"Could not find Core mod dll at GamePath:\"{gamePath}\" or LocalLowPath:\"{localLowNetPath}\".");
        }

    }
}
