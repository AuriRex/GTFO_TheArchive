using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace TheArchive.Utilities;

/// <summary>
/// Maps all sound event ids to their string representation and vice versa.
/// </summary>
public class SoundEventCache : IInitAfterGameDataInitialized
{
    /// <summary>
    /// If setup has executed.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static bool IsReady { get; private set; }

    private static readonly Dictionary<string, uint> _soundIdCache = new();
    private static readonly Dictionary<uint, string> _reverseSoundIdCache = new();

    private static IArchiveLogger _logger;
    private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateLoggerInstance(nameof(SoundEventCache), ConsoleColor.DarkGreen);

    /// <summary>
    /// Tries to resolve the sound id from a sound event string.
    /// </summary>
    /// <param name="soundEvent">The sound event to resolve.</param>
    /// <param name="soundId">The sound event id</param>
    /// <returns><c>True</c> if the event could be resolved.</returns>
    public static bool TryResolve(string soundEvent, out uint soundId)
    {
        soundId = Resolve(soundEvent);
        return soundId != 0;
    }

    /// <summary>
    /// Tries to resolve the sound id from a sound event string.
    /// </summary>
    /// <param name="soundEvent">The sound event to resolve.</param>
    /// <param name="throwIfNotFound">Should an exception be thrown if the event is not found?</param>
    /// <returns>The sound event id or zero.</returns>
    /// <exception cref="SoundEventNotFoundException">If the sound event wasn't found.</exception>
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

    /// <summary>
    /// Tries to resolve the sound event name from a sound id.
    /// </summary>
    /// <param name="id">The sound event id to resolve.</param>
    /// <param name="eventName">The resolved sound event name.</param>
    /// <returns><c>True</c> if the event was resolved.</returns>
    public static bool TryReverseResolve(uint id, out string eventName)
    {
        eventName = ReverseResolve(id);
        return !string.IsNullOrEmpty(eventName);
    }

    /// <summary>
    /// Tries to resolve the sound event name from a sound id.
    /// </summary>
    /// <param name="soundId">The sound id to resolve.</param>
    /// <param name="throwIfNotFound">Should an exception be thrown if the event is not found?</param>
    /// <returns>The resolved event name or null.</returns>
    /// <exception cref="SoundEventNotFoundException">If the sound event wasn't found.</exception>
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

    /// <summary>
    /// Initializes the sound event cache.<br/>
    /// Do not call.
    /// </summary>
    public void Init()
    {
        try
        {
            Logger.Debug("Initializing ...");

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

            Logger.Debug($"Cached {_soundIdCache.Count} sound events!");
        }
        catch(Exception ex)
        {
            Logger.Error($"Threw an exception on {nameof(Init)}:");
            Logger.Exception(ex);
        }

        IsReady = true;
    }

    /// <summary>
    /// Thrown whenever a sound event could not be found.
    /// </summary>
    public class SoundEventNotFoundException : Exception
    {
        /// <inheritdoc/>
        public SoundEventNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Prints all cached sound events to the supplied <paramref name="logger"/>.
    /// </summary>
    /// <param name="logger">The logger to print to.</param>
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