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

        void Init(ArchivePatcher patcher, ArchiveMod core);
        void OnSceneWasLoaded(int buildIndex, string sceneName);
        void OnLateUpdate();

    }
}
