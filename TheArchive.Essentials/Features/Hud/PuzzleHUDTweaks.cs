using ChainedPuzzles;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Hud;

[RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
public class PuzzleHUDTweaks : Feature
{
    public override string Name => "Scan HUD Tweaks";

    public override FeatureGroup Group => FeatureGroups.Hud;

    public override string Description => "Adds an overall alarm class counter to the HUD message for door alarms etc";

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

#if IL2CPP
    [ArchivePatch(typeof(CP_Bioscan_Hud), nameof(CP_Bioscan_Hud.SetVisible))]
    internal static class CP_Bioscan_Hud_SetVisible_Patch
    {
        private static string _ogAtText;
        private static string _ogEnterScanText;

        public static void Postfix(CP_Bioscan_Hud __instance, int puzzleIndex, bool visible)
        {
            if (!__instance.m_atText.Contains("("))
            {
                _ogAtText = __instance.m_atText;
                _ogEnterScanText = __instance.m_enterSecurityScanText;
            }

            if (!visible)
                return;

            // " AT " <- original ("AT" is localized)
            // " (I/IV) AT " <- modified
            var allPuzzleInstances = __instance?.transform?.parent?.GetComponentsInChildren<ChainedPuzzleInstance>();
                
            ChainedPuzzleInstance puzzleInstance;
            if(allPuzzleInstances.Count > 1)
            {
                // Get the correct ChainedPuzzleInstance if there are multiple; ex: on Reactor Shutdown gameobjects
                puzzleInstance = allPuzzleInstances
                    .Where(cpi =>
                        cpi.m_chainedPuzzleCores.FirstOrDefault(core =>
                            GetHudFromCore(core)?.Pointer == __instance.Pointer
                        ) != null
                    ).FirstOrDefault();
            }
            else
            {
                puzzleInstance = allPuzzleInstances.FirstOrDefault();
            }

            if (puzzleInstance == null)
                return;

            var current = puzzleIndex + 1;
            var total = puzzleInstance.m_chainedPuzzleCores.Count;

            if (total == 1 && Settings.IgnoreSingleScans)
                return;

            string scanPuzzleProgress;

            if (Settings.UseRomanNumerals)
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

        public static iChainedPuzzleHUD GetHudFromCore(iChainedPuzzleCore core)
        {
            if(core.TryCastTo<CP_Bioscan_Core>(out var bioScan))
            {
                return bioScan.m_hud;
            }

            if (core.TryCastTo<CP_Cluster_Core>(out var clusterScan))
            {
                // Cluster Hud is a middle man between core and normal hud
                return clusterScan.m_hud?.TryCastTo<CP_Cluster_Hud>()?.m_hud;
            }

            return null;
        }
    }
#endif
}