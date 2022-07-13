using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Interfaces;

namespace TheArchive.Utilities
{
    public class SoundEventCache : IInitAfterGameDataInitialized
    {
        private static Dictionary<string, uint> SoundIdCache { get; set; } = new Dictionary<string, uint>();

        public static uint Resolve(string soundId, bool throwIfNotFound = false)
        {
            if(SoundIdCache.TryGetValue(soundId, out var value))
            {
                return value;
            }

            ArchiveLogger.Error($"[{nameof(SoundEventCache)}] Could not resolve sound id \"{soundId}\"!");

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

                ArchiveLogger.Msg(ConsoleColor.Magenta, $"[{nameof(SoundEventCache)}] Cached {SoundIdCache.Count} sound events!");
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"[{nameof(SoundEventCache)}] Threw an exception:");
                ArchiveLogger.Exception(ex);
            }
        }
    }
}
