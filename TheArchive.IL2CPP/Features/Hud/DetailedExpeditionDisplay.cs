using CellMenu;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Hud
{
    [EnableFeatureByDefault]
    public class DetailedExpeditionDisplay : Feature
    {
        public override string Name => "Detailed Expedition Display";

        public override string Group => FeatureGroups.Hud;

        public override string Description => "Adds the current Rundown Number into the Header as well as onto the Map, Objectives and Success screens.";

#if BepInEx
        // Remove once the patches in here don't cause the runtime to shit itself. 
        public override bool ShouldInit()
        {
            return false;
        }
#endif

        public static bool IsOGBuild { get; private set; }

        public override void Init()
        {
            IsOGBuild = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownOne.To(RundownFlags.RundownSeven));
        }

        [FeatureConfig]
        public static RundownInHeaderSettings Settings { get; set; }

        public class RundownInHeaderSettings
        {
            [FSDisplayName($"{ALTText} and {OGText} prefixes")]
            [FSDescription($"Adds {ALTText} or {OGText} before any mention of the expedition.\n\n{OGText} for the original releases\n{ALTText} for the re-releases")]
            public bool IncludeALTorOGText { get; set; } = true;
        }

        public const string ALTText = "<color=orange>ALT://</color>";
        public const string OGText = "<color=orange>OG://</color>";

        private static bool IsAlt(string header)
        {
            if (IsOGBuild) return false;

            for (int i = 1; i <= 6; i++)
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
                        text = $"{OGText}{text}";
                    }
                    else
                    {
                        text = $"{ALTText}{text}";
                    }
                }

                __instance.m_expeditionName.text = text;
            }
        }

        // No references to the objectives screen type on mono build so reflection it is
        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch("SetExpeditionName")]
        internal static class CM_PageObjectives_SetExpeditionName_Patch
        {
            public static Type Type() => typeof(CM_PageMap).Assembly.GetType("CellMenu.CM_PageObjectives");

            public static void Prefix(ref string name) => CM_PageMap_SetExpeditionName_Patch.Prefix(ref name);
        }

        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(typeof(CM_PageMap), "SetExpeditionName")]
        internal static class CM_PageMap_SetExpeditionName_Patch
        {
            public static void Prefix(ref string name)
            {
                if (name.StartsWith(OGText) || name.StartsWith(ALTText))
                    return;

                if (IsOGBuild)
                {
                    name = $"R{(int)BuildInfo.Rundown}{name}";
                }

                if (Settings.IncludeALTorOGText)
                {
                    if (!IsAlt(name))
                    {
                        name = $"{OGText}{name}";
                    }
                    else
                    {
                        name = $"{ALTText}{name}";
                    }
                }
            }
        }

        // add R4 / R5 to the beginning of the header text ("R4A1: Crytology")
        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectiveHeader")]
        internal static class PlayerGuiLayer_UpdateObjectiveHeader_Patch
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
            public static void PostfixR7(PlayerGuiLayer __instance)
            {
                var headerText = __instance.m_wardenObjective.m_header.text;

                __instance.m_wardenObjective.m_header.text = $"R{(int)BuildInfo.Rundown}{headerText}";
            }

            [IsPostfix, RundownConstraint(RundownFlags.RundownAltOne, RundownFlags.Latest)]
            public static void PostfixAlt(PlayerGuiLayer __instance)
            {
                var headerText = __instance.m_wardenObjective.m_header.text;

                if (Settings.IncludeALTorOGText)
                {
                    // LOL
                    if (headerText.StartsWith("R7") || headerText.StartsWith("R8"))
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

#if MONO
        public static string GetHeaderText()
        {
            if (RundownManager.ActiveExpedition == null) return "WARDEN OBJECTIVE";

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

            if (Settings.IncludeALTorOGText)
            {
                rundownPrefix = $"{OGText}{rundownPrefix}";
            }

            return $"{rundownPrefix}{RundownManager.ActiveExpedition.Descriptive.Prefix}{activeExpeditionData.expeditionIndex + 1}: {RundownManager.ActiveExpedition.Descriptive.PublicName}";
        }

        // Map screen text for R2 & R3
        [RundownConstraint(RundownFlags.RundownTwo, RundownFlags.RundownThree)]
        [ArchivePatch(typeof(CM_PageMap), nameof(CM_PageMap.UpdateObjectiveHeader))]
        internal static class CM_PageMap_UpdateObjectiveHeader_Patch
        {
            public static bool Prefix(PUI_GameObjectives ___m_wardenObjective)
            {
                ___m_wardenObjective.SetHeader(GetHeaderText());

                return ArchivePatch.SKIP_OG;
            }
        }

        // Map screen text for R1
        [RundownConstraint(RundownFlags.RundownOne)]
        [ArchivePatch(typeof(CM_PageMap), nameof(CM_PageMap.UpdateObjectives))]
        internal static class CM_PageMap_UpdateObjectives_Patch
        {
            public static void Prefix(PUI_GameObjectives ___m_wardenObjective)
            {
                ___m_wardenObjective.SetHeader(GetHeaderText());
            }
        }

        // Change the "WARDEN OBJECTIVE" text in the top left of the screen to the current selected mission, ex: "R1A1: The Admin"
        [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownThree)]
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectives")]
        internal static class PlayerGuiLayer_UpdateObjectives_Patch
        {
            public static void Postfix(PUI_GameObjectives ___m_wardenObjective)
            {
                ___m_wardenObjective.SetHeader(GetHeaderText());
            }
        }
#endif
    }
}
