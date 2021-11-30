using GameData;
using Globals;
using HarmonyLib;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TheArchive;
using TheArchive.Core;
using TheArchive.HarmonyPatches;
using TheArchive.Managers;
using TheArchive.Utilities;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.CrashReportHandler;

namespace TheArchive
{

    public class ArchiveModule : IArchiveModule
    {

        public static event Action<uint> OnAfterGameDataInit;

        internal static ArchiveModule instance;

        public static uint CurrentRundownID { get; internal set; } = 0;

        private ArchivePatcher _patcher;
        private ArchiveMod _core;

        public void Init(ArchivePatcher patcher, ArchiveMod core)
        {
            instance = this;
            _patcher = patcher;
            _core = core;

            CrashReportHandler.SetUserMetadata("Modded", "true");
            CrashReportHandler.enableCaptureExceptions = false;

            CosturaUtility.Initialize();

            CustomProgressionManager.Logger = (string msg) => {
                MelonLogger.Msg(ConsoleColor.Magenta, msg);
            };

            OnAfterGameDataInit += (rundownId) => {
                var rundown = Utils.IntToRundownEnum((int) rundownId);
                if(rundown != Utils.RundownID.RundownFour)
                {
                    RundownFiveBoosterSetup();
                }
                _core.SetCurrentRundownAndPatch(rundown);
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RundownFiveBoosterSetup()
        {
            CustomBoosterDropManager.Instance.Setup();
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }


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

#if DEBUG
            if (Input.GetKeyDown(KeyCode.F10))
            {
                FocusStateManager.ToggleFreeflight();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                FocusStateManager.ToggleDebugMenu();
            }
#endif
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
