using CellMenu;
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

        public override string Description => "Adds the current Rundown Number into the Header as well as onto the Map and Objectives screens on OG builds.";

        public override bool ShouldInit()
        {
            return !IsPlayingModded;
        }

        public static bool IsOGBuild { get; private set; }

        public override void Init()
        {
            IsOGBuild = BuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownOne.To(RundownFlags.RundownSeven));
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

        private static bool IsAlt(string header)
        {
            if (IsOGBuild) return false;

            for(int i = 1; i <= 6; i++)
            {
                if (header.StartsWith("R" + i))
                    return true;
            }

            return false;
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
