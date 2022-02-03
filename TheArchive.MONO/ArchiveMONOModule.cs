using Globals;
using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using TheArchive.Core;
using TheArchive.Utilities;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using static TheArchive.Utilities.Utils;

namespace TheArchive
{
    public class ArchiveMONOModule : IArchiveModule
    {
        internal static ArchiveMONOModule instance;

        public static SharedCoroutineStarter CoroutineHelper { get; private set; } = null;

        public bool ApplyHarmonyPatches => true;

        public ArchivePatcher Patcher { get; set; }
        public ArchiveMod Core { get; set; }

        public class SharedCoroutineStarter : MonoBehaviour
        {
            public static SharedCoroutineStarter Instance;

            public void Awake()
            {
                if (Instance == null) Instance = this;
                ArchiveLogger.Msg($"{nameof(SharedCoroutineStarter)} created!");
            }
        }

        public void Init()
        {
            instance = this;

            CrashReportHandler.SetUserMetadata("Modded", "true");
            CrashReportHandler.enableCaptureExceptions = false;

            Core.GameDataInitialized += OnGameDataInitialized;
            Core.DataBlocksReady += OnDataBlocksReady;
        }

        private void OnDataBlocksReady()
        {
#warning TODO: MONO DataBlock dumper
        }

        private void OnGameDataInitialized(RundownID rundownId)
        {
            if (ArchiveMod.Settings.SkipMissionUnlockRequirements && rundownId != RundownID.RundownOne)
            {
                AllowFullRundown();
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AllowFullRundown()
        {
            Global.AllowFullRundown = true;
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

            if (CoroutineHelper == null)
            {
                ArchiveLogger.Msg("Creating the coroutine helper.");
                var go = new GameObject("SharedCoroutineStarter");
                GameObject.DontDestroyOnLoad(go);
                CoroutineHelper = go.AddComponent<SharedCoroutineStarter>();
                GameObject.DontDestroyOnLoad(CoroutineHelper);
            }
        }

#if DEBUG
        private bool state = false;
#endif

        public void OnLateUpdate()
        {
            if(Input.GetKeyDown(KeyCode.F1) && ArchiveMod.Settings.EnableHudToggle)
            {
                // Toggle hud
                ArchiveMod.HudIsVisible = !ArchiveMod.HudIsVisible;
                GuiManager.PlayerLayer.SetVisible(ArchiveMod.HudIsVisible);
                GuiManager.WatermarkLayer.SetVisible(ArchiveMod.HudIsVisible);
                GuiManager.CrosshairLayer.SetVisible(ArchiveMod.HudIsVisible);
            }

#if DEBUG
            if(Input.GetKeyDown(KeyCode.F10))
            {
                FocusStateManager.ToggleFreeflight();
            }

            if(Input.GetKeyDown(KeyCode.F9))
            {
                FocusStateManager.ToggleDebugMenu();
            }


            if (Input.GetKeyDown(KeyCode.F8))
            {
                ArchiveLogger.Msg($"boop {state}");

                state = !state;
            }

            if (state)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            }
#endif

        }

        public void OnExit()
        {

        }

    }
}
