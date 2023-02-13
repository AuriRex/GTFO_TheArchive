using MelonLoader.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TheArchive.Loader.Wrapper")]
namespace TheArchive.Loader
{
    internal static class LoaderCore
    {
        public static string BaseInstallPath { get; internal set; }

        public const string MOD_ASSEMBLY = "TheArchive.Core.dll";
        public const string WRAPPER_ASSEMBLY = "TheArchive.Loader.Wrapper.dll";

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

        private static Assembly _archiveCoreAsm;
        private static Assembly _wrapperAsm;

        private static bool _isLoaded = false;

        public static Assembly LoadASMFromBasePath(string assemblyFileName)
        {
            var gamePath = Path.Combine(ModsPath, assemblyFileName);
            if (File.Exists(gamePath))
            {
                BaseInstallPath = ModsPath;
                Console.WriteLine(BaseInstallPath);
                return Assembly.LoadFile(gamePath);
            }

            var localLowNetPath = Path.Combine(ModLocalLowAssemblyPath, NET);
            var localLowPath = Path.Combine(localLowNetPath, assemblyFileName);
            if (File.Exists(localLowPath))
            {
                BaseInstallPath = localLowNetPath;
                Console.WriteLine(BaseInstallPath);
                return Assembly.LoadFile(localLowPath);
            }

            throw new FileNotFoundException($"Could not find Core mod dll at GamePath:\"{gamePath}\" or LocalLowPath:\"{localLowNetPath}\".");
        }

        public static void LoadMainModASM()
        {
            if (_isLoaded)
                return;
            _isLoaded = true;

            _archiveCoreAsm = LoadASMFromBasePath(MOD_ASSEMBLY);
        }

        public static void LoadWrapperASM()
        {
            _wrapperAsm = LoadASMFromBasePath(WRAPPER_ASSEMBLY);

            var loaderWrapperInterfaceType = _archiveCoreAsm.GetTypes().First(t => t.IsInterface && t.Name == nameof(Interfaces.Loader.ILoaderWrapper));
            var classInjectorInterfaceType = _archiveCoreAsm.GetTypes().First(t => t.IsInterface && t.Name == nameof(Interfaces.Loader.IClassInjector));

            var loaderWrapperType = _wrapperAsm.GetTypes().First(t => loaderWrapperInterfaceType.IsAssignableFrom(t));
            var classInjectorType = _wrapperAsm.GetTypes().First(t => classInjectorInterfaceType.IsAssignableFrom(t));

            var loaderWrapperCoreType = _archiveCoreAsm.GetTypes().First(t => t.FullName == "TheArchive.Loader.LoaderWrapper");

            //LoaderWrapper.LoaderWrapperInstance = (ILoaderWrapper) new loaderWrapperType;
            loaderWrapperCoreType.GetProperty(nameof(LoaderWrapper.LoaderWrapperInstance)).SetValue(null, Activator.CreateInstance(loaderWrapperType));

            //LoaderWrapper.ClassInjector.ClassInjectorInstance = (IClassInjector) new classInjectorType;
            loaderWrapperCoreType.GetNestedTypes().First().GetProperty(nameof(LoaderWrapper.ClassInjector.ClassInjectorInstance)).SetValue(null, Activator.CreateInstance(classInjectorType));
        }

    }
}
