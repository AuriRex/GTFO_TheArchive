using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Audio
{
    [RundownConstraint(RundownFlags.RundownTwo, RundownFlags.Latest)]
    public class DisableSpitterAudio : Feature
    {
        public override string Name => "Disable Spitter Audio";

        public override FeatureGroup Group => FeatureGroups.Audio;

        public override string Description => "Completely Removes all Audio from spitters.\n\n<color=orange>Keep in mind that you won't get any auditory warnings before it's too late</color>";


        private static MethodAccessor<InfectionSpitter> _A_CleanupSound;
        public override void Init()
        {
            _A_CleanupSound = MethodAccessor<InfectionSpitter>.GetAccessor("CleanupSound");
        }

        public override void OnEnable()
        {
            if (!DataBlocksReady)
                return;

            foreach (var zone in SharedUtils.GetAllZones())
            {
                foreach (var spitter in zone.GetComponentsInChildren<InfectionSpitter>())
                {
                    _A_CleanupSound.Invoke(spitter);
                }
            }
        }

        [ArchivePatch(typeof(InfectionSpitter), "TryPlaySound")]
        internal static class InfectionSpitter_TryPlaySound_Patch
        {
            public static bool Prefix(InfectionSpitter __instance, uint id)
            {
                _A_CleanupSound.Invoke(__instance);
                return ArchivePatch.SKIP_OG;
            }
        }
    }
}
