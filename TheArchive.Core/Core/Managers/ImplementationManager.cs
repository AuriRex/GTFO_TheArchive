using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core.Managers;

/// <summary>
/// A way to interact with types that have had their name and or namespace changed<br/>
/// to keep the code compatible across multiple versions (see boosters R5 vs R6 for example)<br/>
/// or types that reference newer types that don't exist in previous versions (see Localization in R6)
/// </summary>
public class ImplementationManager
{
    private static Dictionary<string, Type> _gameTypeDictionary = new Dictionary<string, Type>();
    private static Dictionary<Type, object> _implementationInstances = new Dictionary<Type, object>();

    /// <summary>
    /// Register multiple (game) types
    /// </summary>
    /// <param name="dict"></param>
    public static void RegisterGameTypes(Dictionary<string, Type> dict)
    {
        foreach(var kvp in dict)
        {
            RegisterGameType(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Register a (game) type <paramref name="type"/> using an <paramref name="identifier"/><br/>Can be retrieved using <see cref="GameTypeByIdentifier"/>
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="type"></param>
    public static void RegisterGameType(string identifier, Type type)
    {
        if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentException("Identifier must not be null or whitespace!");
        if (type == null) throw new ArgumentException("Type must not be null!");

        if (_gameTypeDictionary.ContainsKey(identifier))
            throw new ArgumentException($"Duplicate identifier \"{identifier}\" used!");

        _gameTypeDictionary.Add(identifier, type);
        ArchiveLogger.Debug($"Registered: {identifier} --> {type.FullName}");
    }

    /// <summary>
    /// Get a (game) type using the specified <paramref name="identifier"/> that has been registered before using <see cref="RegisterGameType"/>
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public static Type GameTypeByIdentifier(string identifier)
    {
        if (_gameTypeDictionary.TryGetValue(identifier, out var type))
            return type;

        throw new ArgumentException($"No type found for identifier \"{identifier}\", has it not been registered yet?!");
    }

    /// <summary>
    /// Looks for a class implementing <typeparamref name="T"/> in the current domain and returns an instance of the first one it finds.<br/>
    /// Basically a factory resolver.<br/><br/>
    /// First time lookup might be slow as it's looping through every loaded type in the current domain.<br/>
    /// Subsequent lookups are returning a cached instance.
    /// </summary>
    /// <typeparam name="T">Implementation to lookup (cached)</typeparam>
    /// <returns>An instance of the first found type that implements <typeparamref name="T"/></returns>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static T GetOrFindImplementation<T>()
    {
        if(_implementationInstances.TryGetValue(typeof(T), out var val)) {
            return (T) val;
        }
        try
        {
            foreach (var module in ArchiveMod.Modules)
            {
                var asm = module.GetType().Assembly;
                foreach (Type type in asm.GetTypes())
                {
                    if (typeof(T).IsAssignableFrom(type)
                        && typeof(T) != type
                        && AnyRundownConstraintMatches(type)
                        && AnyBuildConstraintMatches(type))
                    {
                        ArchiveLogger.Debug($"Found implementation \"{type.FullName}\" for \"{typeof(T).FullName}\"!");
                        var instance = Activator.CreateInstance(type);
                        _implementationInstances.Add(typeof(T), instance);
                        return (T)instance;
                    }
                }
            }
        }
        catch(System.Reflection.ReflectionTypeLoadException tlex)
        {
            ArchiveLogger.Error($"{nameof(ReflectionTypeLoadException)} was thrown! This should not happen! Falling back to loaded types ... (Ignore stack trace if no {nameof(ArgumentException)} is thrown afterwards)");
            ArchiveLogger.Exception(tlex);

            var type = tlex.Types.FirstOrDefault(type =>
                typeof(T).IsAssignableFrom(type)
                && typeof(T) != type
                && AnyRundownConstraintMatches(type)
                && AnyBuildConstraintMatches(type));

            if (type != null)
            {
                ArchiveLogger.Debug($"Found implementation \"{type.FullName}\" for \"{typeof(T).FullName}\"!");
                var instance = Activator.CreateInstance(type);
                _implementationInstances.Add(typeof(T), instance);
                return (T)instance;
            }
        }

        throw new ArgumentException($"Could not find implementation for type \"{typeof(T).FullName}\"!");
    }

    /// <summary>
    /// Checks if a namespace.typename contains <paramref name="typeName"/> and returns it.
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="exactMatch"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static Type FindTypeInCurrentAppDomain(string typeName, bool exactMatch = false)
    {
        IEnumerable<Type> types = new List<Type>();
#if BepInEx
        try
        {
            foreach (var loadedPlugin in BepInEx.Unity.IL2CPP.IL2CPPChainloader.Instance.Plugins.Values)
            {
                var asm = loadedPlugin.Instance?.GetType().Assembly;
                if(asm != null)
                    foreach (var type in asm.GetTypes())
                    {
                        if (exactMatch)
                        {
                            if (type.FullName == typeName)
                                return type;
                        }
                        else if (type.FullName.Contains(typeName))
                            return type;
                    }
            }

        }
        catch (System.Reflection.ReflectionTypeLoadException rtle)
        {
            types = types.Concat(rtle.Types.Where(t => t != null));
        }
#endif
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (exactMatch)
                    {
                        if (type.FullName == typeName)
                            return type;
                    } 
                    else if (type.FullName.Contains(typeName))
                        return type;
                }
            }
        }
        catch(System.Reflection.ReflectionTypeLoadException rtle)
        {
            types = types.Concat(rtle.Types.Where(t => t != null));
        }

        if(exactMatch)
            return types.FirstOrDefault(t => t?.FullName == typeName);

        return types.FirstOrDefault(t => t?.FullName?.Contains(typeName) ?? false);
    }
}