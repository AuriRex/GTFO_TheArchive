using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheArchive.Utilities;

namespace TheArchive.Core.Managers
{
    public class DataBlockManager
    {
        public static bool HasBeenSetup { get; private set; } = false;

        private static List<Type> _dataBlockTypes;
        public static List<Type> DataBlockTypes
        {
            get
            {
                if (_dataBlockTypes == null)
                    GetAllDataBlockTypes();
                return _dataBlockTypes;
            }
        }

        private static void GetAllDataBlockTypes()
        {
            Type[] gameTypes;

            try
            {
                gameTypes = Assembly.GetAssembly(ImplementationManager.GameTypeByIdentifier("EnemyDataBlock")).GetTypes();
            }
            catch(ReflectionTypeLoadException rtle)
            {
                gameTypes = rtle.Types;
            }

            var AllTypesOfGameDataBlockBase = gameTypes.Where(x => x != null
                && !x.IsAbstract
                && !x.IsInterface
                && x.BaseType != null
                && x.BaseType.IsGenericType
                && x.BaseType.GetGenericTypeDefinition() == ImplementationManager.GameTypeByIdentifier("GameDataBlockBase<>"));

            _dataBlockTypes = AllTypesOfGameDataBlockBase.ToList();
        }

        private static Dictionary<Type, List<Action<object>>> _transformationDictionary = new Dictionary<Type, List<Action<object>>>();

        public static void RegisterTransformationFor<T>(Action<object> func) where T : class
        {
            if (!_transformationDictionary.TryGetValue(typeof(T), out var list))
            {
                list = new List<Action<object>>();

                _transformationDictionary.Add(typeof(T), list);
            }

            list.Add(func);
        }

        public static object GetWrapper(Type type, out Type wrapperType)
        {
            var genericType = ImplementationManager.GameTypeByIdentifier("GameDataBlockBase<>").MakeGenericType(type);
            wrapperType = ImplementationManager.GameTypeByIdentifier("GameDataBlockWrapper<>").MakeGenericType(type);
            var wrapper = genericType.GetProperty("Wrapper", Utils.AnyBindingFlagss)?.GetValue(null) ?? genericType.GetField("Wrapper", Utils.AnyBindingFlagss).GetValue(null);

            return wrapper;
        }

        public static object GetAllBlocksFromWrapper(Type wrapperType, object wrapper)
        {
            return wrapperType.GetProperty("Blocks").GetValue(wrapper);
        }

        public static void Setup()
        {
            ArchiveLogger.Msg(ConsoleColor.Green, $"{nameof(DataBlockManager)} is setting up ...");
            try
            {
                ArchiveLogger.Msg(ConsoleColor.Green, $"[{nameof(DataBlockManager)}] Dumping built in DataBlocks ...");
                foreach (var type in DataBlockTypes)
                {
                    ArchiveLogger.Msg(ConsoleColor.DarkGreen, $"> {type.FullName}");

                    var path = Path.Combine(LocalFiles.DataBlockDumpPath, type.Name + ".json");

                    

                    var genericType = ImplementationManager.GameTypeByIdentifier("GameDataBlockBase<>").MakeGenericType(type);

                    string fileContents = (string) genericType.GetMethod("GetFileContents").Invoke(null, new object[0]);
                    
                    if (string.IsNullOrWhiteSpace(fileContents))
                    {
                        ArchiveLogger.Warning($"  X returned string is empty!!");
                    }

                    if (ArchiveMod.Settings.DumpDataBlocks && (ArchiveMod.Settings.AlwaysOverrideDataBlocks || !File.Exists(path)))
                    {
                        ArchiveLogger.Msg(ConsoleColor.DarkYellow, $"  > Writing to file: {path}");

                        File.WriteAllText(path, fileContents);
                    }
                }
                if(_transformationDictionary.Count > 0)
                {
                    ArchiveLogger.Msg(ConsoleColor.Green, $"[{nameof(DataBlockManager)}] Applying DataBlock transformations ...");
                    foreach (var kvp in _transformationDictionary)
                    {
                        foreach (var func in kvp.Value)
                        {
                            try
                            {
                                var wrapper = GetWrapper(kvp.Key, out var wrapperType);
                                var allBlocks = GetAllBlocksFromWrapper(wrapperType, wrapper);
                                
                                func?.Invoke(allBlocks);
                            }
                            catch(Exception ex)
                            {
                                ArchiveLogger.Exception(ex);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
            HasBeenSetup = true;
        }

    }
}
