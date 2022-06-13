using AK;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core;
using TheArchive.Utilities;
using static HackingTool;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Utils;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableQualityOfLifeImprovements), "QOL")]
    public class QualityOfLifePatches
    {
        

        

        // Remove the character restriction in chat, this also results in the user being able to use TMP tags but whatever
        [ArchivePatch(typeof(PlayerChatManager), nameof(PlayerChatManager.Setup))]
        internal static class PlayerChatManager_SetupPatch
        {
            public static void Postfix(ref PlayerChatManager __instance)
            {
                try
                {
                    typeof(PlayerChatManager).GetProperty(nameof(PlayerChatManager.m_forbiddenChars)).SetValue(__instance, new UnhollowerBaseLib.Il2CppStructArray<int>(0));
                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
                
            }
        }

        

    }
}
