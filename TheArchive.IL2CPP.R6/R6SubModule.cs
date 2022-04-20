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

            MuteSpeak.Update();

            if (Input.GetKeyDown(KeyCode.F8))
            {
                MuteSpeak.EnableOtherVoiceBinds = !MuteSpeak.EnableOtherVoiceBinds;
                ArchiveLogger.Notice($"Voice binds enabled: {MuteSpeak.EnableOtherVoiceBinds}");
            }
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }
    }
}
