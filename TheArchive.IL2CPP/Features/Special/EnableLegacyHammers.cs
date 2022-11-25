using GameData;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Special
{
    [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
    public class EnableLegacyHammers : Feature
    {
        public override string Name => "Enable old Hammers";

        public override string Group => FeatureGroups.Special;

        public override string Description => "Re-enable the pre-R6 Hammers:\nMaul, Gavel, Sledge and Mallet";

        public override bool RequiresRestart => true;

        public new static IArchiveLogger FeatureLogger { get; set; }

#if IL2CPP
        public override void OnEnable()
        {
            DataBlockManager.RegisterTransformationForDB<PlayerOfflineGearDataBlock>(Transform, -10);
        }

        public static void Transform(List<PlayerOfflineGearDataBlock> allPlayerOfflineGearDataBlocks)
        {
            var blocksToEnable = new List<string>
            {
                "Maul",
                "Gavel",
                "Mallet",
                "Sledgehammer"
            };

            foreach(var block in allPlayerOfflineGearDataBlocks)
            {
                if(blocksToEnable.Contains(block.name))
                {
                    FeatureLogger.Success($" > Enabling block: ID: {block.persistentID}, Name: '{block.name}'");
                    block.internalEnabled = true;
                }
            }
        }
#endif
    }
}
