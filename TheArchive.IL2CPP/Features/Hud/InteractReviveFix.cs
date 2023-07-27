using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Hud
{
    [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
    public class InteractReviveFix : Feature
    {
        public override string Name => "Fix Multi-Revive UI";

        public override string Group => FeatureGroups.Hud;

        public override string Description => "Fix revive progress <i>visually</i> resetting whenever multiple revives are going on at the same time.";


        [FeatureConfig]
        public static InteractReviveFixSettings Settings { get; set; }

        public class InteractReviveFixSettings
        {
            [FSDisplayName("Show Multi-Res Warning")]
            [FSDescription("Display whenever multiple people are trying to revive the same person.")]
            public bool ShowMultiResWarning { get; set; } = true;

            [FSDisplayName("Multi-Res Warning Text")]
            [FSDescription("The warning text to display.\nDefault is: <#F00><u>/!\\</u></color> <u>Double</u>")]
            public string MultiResWarning { get; set; } = "<#F00><u>/!\\</u></color> <u>Double</u>";

            [FSDisplayName("Replace revive text completely")]
            [FSDescription("If the revive text should be replaced completely by the setting above if multiple people are reviving the same person.")]
            public bool MultiResWarningReplaceCompletely { get; set; } = false;
        }

#if IL2CPP
        // Called once at the start and at the end of every players revive interaction
        [ArchivePatch(typeof(Interact_Revive), nameof(Interact_Revive.SetUIState))]
        internal static class Interact_Revive_SetUIState_Patch
        {
            public static bool Prefix(Interact_Revive __instance)
            {
                var deadGuy = __instance.m_interactTargetAgent;
                if (deadGuy.IsLocallyOwned)
                    return ArchivePatch.RUN_OG;

                foreach (var interactors in __instance.m_interactors)
                {
                    if (interactors.Agent.IsLocallyOwned)
                    {
                        return ArchivePatch.RUN_OG;
                    }
                }

                return ArchivePatch.SKIP_OG;
            }

            public static void Postfix(Interact_Revive __instance, bool state)
            {
                if (state && Settings.ShowMultiResWarning && __instance.m_interactors.Count > 1)
                {
                    string multiReviveText = Settings.MultiResWarning;
                    if (!Settings.MultiResWarningReplaceCompletely)
                    {
                        multiReviveText = $"{multiReviveText} {string.Format(Localization.Text.Get(852U), __instance.m_interactTargetAgent.InteractionName)}";
                    }

                    GuiManager.InteractionLayer.SetInteractPrompt(multiReviveText, "", ePUIMessageStyle.Default);
                }
            }
        }

        // Called everytime the timer updates on any players revive
        [ArchivePatch(typeof(Interact_Timed), nameof(Interact_Timed.OnTimerUpdate))]
        internal static class Interact_Timed_OnTimerUpdate_Patch
        {
            public static bool Prefix(Interact_Timed __instance, float timeRel)
            {
                // Had to patch the base class here because Interact_Revive does not override this method
                // Ignore all instances that aren't revives!
                if (__instance.TryCastTo<Interact_Revive>() == null)
                    return ArchivePatch.RUN_OG;

                var deadGuy = __instance.m_interactTargetAgent;
                if (deadGuy.IsLocallyOwned)
                    return ArchivePatch.RUN_OG;

                foreach (var interactors in __instance.m_interactors)
                {
                    if (interactors.Agent.IsLocallyOwned)
                    {
                        return ArchivePatch.RUN_OG;
                    }
                }

                return ArchivePatch.SKIP_OG;
            }
        }
#endif
    }
}
