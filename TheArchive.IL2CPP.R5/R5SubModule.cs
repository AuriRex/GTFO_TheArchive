using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core;

namespace TheArchive.IL2CPP.R5
{
    public class R5SubModule : IArchiveModule
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
