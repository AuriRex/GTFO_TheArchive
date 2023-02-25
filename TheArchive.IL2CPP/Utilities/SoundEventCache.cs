using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace TheArchive.Utilities
{
    public class SoundEventCache : IInitAfterGameDataInitialized
    {
        public static bool IsReady { get; private set; } = false;

        private static Dictionary<string, uint> SoundIdCache { get; set; } = new Dictionary<string, uint>();

        private static IArchiveLogger _logger;
        private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateLoggerInstance(nameof(SoundEventCache), ConsoleColor.DarkGreen);

        public static bool TryResolve(string soundEvent, out uint soundId)
        {
            soundId = Resolve(soundEvent);
            return soundId != 0;
        }

        public static uint Resolve(string soundId, bool throwIfNotFound = false)
        {
            if(!IsReady)
            {
                Logger.Error($"{nameof(SoundEventCache)} isn't ready yet! Try resolving sound events a little later (after GameDataInit for example)!");
                return 0;
            }

            if(SoundIdCache.TryGetValue(soundId, out var value))
            {
                return value;
            }

            Logger.Error($"Could not resolve sound id \"{soundId}\"!");

            if(throwIfNotFound)
            {
                throw new ArgumentException($"SoundID \"{soundId}\" could not be found!");
            }

            return 0;
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
                    uint value = (uint)fp.GetValue(null);
                    SoundIdCache.Add(name, value);
                }

                Logger.Msg(ConsoleColor.Magenta, $"Cached {SoundIdCache.Count} sound events!");
            }
            catch(Exception ex)
            {
                Logger.Error($"Threw an exception on {nameof(Init)}:");
                Logger.Exception(ex);
            }

            IsReady = true;
        }

        /// <summary>
        /// Prints all cached sound events to the supplied <paramref name="logger"/>
        /// </summary>
        /// <param name="logger"></param>
        public static void DebugLog(IArchiveLogger logger)
        {
            logger.Notice($"Logging all cached sound events! ({SoundIdCache.Count})");
            foreach(var entry in SoundIdCache.Keys)
            {
                logger.Info(entry);
            }
            logger.Notice($"Done logging all cached sound events! ({SoundIdCache.Count})");
        }
    }
}
