using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Core.Bootstrap;

public static class TypeLoader
{
    static TypeLoader()
    {
        CecilResolver.ResolveFailure += CecilResolveOnFailure;
    }

    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= Loader.LoaderWrapper.CreateLoggerInstance(nameof(TypeLoader), ConsoleColor.White);

    public static AssemblyDefinition CecilResolveOnFailure(object sender, AssemblyNameReference reference)
    {
        if (!Utils.TryParseAssemblyName(reference.FullName, out var name))
        {
            return null;
        }
        foreach (string dir in new string[]
        {
#if R_BIE
            BepInEx.Paths.BepInExAssemblyDirectory,
            BepInEx.Paths.PluginPath,
            BepInEx.Paths.PatcherPluginPath,
            BepInEx.Paths.ManagedPath
#endif
        }.Concat(SearchDirectories))
        {
            if (!Directory.Exists(dir))
            {
                Logger.Debug($"Unable to resolve cecil search directory '{dir}'");
            }
            else if (Utils.TryResolveDllAssembly(name, dir, ReaderParameters, out var assembly))
            {
                return assembly;
            }
        }
        AssemblyResolveEventHandler assemblyResolve = AssemblyResolve;
        if (assemblyResolve == null)
        {
            return null;
        }
        return assemblyResolve(sender, reference);
    }

    public static event AssemblyResolveEventHandler AssemblyResolve;

    public static Dictionary<string, List<T>> FindModuleTypes<T>(string directory, Func<TypeDefinition, string, T> typeSelector, Func<AssemblyDefinition, bool> assemblyFilter = null, string cacheName = null) where T : ICacheable, new()
    {
        Dictionary<string, List<T>> result = new Dictionary<string, List<T>>();
        Dictionary<string, string> hashes = new Dictionary<string, string>();
        Dictionary<string, CachedAssembly<T>> cache = null;
        if (cacheName != null)
        {
            cache = LoadAssemblyCache<T>(cacheName);
        }
        string[] files = Directory.GetFiles(Path.GetFullPath(directory), "*.dll", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string dll = files[i];
            try
            {
                using (MemoryStream dllMs = new MemoryStream(File.ReadAllBytes(dll)))
                {
                    string hash = Utils.HashStream(dllMs);
                    hashes[dll] = hash;
                    dllMs.Position = 0L;
                    if (cache != null && cache.TryGetValue(dll, out var cacheEntry) && hash == cacheEntry.Hash)
                    {
                        result[dll] = cacheEntry.CacheItems;
                    }
                    else
                    {
                        using (AssemblyDefinition ass = AssemblyDefinition.ReadAssembly(dllMs, ReaderParameters))
                        {
                            if (assemblyFilter != null && !assemblyFilter(ass))
                            {
                                result[dll] = new List<T>();
                            }
                            else
                            {
                                List<T> matches = (from t in ass.MainModule.Types
                                                   select typeSelector(t, dll) into t
                                                   where t != null
                                                   select t).ToList();
                                result[dll] = matches;
                            }
                        }
                    }
                }
            }
            catch (BadImageFormatException e1)
            {
                Logger.Error($"Skipping loading {dll} because it's not a valid .NET assembly. Full error:");
                Logger.Exception(e1);
            }
            catch (Exception e2)
            {
                Logger.Exception(e2);
            }
        }
        if (cacheName != null)
        {
            SaveAssemblyCache(cacheName, result, hashes);
        }
        return result;
    }

    public static Dictionary<string, CachedAssembly<T>> LoadAssemblyCache<T>(string cacheName) where T : ICacheable, new()
    {
        if (!EnableAssemblyCache)
        {
            return null;
        }
        Dictionary<string, CachedAssembly<T>> result = new Dictionary<string, CachedAssembly<T>>();
        try
        {
#if R_BIE
            string path = Path.Combine(BepInEx.Paths.CachePath, cacheName + "_typeloader.dat");
#endif
            if (!File.Exists(path))
            {
                return null;
            }
            using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
            {
                int entriesCount = br.ReadInt32();
                for (int i = 0; i < entriesCount; i++)
                {
                    string entryIdentifier = br.ReadString();
                    string hash = br.ReadString();
                    int itemsCount = br.ReadInt32();
                    List<T> items = new List<T>();
                    for (int j = 0; j < itemsCount; j++)
                    {
                        T entry = new T();
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
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to load cache \"{cacheName}\": skipping loading cache. Reason:");
            Logger.Exception(e);
        }
        return result;
    }

    public static void SaveAssemblyCache<T>(string cacheName, Dictionary<string, List<T>> entries, Dictionary<string, string> hashes) where T : ICacheable
    {
        if (!EnableAssemblyCache)
        {
            return;
        }
        try
        {
#if R_BIE
            if (!Directory.Exists(BepInEx.Paths.CachePath))
            {
                Directory.CreateDirectory(BepInEx.Paths.CachePath);
            }
            using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(Path.Combine(BepInEx.Paths.CachePath, cacheName + "_typeloader.dat"))))
            {
                bw.Write(entries.Count);
                foreach (KeyValuePair<string, List<T>> kv in entries)
                {
                    bw.Write(kv.Key);
                    string hash;
                    bw.Write(hashes.TryGetValue(kv.Key, out hash) ? hash : "");
                    bw.Write(kv.Value.Count);
                    foreach (T item in kv.Value)
                    {
                        item.Save(bw);
                    }
                }
            }
#endif
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to save cache \"{cacheName}\"; skipping saving cache. Reason:");
            Logger.Exception(e);
        }
    }

    public static string TypeLoadExceptionToString(ReflectionTypeLoadException ex)
    {
        StringBuilder sb = new StringBuilder();
        foreach (Exception exSub in ex.LoaderExceptions)
        {
            sb.AppendLine(exSub.Message);
            FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
            if (exFileNotFound != null)
            {
                if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                {
                    sb.AppendLine("Fusion Log:");
                    sb.AppendLine(exFileNotFound.FusionLog);
                }
            }
            else
            {
                FileLoadException exLoad = exSub as FileLoadException;
                if (exLoad != null && !string.IsNullOrEmpty(exLoad.FusionLog))
                {
                    sb.AppendLine("Fusion Log:");
                    sb.AppendLine(exLoad.FusionLog);
                }
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public static readonly DefaultAssemblyResolver CecilResolver = new DefaultAssemblyResolver();

    public static readonly ReaderParameters ReaderParameters = new ReaderParameters
    {
        AssemblyResolver = CecilResolver
    };

    public static HashSet<string> SearchDirectories = new HashSet<string>();

    private static readonly bool EnableAssemblyCache = true;
}
