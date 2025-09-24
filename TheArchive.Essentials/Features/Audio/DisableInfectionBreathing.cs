using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Accessibility;

[RundownConstraint(RundownFlags.RundownTwo, RundownFlags.Latest)]
internal class DisableInfectionBreathing : Feature
{
    public override string Name => "Disable Breathing (<#1F835F>Infection</color>)";

    public override GroupBase Group => GroupManager.Audio;

    public override string Description => "Disables the infection \"drinking-straw-sucking\" sounding sound.";

    public static void SetInfectionBreathingIntensity(PlayerBreathing playerBreathing, float value)
    {
        DisableStaminaBreathing.A_m_sfxBreathe.Get(playerBreathing).SetRTPCValue(AK.GAME_PARAMETERS.INFECTION_LEVEL, value);
    }

    [ArchivePatch(typeof(PlayerBreathing), nameof(PlayerBreathing.UpdateInfectionRTPC))]
    internal static class PlayerBreathing_UpdateInfectionRTPC_Patch
    {
        public static bool Prefix(PlayerBreathing __instance)
        {
            SetInfectionBreathingIntensity(__instance, 0f);
            return ArchivePatch.SKIP_OG;
        }
    }
}