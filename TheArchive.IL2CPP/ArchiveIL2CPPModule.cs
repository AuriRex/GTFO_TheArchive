using GameData;
using Globals;
using HarmonyLib;
using MelonLoader;
using Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TheArchive;
using TheArchive.Core;
using TheArchive.Core.Core;
using TheArchive.HarmonyPatches;
using TheArchive.Managers;
using TheArchive.Utilities;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.CrashReportHandler;

namespace TheArchive
{

    public class ArchiveIL2CPPModule : IArchiveModule
    {

        public static event Action<uint> OnAfterGameDataInit;

        internal static ArchiveIL2CPPModule instance;

        public static uint CurrentRundownID { get; internal set; } = 0;

        public bool ApplyHarmonyPatches => true;

        public ArchivePatcher Patcher { get; set; }
        public ArchiveMod Core { get; set; }

        [SubModule(Utils.RundownFlags.RundownFour, Utils.RundownFlags.RundownFive)]
        public static string R5SubModule => "TheArchive.Resources.TheArchive.IL2CPP.R5.dll";
        [SubModule(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
        public static string R6SubModule => "TheArchive.Resources.TheArchive.IL2CPP.R6.dll";

        public void Init()
        {
            instance = this;

            if(!ArchiveMod.Settings.DisableGameAnalytics)
            {
                CrashReportHandler.SetUserMetadata("Modded", "true");
                CrashReportHandler.enableCaptureExceptions = false;
            }

            CosturaUtility.Initialize();

            ClassInjector.RegisterTypeInIl2Cpp<TestSoundComp>();

            CustomProgressionManager.Logger = (string msg) => {
                ArchiveLogger.Msg(ConsoleColor.Magenta, msg);
            };

            OnAfterGameDataInit += (rundownId) => {
                var rundown = Utils.IntToRundownEnum((int) rundownId);
                if(rundown != Utils.RundownID.RundownFour)
                {
                    BoosterSetup();
                }
                Core.SetCurrentRundownAndPatch(rundown);
                DataBlockManager.DumpDataBlocksToDisk();
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void BoosterSetup()
        {
            CustomBoosterDropManager.Instance.Setup();
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }

        #region soundtestthing
        private bool doOnce = true;
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

        public void OnLateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1) && ArchiveMod.Settings.EnableHudToggle)
            {
                // Toggle hud
                ArchiveMod.HudIsVisible = !ArchiveMod.HudIsVisible;
                GuiManager.PlayerLayer.SetVisible(ArchiveMod.HudIsVisible);
                GuiManager.WatermarkLayer.SetVisible(ArchiveMod.HudIsVisible);
                GuiManager.CrosshairLayer.SetVisible(ArchiveMod.HudIsVisible);
            }

            if(doOnce)
            {
                ArchiveLogger.Warning("Creating SoundTest GameObject!");
                test = new GameObject("TestSoundThing!");
                GameObject.DontDestroyOnLoad(test);
                test.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
                testSoundComp = test.AddComponent<TestSoundComp>();

                doOnce = false;
            }

            

            MuteSpeakManager.Update();

            if (Input.GetKeyDown(KeyCode.F8))
            {
                MuteSpeakManager.EnableOtherVoiceBinds = !MuteSpeakManager.EnableOtherVoiceBinds;
                ArchiveLogger.Notice($"Voice binds enabled: {MuteSpeakManager.EnableOtherVoiceBinds}");
            }

#if DEBUG
#warning move this to seperate toolbelt mod
            if (Input.GetKeyDown(KeyCode.F10))
            {
                FocusStateManager.ToggleFreeflight();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                FocusStateManager.ToggleDebugMenu();
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                testSoundComp.soundPlayer.Post(AK.EVENTS.AMBIENCEALLSTOP);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                testSoundComp.soundPlayer.Post(AK.EVENTS.AMBIENCE_STOP_ALL);
            }
#endif
        }

        public void OnExit()
        {

        }

        [HarmonyPatch(typeof(GameDataInit), "Initialize")]
        internal static class GameDataInit_InitializePatch
        {
            public static void Postfix()
            {
                GameSetupDataBlock block = GameDataBlockBase<GameSetupDataBlock>.GetBlock(1u);
                var rundownId = block.RundownIdToLoad;

                CurrentRundownID = rundownId;

                OnAfterGameDataInit?.Invoke(rundownId);
            }
        }

        [HarmonyPatch(typeof(GlobalSetup), "Awake")]
        internal static class GlobalSetup_AwakePatch
        {
            public static void Postfix(GlobalSetup __instance)
            {
                //__instance.m_allowFullRundown = true;
            }
        }
    }
}
