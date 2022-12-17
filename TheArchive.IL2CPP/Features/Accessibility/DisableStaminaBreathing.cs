using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Accessibility
{
    internal class DisableStaminaBreathing : Feature
    {
        public override string Name => "Disable Breathing (<#8ED7E4>Stamina</color>)";

        public override string Group => FeatureGroups.Accessibility;

        public override string Description => "Disables the player breathing and panting due to running around or enemy encounters in pre-R6 builds.";

        public static new IArchiveLogger FeatureLogger { get; set; }

        public static IValueAccessor<PlayerBreathing, CellSoundPlayer> A_m_sfxBreathe;
        public override void Init()
        {
            A_m_sfxBreathe = AccessorBase.GetValueAccessor<PlayerBreathing, CellSoundPlayer>("m_sfxBreathe");
        }

        public override void OnEnable()
        {
            var localPlayer = Player.PlayerManager.GetLocalPlayerAgent();

            if (localPlayer == null || localPlayer.Breathing == null)
                return;

            localPlayer.Breathing.enabled = false;
            A_m_sfxBreathe.Get(localPlayer.Breathing).SafePost(AK.EVENTS.INTENSITY_0_SILENT);
            if (BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest()))
                StaminaGoneR6Plus(localPlayer);
        }

        private static void StaminaGoneR6Plus(Player.PlayerAgent localPlayer)
        {
#if IL2CPP
            localPlayer.Breathing.m_sfxBreathe.Post(AK.EVENTS.STAMINA_EXHAUSTED_STOP, localPlayer.Position);
#endif
        }

        public override void OnDisable()
        {
            var localPlayer = Player.PlayerManager.GetLocalPlayerAgent();

            if (localPlayer == null || localPlayer.Breathing == null)
                return;

            localPlayer.Breathing.enabled = true;
        }

        [ArchivePatch(typeof(PlayerBreathing), nameof(PlayerBreathing.Setup))]
        internal static class PlayerBreathing_Setup_Patch
        {
            public static void Postfix(PlayerBreathing __instance)
            {
                __instance.enabled = false;
            }
        }
    }
}
