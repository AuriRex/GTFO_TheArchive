using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.QoL;

[RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
public class NoStoryDialog : Feature
{
    public override string Name => "Remove Story Dialog";

    public override FeatureGroup Group => FeatureGroups.QualityOfLife;

    public override string Description => "Removes all level-based voice events that come with subtitles.\naka Schaeffer-be-gone";

#if IL2CPP
    public static new IArchiveLogger FeatureLogger { get; set; }

    [ArchivePatch(typeof(WardenObjectiveManager), nameof(WardenObjectiveManager.CheckAndExecuteEventsOnTrigger), new Type[] { typeof(WardenObjectiveEventData), typeof(eWardenObjectiveEventTrigger), typeof(bool), typeof(float) })]
    internal static class WardenObjectiveManager_CheckAndExecuteEventsOnTrigger_Patch
    {
        [IsPrefix]
        [RundownConstraint(RundownFlags.RundownSeven, RundownFlags.Latest)]
        public static void PrefixR7(WardenObjectiveEventData eventToTrigger)
        {
            if(eventToTrigger.SoundSubtitle.HasValue)
            {
                FeatureLogger.Notice($"SoundEvent about to execute was: {eventToTrigger.SoundID} ({SoundEventCache.ReverseResolve(eventToTrigger.SoundID)})");
                eventToTrigger.SoundID = 0;
            }
        }

        [IsPrefix]
        [RundownConstraint(RundownFlags.RundownSix)]
        public static void PrefixR6(WardenObjectiveEventData eventToTrigger)
        {
            if (eventToTrigger.SoundID > 0)
            {
                FeatureLogger.Notice($"SoundEvent about to execute was: {eventToTrigger.SoundID} ({SoundEventCache.ReverseResolve(eventToTrigger.SoundID)})");
                eventToTrigger.SoundID = 0;
            }
        }
    }
#endif
}