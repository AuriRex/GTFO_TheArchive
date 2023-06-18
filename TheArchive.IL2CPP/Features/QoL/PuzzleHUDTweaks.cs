using ChainedPuzzles;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.QoL
{
#warning TODO: Port to older rundowns
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    public class PuzzleHUDTweaks : Feature
    {
        public override string Name => "Scan HUD Tweaks";

        public override string Group => FeatureGroups.QualityOfLife;

        public override string Description => "Adds an overall alarm class counter to the HUD message";

        [FeatureConfig]
        public static PuzzleHUDTweaksSettings Settings { get; set; }

        public class PuzzleHUDTweaksSettings
        {
            [FSDisplayName("Use Roman Numerals")]
            [FSDescription("If Roman Numerals should be used instead of numbers:\nI, II, III, IV, V, VI, ...")]
            public bool UseRomanNumerals { get; set; } = true;

            [FSDisplayName("Ignore Single Scans")]
            [FSDescription("If alarms with only a single scan should hide the <color=white>(I/I)</color> text")]
            public bool IgnoreSingleScans { get; set; } = true;
        }


        [ArchivePatch(typeof(CP_Bioscan_Hud), nameof(CP_Bioscan_Hud.SetVisible))]
        internal static class CP_Bioscan_Hud_SetVisible_Patch
        {
            private static string _ogAtText;
            private static string _ogEnterScanText;

            public static void Postfix(CP_Bioscan_Hud __instance, int puzzleIndex)
            {
                if (!__instance.m_atText.Contains("("))
                {
                    _ogAtText = __instance.m_atText;
                    _ogEnterScanText = __instance.m_enterSecurityScanText;
                }

                // " AT " <- original ("AT" is localized)
                // " (I/IV) AT " <- modified
                var puzzleInstance = __instance?.transform?.parent?.GetComponentInChildren<ChainedPuzzleInstance>();

                if (puzzleInstance == null)
                    return;

                var current = puzzleIndex + 1;
                var total = puzzleInstance.m_chainedPuzzleCores.Count;

                if (total == 1 && Settings.IgnoreSingleScans)
                    return;

                string scanPuzzleProgress;

                if(Settings.UseRomanNumerals)
                {
                    scanPuzzleProgress = $" <color=white>({Utils.ToRoman(current)}/{Utils.ToRoman(total)})</color>";
                }
                else
                {
                    scanPuzzleProgress = $" <color=white>({current}/{total})</color>";
                }

                __instance.m_atText = $"{scanPuzzleProgress}{_ogAtText}";
                __instance.m_enterSecurityScanText = $"{_ogEnterScanText}{scanPuzzleProgress}";
            }
        }
    }
}
