using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Utilities;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class PlayfabMatchmakingManagerPatches
    {


        [HarmonyPatch(typeof(PlayfabMatchmakingManager), nameof(PlayfabMatchmakingManager.Setup))]
        public class PlayfabMatchmakingManager_SetupPatch
        {

            public static bool Prefix()
            {
                if(AutoPlayFabPatches.DisableAllPlayFabInteraction)
                {
                    ArchiveLogger.Msg(ConsoleColor.Red, $"Canceled {nameof(PlayfabMatchmakingManager)}.{nameof(PlayfabMatchmakingManager.Setup)}().");
                    return false;
                }

                return true;
            }

        }

    }
}
