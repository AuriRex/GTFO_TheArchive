using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core;
using TheArchive.IL2CPP.R6.ArchivePatches;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.IL2CPP.R6
{
    public class R6SubModule : IArchiveModule
    {
        public bool ApplyHarmonyPatches => false;

        public ArchivePatcher Patcher { get; set; }
        public ArchiveMod Core { get; set; }

        public void Init()
        {

        }

        public void OnExit()
        {

        }

        public void OnLateUpdate()
        {
#if DEBUG
#warning Remove and add to seperate toolbelt mod
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    PlayerAgentPatches.PlayerAgent_TryWarpToPatch.DimensionIndex -= 1;
                    ArchiveLogger.Info($"Current Dimension Override: {PlayerAgentPatches.PlayerAgent_TryWarpToPatch.DimensionIndex}");
                }

                if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    PlayerAgentPatches.PlayerAgent_TryWarpToPatch.DimensionIndex += 1;
                    ArchiveLogger.Info($"Current Dimension Override: {PlayerAgentPatches.PlayerAgent_TryWarpToPatch.DimensionIndex}");
                }

                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    PlayerAgentPatches.PlayerAgent_TryWarpToPatch.OverrideDimensionIndex = !PlayerAgentPatches.PlayerAgent_TryWarpToPatch.OverrideDimensionIndex;
                    ArchiveLogger.Notice($"Should Override Dimension index: {PlayerAgentPatches.PlayerAgent_TryWarpToPatch.OverrideDimensionIndex}");
                }
            }
#endif
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }
    }
}
