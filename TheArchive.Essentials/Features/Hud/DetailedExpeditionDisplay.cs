using CellMenu;
using System;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Hud;

[EnableFeatureByDefault]
public class DetailedExpeditionDisplay : Feature
{
    public override string Name => "Detailed Expedition Display";

    public override GroupBase Group => GroupManager.Hud;

    public override string Description => "Adds the current Rundown Number into the Header as well as onto the Map, Objectives and Success screens.";

    public override bool ShouldInit()
    {
        return !IsPlayingModded;
    }

    public static bool IsOGBuild { get; private set; }

    public override void Init()
    {
        IsOGBuild = !Is.A1OrLater;
    }

    [FeatureConfig]
    public static RundownInHeaderSettings Settings { get; set; }

    public class RundownInHeaderSettings
    {
        [FSDisplayName($"{ALT_TEXT} and {OG_TEXT} prefixes")]
        [FSDescription($"Adds {ALT_TEXT} or {OG_TEXT} before any mention of the expedition.\n\n{OG_TEXT} for the original releases\n{ALT_TEXT} for the re-releases")]
        public bool IncludeALTorOGText { get; set; } = true;
    }

    public const string ALT_TEXT = "<color=orange>ALT://</color>";
    public const string OG_TEXT = "<color=orange>OG://</color>";

    private static bool IsAlt(string header)
    {
        if (IsOGBuild) return false;

        for (var i = 1; i <= 6; i++)
        {
            if (header.StartsWith("R" + i))
                return true;
        }

        return false;
    }

    [ArchivePatch(typeof(CM_PageExpeditionSuccess), nameof(UnityMessages.OnEnable))]
    internal static class CM_PageExpeditionSuccess_OnEnable_Patch
    {
        public static void Postfix(CM_PageExpeditionSuccess __instance)
        {

            var text = __instance.m_expeditionName.text;

            if (IsOGBuild && !Is.R2 && !Is.R3)
            {
                text = $"R{(int)BuildInfo.Rundown}{text}";
            }

            if (Settings.IncludeALTorOGText)
            {
                if (!IsAlt(text))
                {
                    text = $"{OG_TEXT}{text}";
                }
                else
                {
                    text = $"{ALT_TEXT}{text}";
                }
            }

            __instance.m_expeditionName.text = text;
        }
    }

    [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
    [ArchivePatch(typeof(CM_PageMap), nameof(CM_PageMap.SetExpeditionName))]
    internal static class CM_PageMap__SetExpeditionName__Patch
    {
        public static void Prefix(ref string name)
        {
            if (name.StartsWith(OG_TEXT) || name.StartsWith(ALT_TEXT))
                return;

            if (IsOGBuild)
            {
                name = $"R{(int)BuildInfo.Rundown}{name}";
            }

            if (!Settings.IncludeALTorOGText)
                return;
            
            if (IsAlt(name))
            {
                name = $"{ALT_TEXT}{name}";
                return;
            }
            
            name = $"{OG_TEXT}{name}";
        }
    }

    // add R4 / R5 to the beginning of the header text ("R4A1: Crytology")
    [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
    [ArchivePatch(typeof(PlayerGuiLayer), nameof(PlayerGuiLayer.UpdateObjectiveHeader))]
    internal static class PlayerGuiLayer__UpdateObjectiveHeader__Patch
    {
        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.RundownSix)]
        public static void Prefix(ref string header)
        {
            header = $"R{(int)BuildInfo.Rundown}{header}";

            if (Settings.IncludeALTorOGText)
            {
                header = $"{OG_TEXT}{header}";
            }
        }

#if IL2CPP
        [IsPostfix, RundownConstraint(RundownFlags.RundownSeven)]
        public static void PostfixR7(PlayerGuiLayer __instance)
        {
            var headerText = __instance.m_wardenObjective.m_header.text;

            __instance.m_wardenObjective.m_header.text = $"R{(int)BuildInfo.Rundown}{headerText}";
        }

        [IsPostfix, RundownConstraint(RundownFlags.RundownAltOne, RundownFlags.Latest)]
        public static void PostfixAlt(PlayerGuiLayer __instance)
        {
            var headerText = __instance.m_wardenObjective.m_header.text;

            if (!Settings.IncludeALTorOGText)
                return;
            
            // LOL
            if (headerText.StartsWith("R7") || headerText.StartsWith("R8"))
            {
                __instance.m_wardenObjective.m_header.text = $"{OG_TEXT}{headerText}";
                return;
            }

            __instance.m_wardenObjective.m_header.text = $"{ALT_TEXT}{headerText}";
        }
#endif
    }
}