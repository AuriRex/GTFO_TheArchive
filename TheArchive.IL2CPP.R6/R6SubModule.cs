using TheArchive.Core;
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
            
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }
    }
}
