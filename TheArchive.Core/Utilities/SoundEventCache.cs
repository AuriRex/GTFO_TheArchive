using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace TheArchive.Utilities;

public class SoundEventCache : IInitAfterGameDataInitialized
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static bool IsReady { get; private set; }

    private static readonly Dictionary<string, uint> _soundIdCache = new();
    private static readonly Dictionary<uint, string> _reverseSoundIdCache = new();

    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateLoggerInstance(nameof(SoundEventCache), ConsoleColor.DarkGreen);

    public static bool TryResolve(string soundEvent, out uint soundId)
    {
        soundId = Resolve(soundEvent);
        return soundId != 0;
    }

    public static uint Resolve(string soundEvent, bool throwIfNotFound = false)
    {
        if(!IsReady)
        {
            Logger.Error($"{nameof(SoundEventCache)} isn't ready yet! Try resolving sound events a little later (after GameDataInit for example)!");
            return 0;
        }

        if(_soundIdCache.TryGetValue(soundEvent, out var value))
        {
            return value;
        }

        var msg = $"Sound event \"{soundEvent}\" could not be resolved!";
        if (throwIfNotFound)
            throw new SoundEventNotFoundException(msg);
        
        Logger.Error(msg);
        return 0;
    }

    public static bool TryReverseResolve(uint id, out string eventName)
    {
        eventName = ReverseResolve(id);
        return !string.IsNullOrEmpty(eventName);
    }

    public static string ReverseResolve(uint soundId, bool throwIfNotFound = false)
    {
        if (!IsReady)
        {
            Logger.Error($"{nameof(SoundEventCache)} isn't ready yet! Try resolving sound events a little later (after GameDataInit for example)!");
            return null;
        }

        if (_reverseSoundIdCache.TryGetValue(soundId, out var value))
        {
            return value;
        }

        var msg = $"Sound id \"{soundId}\" could not be resolved!";
        if (throwIfNotFound)
            throw new SoundEventNotFoundException(msg);
        
        Logger.Error(msg);
        return null;
    }

    public void Init()
    {
        try
        {
            Logger.Msg(ConsoleColor.Magenta, $"Initializing ...");

            var fieldOrProps = typeof(AK.EVENTS)
#if IL2CPP
                .GetProperties().Where(p => p.GetMethod?.ReturnType == typeof(uint));
#else
                    .GetFields().Where(f => f.FieldType == typeof(uint));
#endif

            foreach (var fp in fieldOrProps)
            {
                var name = fp.Name;
                var value = (uint)fp.GetValue(null)!;
                _soundIdCache.Add(name, value);
                _reverseSoundIdCache.Add(value, name);
            }

            Logger.Msg(ConsoleColor.Magenta, $"Cached {_soundIdCache.Count} sound events!");
        }
        catch(Exception ex)
        {
            Logger.Error($"Threw an exception on {nameof(Init)}:");
            Logger.Exception(ex);
        }

        IsReady = true;
    }

    public class SoundEventNotFoundException : Exception
    {
        public SoundEventNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Prints all cached sound events to the supplied <paramref name="logger"/>
    /// </summary>
    /// <param name="logger"></param>
    public static void DebugLog(IArchiveLogger logger)
    {
        logger.Notice($"Logging all cached sound events! ({_soundIdCache.Count})");
        foreach(var entry in _soundIdCache.Keys)
        {
            logger.Info(entry);
        }
        logger.Notice($"Done logging all cached sound events! ({_soundIdCache.Count})");
    }
}