using Player;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Dev;

/// <summary>
/// Manages filters for player voice lines.
/// </summary>
[EnableFeatureByDefault]
[HideInModSettings]
public class PlayerDialogFilter : Feature
{
    /// <inheritdoc />
    public override string Name => "Player Dialog Filter";

    /// <inheritdoc />
    public override FeatureGroup Group => FeatureGroups.Dev;

    /// <inheritdoc />
    public override string Description => "Remove unwanted player sound events.";


    private static readonly List<uint> _soundEventsToFilter = new List<uint>();

    /// <summary>
    /// Add a sound event to the filter list.
    /// </summary>
    /// <param name="soundEvent">The sound event to filter.</param>
    /// <returns><c>False</c> if the sound event does not exist.</returns>
    public static bool AddEventToFilter(string soundEvent)
    {
        if (!SoundEventCache.TryResolve(soundEvent, out var soundId))
            return false;

        if (_soundEventsToFilter.Contains(soundId))
            return true;

        _soundEventsToFilter.Add(soundId);
        return true;
    }

    /// <summary>
    /// Remove a sound event from the filter list.
    /// </summary>
    /// <param name="soundEvent">The sound event to remove from the filter list.</param>
    /// <returns><c>True</c> if the filter was successfully removed.</returns>
    public static bool RemoveEventFromFilter(string soundEvent)
    {
        if (!SoundEventCache.TryResolve(soundEvent, out var soundId))
            return false;
        
        return _soundEventsToFilter.Remove(soundId);
    }

    [ArchivePatch(typeof(PlayerVoiceManager), nameof(PlayerVoiceManager.DoSayLine))]
    internal static class PlayerVoiceManager__DoSayLine__Patch
    {
        public static bool Prefix(PlayerVoiceManager.pSayLine data)
        {
            if (_soundEventsToFilter.Contains(data.eventID))
                return ArchivePatch.SKIP_OG;

            return ArchivePatch.RUN_OG;
        }
    }
}