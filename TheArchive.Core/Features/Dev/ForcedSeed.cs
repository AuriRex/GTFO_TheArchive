using GameData;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;

namespace TheArchive.Features.Dev;

[HideInModSettings]
public class ForcedSeed : Feature
{
    public override string Name => "Force Expedition Seeds";

    public override FeatureGroup Group => FeatureGroups.Dev;

    public override string Description => $"Force Seeds used to randomize objectives, boxes and enemies spawns.\n\n<#F00>(Master only!)</color>\n(Probably doesn't work in multiplayer, idk ... haven't tested it :p)";

    public new static IArchiveLogger FeatureLogger { get; set; }

    [FeatureConfig]
    public static ForcedSeedSettings Settings { get; set; }

    public class ForcedSeedSettings
    {
        [FSDisplayName("Force Session Seed")]
        public bool ForceSessionSeed { get; set; } = true;

        [FSDisplayName("Session Seed Override")]
        [FSDescription("The main seed used for level randomization.")]
        public int SessionSeed { get; set; } = 0;

        [FSDisplayName("Force HostID Seed")]
        public bool ForceHostIDSeed { get; set; } = false;

        [FSDisplayName("HostID Seed Override")]
        public int HostIDSeed { get; set; } = 0;
    }

    //[ArchivePatch(typeof(RundownManager), nameof(RundownManager.SetActiveExpedition))]
    [ArchivePatch(typeof(ExpeditionInTierData), nameof(ExpeditionInTierData.SetSeeds))]
    //public void SetSeeds(int hostIDSeed, int sessionSeed)
    internal static class RundownManager_SetActiveExpedition_Patch
    {
        //public static void Prefix(ref pActiveExpedition expPackage, ref ExpeditionInTierData expTierData)
        public static void Prefix(ref int hostIDSeed, ref int sessionSeed)
        {
            if (!SNetwork.SNet.IsMaster)
                return;


            if (Settings.ForceSessionSeed)
            {
                FeatureLogger.Notice($"Forcing seed: {nameof(ForcedSeedSettings.SessionSeed)}: {Settings.SessionSeed}");
                sessionSeed = Settings.SessionSeed;
            }

            if (Settings.ForceHostIDSeed)
            {
                FeatureLogger.Notice($"Forcing seed: {nameof(ForcedSeedSettings.HostIDSeed)}: {Settings.HostIDSeed}");
                hostIDSeed = Settings.HostIDSeed;
            }
        }
    }
}