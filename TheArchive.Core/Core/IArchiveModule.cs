using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Core
{
    public interface IArchiveModule
    {
        bool ApplyHarmonyPatches { get; }
        ArchivePatcher Patcher { get; set; }
        ArchiveMod Core { get; set; }

        void Init();
        void OnSceneWasLoaded(int buildIndex, string sceneName);
        void OnLateUpdate();
        void OnExit();
    }
}
