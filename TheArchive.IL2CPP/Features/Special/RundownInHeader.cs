using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Special
{
    [EnableFeatureByDefault]
    public class RundownInHeader : Feature
    {
        public override string Name => "Rundown # in Header";

        public override string Group => FeatureGroups.QualityOfLife;

        public override bool ShouldInit()
        {
            return !IsPlayingModded;
        }

        [FeatureConfig]
        public static RundownInHeaderSettings Settings { get; set; }

        public class RundownInHeaderSettings
        {
            [FSDisplayName($"Add {ALTText} and {OGText} prefixes")]
            public bool IncludeALTorOGText { get; set; } = true;
        }

        public const string ALTText = "<color=orange>ALT://</color>";
        public const string OGText = "<color=orange>OG://</color>";

        // add R4 / R5 to the beginning of the header text ("R4A1: Crytology")
        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectiveHeader")]
        internal static class PlayerGuiLayer_UpdateObjectiveHeaderPatch
        {
            [RundownConstraint(RundownFlags.RundownFour, RundownFlags.RundownSix)]
            public static void Prefix(ref string header)
            {
                header = $"R{(int)BuildInfo.Rundown}{header}";

                if (Settings.IncludeALTorOGText)
                {
                    header = $"{OGText}{header}";
                }
            }

#if IL2CPP
            [IsPostfix, RundownConstraint(RundownFlags.RundownSeven)]
            public static void PostfixR7(PlayerGuiLayer __instance, ref Il2CppSystem.Func<string> header)
            {
                var headerText = __instance.m_wardenObjective.m_header.text;

                __instance.m_wardenObjective.m_header.text = $"R{(int)BuildInfo.Rundown}{headerText}";
            }

            [IsPostfix, RundownConstraint(RundownFlags.RundownAltOne, RundownFlags.Latest)]
            public static void PostfixAlt(PlayerGuiLayer __instance, ref Il2CppSystem.Func<string> header)
            {
                var headerText = __instance.m_wardenObjective.m_header.text;

                if (Settings.IncludeALTorOGText)
                {
                    if(headerText.StartsWith("R7"))
                    {
                        __instance.m_wardenObjective.m_header.text = $"{OGText}{headerText}";
                    }
                    else
                    {
                        __instance.m_wardenObjective.m_header.text = $"{ALTText}{headerText}";
                    }
                }
            }
#endif

        }

        // Change the "WARDEN OBJECTIVE" text in the top left of the screen to the current selected mission, ex: "R1A1: The Admin"
        [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownThree)]
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectives")]
        internal static class PlayerGuiLayer_UpdateObjectivesPatch
        {
            public static void Postfix(ref PlayerGuiLayer __instance, ref PUI_GameObjectives ___m_wardenObjective)
            {
                try
                {
                    if (RundownManager.ActiveExpedition == null) return;

                    pActiveExpedition activeExpeditionData = RundownManager.GetActiveExpeditionData();

                    string rundownPrefix = string.Empty;

                    switch (ArchiveMod.CurrentRundown)
                    {
                        case RundownID.RundownTwo:
                        case RundownID.RundownThree:
                            break;
                        default:
                            rundownPrefix = $"R{(int)ArchiveMod.CurrentRundown}";
                            break;
                    }

                    if(Settings.IncludeALTorOGText)
                    {
                        rundownPrefix = $"{OGText}{rundownPrefix}";
                    }

                    ___m_wardenObjective.m_header.text = $"{rundownPrefix}{RundownManager.ActiveExpedition.Descriptive.Prefix}{activeExpeditionData.expeditionIndex + 1}: {RundownManager.ActiveExpedition.Descriptive.PublicName}";
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }

    }
}
