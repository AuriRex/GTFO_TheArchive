using System;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.QoL
{
    [EnableFeatureByDefault(true)]
    public class RundownInHeader : Feature
    {
        public override string Name => "Rundown # in Header";


        // add R4 / R5 to the beginning of the header text ("R4A1: Crytology")
        [RundownConstraint(RundownFlags.RundownFour, RundownFlags.Latest)]
        [ArchivePatch(typeof(PlayerGuiLayer), "UpdateObjectiveHeader")]
        internal static class PlayerGuiLayer_UpdateObjectiveHeaderPatch
        {
            
            public static void Prefix(ref string header)
            {
                header = $"R{(int)BuildInfo.Rundown}{header}";
            }
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
