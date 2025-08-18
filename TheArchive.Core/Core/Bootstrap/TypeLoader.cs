// This file is licensed under the LGPL 2.1 LICENSE
// See LICENSE_BepInEx in the projects root folder
// Original code from https://github.com/BepInEx/BepInEx

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Core.Bootstrap;

/// <summary>
///     Provides methods for loading specified types from an assembly.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class TypeLoader
{
    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(TypeLoader));
    
    /// <summary>
    ///     Default assembly resolved used by the <see cref="TypeLoader" />
    /// </summary>
    public static readonly DefaultAssemblyResolver CecilResolver;

    /// <summary>
    ///     Default reader parameters used by <see cref="TypeLoader" />
    /// </summary>
    public static readonly ReaderParameters ReaderParameters;

    /// <summary>
    /// Static directories to search through
    /// </summary>
    public static HashSet<string> SearchDirectories = new();

    private static readonly bool EnableAssemblyCache = true;
    
    static TypeLoader()
    {
        CecilResolver = new DefaultAssemblyResolver();
        ReaderParameters = new ReaderParameters { AssemblyResolver = CecilResolver };
        
        CecilResolver.ResolveFailure += CecilResolveOnFailure;
    }
    
    /// <summary>
    /// Cecil resolve on failure method
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="reference">Assembly reference to search for.</param>
    /// <returns></returns>
    public static AssemblyDefinition CecilResolveOnFailure(object sender, AssemblyNameReference reference)
    {
        if (!Utils.TryParseAssemblyName(reference.FullName, out var name))
            return null;

        var resolveDirs = new[]
        {
            BepInEx.Paths.BepInExAssemblyDirectory,
            BepInEx.Paths.PluginPath,
            BepInEx.Paths.PatcherPluginPath,
            BepInEx.Paths.ManagedPath
        }.Concat(SearchDirectories);
        
        foreach (var dir in resolveDirs)
        {
            if (!Directory.Exists(dir))
            {
                Logger.Debug($"Unable to resolve cecil search directory '{dir}'");
                continue;
            }

            if (Utils.TryResolveDllAssembly(name, dir, ReaderParameters, out var assembly))
                return assembly;
        }
        
        return AssemblyResolve?.Invoke(sender, reference);
    }

    /// <summary>
    ///     Event fired when <see cref="TypeLoader" /> fails to resolve a type during type loading.
    /// </summary>
    public static event AssemblyResolveEventHandler AssemblyResolve;

    /// <summary>
    ///     Looks up assemblies in the given directory and locates all types that can be loaded and collects their metadata.
    /// </summary>
    /// <typeparam name="T">The specific base type to search for.</typeparam>
    /// <param name="directory">The directory to search for assemblies.</param>
    /// <param name="typeSelector">A function to check if a type should be selected and to build the type metadata.</param>
    /// <param name="assemblyFilter">A filter function to quickly determine if the assembly can be loaded.</param>
    /// <param name="cacheName">The name of the cache to get cached types from.</param>
    /// <returns>
    ///     A dictionary of all assemblies in the directory and the list of type metadatas of types that match the
    ///     selector.
    /// </returns>
    public static Dictionary<string, List<T>> FindModuleTypes<T>(string directory,
                                                                 Func<TypeDefinition, string, T> typeSelector,
                                                                 Func<AssemblyDefinition, bool> assemblyFilter = null,
                                                                 string cacheName = null)
        where T : ICacheable, new()
    {
        var result = new Dictionary<string, List<T>>();
        var hashes = new Dictionary<string, string>();
        Dictionary<string, CachedAssembly<T>> cache = null;
        
        if (cacheName != null)
            cache = LoadAssemblyCache<T>(cacheName);
        
        foreach(var dll in Directory.GetFiles(Path.GetFullPath(directory), "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                using var dllMs = new MemoryStream(File.ReadAllBytes(dll));
                
                var hash = Utils.HashStream(dllMs);
                hashes[dll] = hash;
                dllMs.Position = 0L;
                if (cache != null && cache.TryGetValue(dll, out var cacheEntry) && hash == cacheEntry.Hash)
                {
                    result[dll] = cacheEntry.CacheItems;
                    continue;
                }

                using var ass = AssemblyDefinition.ReadAssembly(dllMs, ReaderParameters);
                
                if (!assemblyFilter?.Invoke(ass) ?? false)
                {
                    result[dll] = new List<T>();
                    continue;
                }

                var matches = ass.MainModule.Types
                                        .Select(t => typeSelector(t, dll))
                                        .Where(t => t != null).ToList();
                result[dll] = matches;
            }
            catch (BadImageFormatException e)
            {
                Logger.Debug($"Skipping loading {dll} because it's not a valid .NET assembly. Full error: {e.Message}");
            }
            catch (Exception e)
            {
                Logger.Exception(e);
            }
        }
        
        if (cacheName != null)
            SaveAssemblyCache(cacheName, result, hashes);
        
        return result;
    }

    /// <summary>
    ///     Loads an index of type metadatas from a cache.
    /// </summary>
    /// <param name="cacheName">Name of the cache</param>
    /// <typeparam name="T">Cacheable item</typeparam>
    /// <returns>
    ///     Cached type metadatas indexed by the path of the assembly that defines the type. If no cache is defined,
    ///     return null.
    /// </returns>
    public static Dictionary<string, CachedAssembly<T>> LoadAssemblyCache<T>(string cacheName) where T : ICacheable, new()
    {
        if (!EnableAssemblyCache)
            return null;
        
        var result = new Dictionary<string, CachedAssembly<T>>();
        try
        {
            var path = Path.Combine(BepInEx.Paths.CachePath, $"{cacheName}_typeloader.dat");
            if (!File.Exists(path))
                return null;

            using var br = new BinaryReader(File.OpenRead(path));
            
            var entriesCount = br.ReadInt32();
            
            for (var i = 0; i < entriesCount; i++)
            {
                var entryIdentifier = br.ReadString();
                var hash = br.ReadString();
                var itemsCount = br.ReadInt32();
                var items = new List<T>();
                
                for (var j = 0; j < itemsCount; j++)
                {
                    var entry = new T();
                    entry.Load(br);
                    items.Add(entry);
                }
                
                result[entryIdentifier] = new CachedAssembly<T>
                {
                    Hash = hash,
                    CacheItems = items
                };
            }
        }
        catch (Exception e)
        {
            Logger.Warning($"Failed to load cache \"{cacheName}\": skipping loading cache. Reason: {e.Message}");
        }
        
        return result;
    }

    /// <summary>
    ///     Saves indexed type metadata into a cache.
    /// </summary>
    /// <param name="cacheName">Name of the cache</param>
    /// <param name="entries">List of plugin metadatas indexed by the path to the assembly that contains the types</param>
    /// <param name="hashes">Hash values that can be used for checking similarity between cached and live assembly</param>
    /// <typeparam name="T">Cacheable item</typeparam>
    public static void SaveAssemblyCache<T>(string cacheName, Dictionary<string, List<T>> entries, Dictionary<string, string> hashes) where T : ICacheable
    {
        if (!EnableAssemblyCache)
            return;
        
        try
        {
            if (!Directory.Exists(BepInEx.Paths.CachePath))
                Directory.CreateDirectory(BepInEx.Paths.CachePath);

            using var bw = new BinaryWriter(File.OpenWrite(Path.Combine(BepInEx.Paths.CachePath, $"{cacheName}_typeloader.dat")));
            bw.Write(entries.Count);
            
            foreach (var kv in entries)
            {
                bw.Write(kv.Key);
                bw.Write(hashes.GetValueOrDefault(kv.Key, ""));
                bw.Write(kv.Value.Count);
                
                foreach (var item in kv.Value)
                    item.Save(bw);
            }
        }
        catch (Exception e)
        {
            Logger.Warning($"Failed to save cache \"{cacheName}\"; skipping saving cache. Reason: {e.Message}");
        }
    }

    /// <summary>
    ///     Converts TypeLoadException to a readable string.
    /// </summary>
    /// <param name="ex">TypeLoadException</param>
    /// <returns>Readable representation of the exception</returns>
    public static string TypeLoadExceptionToString(ReflectionTypeLoadException ex)
    {
        var sb = new StringBuilder();
        foreach (var exSub in ex.LoaderExceptions)
        {
            sb.AppendLine(exSub!.Message);
            if (exSub is FileNotFoundException exFileNotFound)
            {
                if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                {
                    sb.AppendLine("Fusion Log:");
                    sb.AppendLine(exFileNotFound.FusionLog);
                }
            }
            else if (exSub is FileLoadException exLoad)
            {
                if (!string.IsNullOrEmpty(exLoad.FusionLog))
                {
                    sb.AppendLine("Fusion Log:");
                    sb.AppendLine(exLoad.FusionLog);
                }
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
}
