using TheArchive.Core;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.IL2CPP.R6
{
    public class R6SubModule : IArchiveModule
    {
        public bool ApplyHarmonyPatches => false;
        public bool UsesLegacyPatches => false;
        public ArchiveLegacyPatcher Patcher { get; set; }

        public void Init()
        {

        }

        public void OnExit()
        {

        }

        public void OnLateUpdate()
        {
            
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }
    }
}
