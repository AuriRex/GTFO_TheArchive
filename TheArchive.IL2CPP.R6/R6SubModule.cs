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

        #region soundtestthing
        private GameObject test;
        private TestSoundComp testSoundComp;

        public class TestSoundComp : MonoBehaviour
        {
            public TestSoundComp(IntPtr ptr) : base(ptr) { }

            public CellSoundPlayer soundPlayer;
            public void Start()
            {
                soundPlayer = new CellSoundPlayer();
            }
        }
        #endregion

        public void Init()
        {

            ClassInjector.RegisterTypeInIl2Cpp<TestSoundComp>();

            ArchiveLogger.Warning("Creating SoundTest GameObject!");
            test = new GameObject("TestSoundThing!");
            GameObject.DontDestroyOnLoad(test);
            test.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
            testSoundComp = test.AddComponent<TestSoundComp>();

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

                if (Input.GetKeyDown(KeyCode.F6))
                {
                    testSoundComp.soundPlayer.Post(AK.EVENTS.AMBIENCEALLSTOP);
                }

                if (Input.GetKeyDown(KeyCode.F5))
                {
                    testSoundComp.soundPlayer.Post(AK.EVENTS.AMBIENCE_STOP_ALL);
                }
            }
#endif
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }
    }
}
