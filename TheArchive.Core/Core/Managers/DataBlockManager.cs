using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;

namespace TheArchive.Core.Managers;

/// <summary>
/// Interact with the games datablocks indirectly maybe I suppose.
/// </summary>
public static partial class DataBlockManager
{
    /// <summary>
    /// Has it been set up?
    /// </summary>
    public static bool HasBeenSetup { get; private set; }

    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateArSubLoggerInstance(nameof(DataBlockManager), ConsoleColor.Green);

    private static HashSet<Type> _dataBlockTypes;
    
    /// <summary>
    /// Set of all existing datablock types.
    /// </summary>
    public static HashSet<Type> DataBlockTypes
    {
        get
        {
            if (_dataBlockTypes == null)
                GetAllDataBlockTypes();
            return _dataBlockTypes;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void GetAllDataBlockTypes()
    {
        Type[] gameTypes;

        try
        {
            gameTypes = Assembly.GetAssembly(ImplementationManager.GameTypeByIdentifier("EnemyDataBlock"))!.GetTypes();
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


    private static readonly List<ITransformationData> _transformationDataToApply = new();

    /// <summary>
    /// Registers a datablock transformation function.<br/>
    /// This allows you to change values on individual blocks.
    /// </summary>
    /// <param name="action">The transformation function.</param>
    /// <param name="priority">The priority.</param>
    /// <typeparam name="T">The datablock type to act on.</typeparam>
    /// <exception cref="Exception">Transformation functions have to be registered before the datablocks have been registered!</exception>
    /// <remarks>
    /// Transformation functions have to be registered before the datablocks have been registered!
    /// </remarks>
    public static void RegisterTransformationFor<T>(Action<List<T>> action, int priority = 0) where T : class
    {
        if (HasBeenSetup)
            throw new Exception("Transformations have to be registered before DataBlocks have been initialized!");

        var frame = new StackTrace().GetFrame(1);

        var trans = new TransformationData<T>(action, priority, originMethod: frame!.GetMethod());

        _transformationDataToApply.Add(trans);

        Logger.Debug($"Transform for {typeof(T).Name} Registered: '{trans.DeclaringType?.FullName ?? "Null"}', Method: '{trans.OriginMethod?.Name ?? "Null"}' (Asm:{trans.DeclaringAssembly?.GetName()?.Name ?? "Null"}) [Priority:{trans.Priority}]");
    }

    private static object GetWrapper(Type type, out Type wrapperType)
    {
        var genericType = ImplementationManager.GameTypeByIdentifier("GameDataBlockBase<>").MakeGenericType(type);
        wrapperType = ImplementationManager.GameTypeByIdentifier("GameDataBlockWrapper<>").MakeGenericType(type);
        var wrapper = genericType.GetProperty("Wrapper", Utils.AnyBindingFlagss)?.GetValue(null) ?? genericType.GetField("Wrapper", Utils.AnyBindingFlagss)!.GetValue(null);

        return wrapper;
    }

    private static object GetAllBlocksFromWrapper(Type wrapperType, object wrapper)
    {
        return wrapperType.GetProperty("Blocks")!.GetValue(wrapper);
    }

    /// <summary>
    /// Get all datablocks of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The datablock type to get all blocks for.</typeparam>
    /// <returns>A list containing all existing blocks of type <typeparamref name="T"/>.</returns>
    public static List<T> GetAllBlocks<T>()
    {
        return (List<T>) GetAllBlocks(typeof(T));
    }

    /// <summary>
    /// Get all datablocks of type <paramref name="dataBlockType"/>.
    /// </summary>
    /// <param name="dataBlockType">The datablock type to get all blocks for.</param>
    /// <returns>A list containing all existing blocks of type <paramref name="dataBlockType"/>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IList GetAllBlocks(Type dataBlockType)
    {
        if (!IsDataBlockType(dataBlockType))
            throw new InvalidOperationException($"The Type \"{dataBlockType?.FullName}\" is not a valid DataBlock Type!");

        var wrapper = GetWrapper(dataBlockType, out var wrapperType);
        var allBlocks = GetAllBlocksFromWrapper(wrapperType, wrapper);

        return Utils.ToSystemListSlow(allBlocks, dataBlockType);
    }

    /// <summary>
    /// Checks if the given type <paramref name="type"/> is a valid datablock type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>True</c> if the type is a datablock type.</returns>
    public static bool IsDataBlockType(Type type)
    {
        return DataBlockTypes.Contains(type);
    }

    internal static void Setup()
    {
        Logger.Debug("Setting up ...");
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

    /// <summary>
    /// Dumps all <b>vanilla</b> datablock values to disk.
    /// </summary>
    /// <param name="pathToDumpTo">Folder path to dump files into.</param>
    /// <param name="overrideExistingFiles">Override files if they already exist in the destination folder.</param>
    /// <exception cref="ArgumentException">If the folder path is invalid or doesn't exist</exception>
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

            var fileContents = (string)genericType.GetMethod("GetFileContents")!.Invoke(null, Array.Empty<object>());

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