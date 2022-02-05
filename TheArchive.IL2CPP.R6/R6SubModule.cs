using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core;
using TheArchive.IL2CPP.R6.ArchivePatches;
using TheArchive.Managers;
using TheArchive.Utilities;
using UnhollowerRuntimeLib;
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

            MuteSpeakManager.Update();

            if (Input.GetKeyDown(KeyCode.F8))
            {
                MuteSpeakManager.EnableOtherVoiceBinds = !MuteSpeakManager.EnableOtherVoiceBinds;
                ArchiveLogger.Notice($"Voice binds enabled: {MuteSpeakManager.EnableOtherVoiceBinds}");
            }
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }
    }
}
