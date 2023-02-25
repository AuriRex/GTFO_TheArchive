using Player;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Special
{
    [EnableFeatureByDefault]
    [HideInModSettings]
    public class PlayerDialogFilter : Feature
    {
        public override string Name => "Player Dialog Filter";

        public override string Group => FeatureGroups.Special;

        public override string Description => "Remove unwanted player sound events.";
 

        private static readonly List<uint> _soundEventsToFilter = new List<uint>();

        public static bool AddEventToFilter(string soundEvent)
        {
            if (!SoundEventCache.TryResolve(soundEvent, out var soundId))
                return false;

            if (_soundEventsToFilter.Contains(soundId))
                return true;

            _soundEventsToFilter.Add(soundId);
            return true;
        }

        public static bool RemoveEventFromFilter(string soundEvent)
        {
            if (!SoundEventCache.TryResolve(soundEvent, out var soundId))
                return false;

            if (_soundEventsToFilter.Contains(soundId))
                return _soundEventsToFilter.Remove(soundId);

            return false;
        }

        [ArchivePatch(typeof(PlayerVoiceManager), nameof(PlayerVoiceManager.DoSayLine))]
        public static class PlayerVoiceManager_DoSayLine_Patch
        {
            public static bool Prefix(PlayerVoiceManager.pSayLine data)
            {
                if (_soundEventsToFilter.Contains(data.eventID))
                    return ArchivePatch.SKIP_OG;

                return ArchivePatch.RUN_OG;
            }
        }
    }
}
