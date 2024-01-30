using GameData;
using Gear;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.QoL
{
    [RundownConstraint(RundownFlags.RundownSeven, RundownFlags.Latest)]
    internal class NoDroppedMagazineSounds : Feature
    {
        public override string Name => "No Magazine Drop Sound";

        public override FeatureGroup Group => FeatureGroups.QualityOfLife;

        public override string Description => "Removes the <i>globalally audible</i> sound whenever a magazine drops on the floor after a reload.";

        public override bool RequiresRestart => true;

#if IL2CPP
        public override void OnGameDataInitialized()
        {
            if (!Enabled)
                return;

            var allBlocks = GearMagPartDataBlock.GetAllBlocks();

            foreach(var block in allBlocks)
            {
                block.DropSoundType = MagazineDropSoundType.None;
            }
        }
#endif
    }
}
