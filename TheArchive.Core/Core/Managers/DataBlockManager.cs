using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Core.Managers
{
    public class DataBlockManager
    {
        public static bool HasBeenSetup { get; private set; } = false;

        private static IArchiveLogger _logger;
        private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateArSubLoggerInstance(nameof(DataBlockManager), ConsoleColor.Green);

        private static HashSet<Type> _dataBlockTypes;
        public static HashSet<Type> DataBlockTypes
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

            _dataBlockTypes = AllTypesOfGameDataBlockBase.ToHashSet();
        }


        private static List<ITransformationData> _transformationDataToApply = new List<ITransformationData>();

        public static void RegisterTransformationFor<T>(Action<List<T>> action, int priority = 0) where T : class
        {
            if (HasBeenSetup)
                throw new Exception("Transformations have to be registered before DataBlocks have been inited!");

            var frame = new System.Diagnostics.StackTrace().GetFrame(1);

            var trans = new TransformationData<T>(action, priority, originMethod: frame.GetMethod());

            _transformationDataToApply.Add(trans);

            Logger.Debug($"Transform for {typeof(T).Name} Registered: '{trans.DeclaringType?.FullName ?? "Null"}', Method: '{trans.OriginMethod?.Name ?? "Null"}' (Asm:{trans.DeclaringAssembly?.GetName()?.Name ?? "Null"}) [Priority:{trans.Priority}]");
        }

        private interface ITransformationData
        {
            public int Priority { get; }
            public Type DBType { get; }
            internal void Invoke(IList list);

            public MethodBase OriginMethod { get; }
            public Type DeclaringType { get; }
            public Assembly DeclaringAssembly { get; }
        }

        private class TransformationData<T> : ITransformationData
        {
            public int Priority { get; private set; } = 0;
            public Type DBType => typeof(T);

            public MethodBase OriginMethod { get; private set; }
            public Type DeclaringType => OriginMethod.DeclaringType;
            public Assembly DeclaringAssembly => DeclaringType.Assembly;

            private readonly MethodInfo _method;
            private readonly object _target;

            public TransformationData(Action<List<T>> transform, int priority = 0, MethodBase originMethod = null)
            {
                _method = transform.Method;
                _target = transform.Target;
                OriginMethod = originMethod;
                Priority = priority;
            }

            public void Invoke(IList list)
            {
                _method.Invoke(_target, new object[] { list });
            }
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

        public static List<T> GetAllBlocks<T>()
        {
            return (List<T>) GetAllBlocks(typeof(T));
        }

        public static IList GetAllBlocks(Type dataBlockType)
        {
            if (!IsDataBlockType(dataBlockType))
                throw new InvalidOperationException($"The Type \"{dataBlockType?.FullName}\" is not a valid DataBlock Type!");

            var wrapper = GetWrapper(dataBlockType, out var wrapperType);
            var allBlocks = GetAllBlocksFromWrapper(wrapperType, wrapper);

            return Utils.ToSystemListSlow(allBlocks, dataBlockType);
        }

        public static bool IsDataBlockType(Type type)
        {
            return DataBlockTypes.Contains(type);
        }

        internal static void Setup()
        {
            Logger.Msg(ConsoleColor.Green, $"Setting up ...");
            try
            {
                if (ArchiveMod.Settings.DumpDataBlocks && !ArchiveMod.IsPlayingModded)
                {
                    DumpOriginalDataBlocks(LocalFiles.DataBlockDumpPath, ArchiveMod.Settings.AlwaysOverrideDataBlocks);
                }

                if(_transformationDataToApply.Count > 0)
                {
                    Logger.Msg(ConsoleColor.Green, $"Applying DataBlock transformations ...");

                    var orderedTransformations = _transformationDataToApply.OrderBy(x => x.Priority);

                    foreach (var trans in orderedTransformations)
                    {
                        try
                        {
                            Logger.Notice($"> Applying transform for {trans.DBType.Name} from '{trans.DeclaringType?.FullName ?? "Null"}', Method: '{trans.OriginMethod?.Name ?? "Null"}' (Asm:{trans.DeclaringAssembly?.GetName()?.Name ?? "Null"}) [Priority:{trans.Priority}]");

                            trans.Invoke(GetAllBlocks(trans.DBType));
                        }
                        catch (Exception ex)
                        {
                            Logger.Exception(ex);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
            HasBeenSetup = true;
        }

        public static void DumpOriginalDataBlocks(string pathToDumpTo, bool overrideExistingFiles)
        {
            if (string.IsNullOrWhiteSpace(pathToDumpTo) || !Directory.Exists(pathToDumpTo))
                throw new ArgumentException($"Parameter \"{nameof(pathToDumpTo)}\" may not be null or whitespace and has to be a valid directory path!");

            Logger.Msg(ConsoleColor.Green, $"Dumping original DataBlocks into \"{pathToDumpTo}\"");

            foreach (var type in DataBlockTypes)
            {
                Logger.Msg(ConsoleColor.DarkGreen, $"> {type.FullName}");

                var path = Path.Combine(pathToDumpTo, type.Name + ".json");

                var genericType = ImplementationManager.GameTypeByIdentifier("GameDataBlockBase<>").MakeGenericType(type);

                string fileContents = (string)genericType.GetMethod("GetFileContents").Invoke(null, Array.Empty<object>());

                if (string.IsNullOrWhiteSpace(fileContents))
                {
                    Logger.Warning($"  X This DataBlock does not have any content!");
                }

                if (overrideExistingFiles || !File.Exists(path))
                {
                    Logger.Msg(ConsoleColor.DarkYellow, $"  > Writing to file: {path}");

                    File.WriteAllText(path, fileContents);
                }
            }
        }
    }
}
