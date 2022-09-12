using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.LocalProgression
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
    internal class CheckPointManagerPatch : Feature
    {
        public override string Name => "CheckPointManagerPatch";

        public new static IArchiveLogger FeatureLogger { get; set; }

        [ArchivePatch(nameof(CheckpointManager.StoreCheckpoint))]
        public class CheckpointManager_Patch
        {
            public static Type Type() => typeof(CheckpointManager);
            public static void Prefix()
            {
                FeatureLogger.Notice($"Saving checkpoint.");
            }
        }

        [ArchivePatch(nameof(CheckpointManager.ReloadCheckpoint))]
        public class CheckpointManager_2Patch
        {
            public static Type Type() => typeof(CheckpointManager);
            public static void Prefix()
            {
                FeatureLogger.Notice($"Reloading checkpoint.");
            }
        }
    }
}
