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

        private bool enableVoiceBinds = false;

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
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                var localPlayer = PlayerManager.GetLocalPlayerAgent();
                
                PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.PLAY_CL_LEFT);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                var localPlayer = PlayerManager.GetLocalPlayerAgent();

                PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.PLAY_CL_RIGHT);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                var localPlayer = PlayerManager.GetLocalPlayerAgent();

                PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.PLAY_CL_YES);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                var localPlayer = PlayerManager.GetLocalPlayerAgent();

                PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.PLAY_CL_NO);
            }
            if(enableVoiceBinds)
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
                    var localPlayer = PlayerManager.GetLocalPlayerAgent();

                    PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.PLAY_CL_NICE);
                }
                if (Input.GetKeyDown(KeyCode.K))
                {
                    var localPlayer = PlayerManager.GetLocalPlayerAgent();

                    PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.PLAY_CL_SORRY);
                }
                if (Input.GetKeyDown(KeyCode.J))
                {
                    var localPlayer = PlayerManager.GetLocalPlayerAgent();

                    PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.PLAY_CL_WELLDONE);
                }

                if (Input.GetKeyDown(KeyCode.P))
                {
                    var localPlayer = PlayerManager.GetLocalPlayerAgent();

                    PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.PLAY_CL_THANKYOU);
                }
                if (Input.GetKeyDown(KeyCode.U))
                {
                    var localPlayer = PlayerManager.GetLocalPlayerAgent();

                    PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.SCOUT_DETECT_SCREAM_CHARGE);
                }
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    var localPlayer = PlayerManager.GetLocalPlayerAgent();

                    PlayerVoiceManager.WantToSay(localPlayer.CharacterID, AK.EVENTS.SCOUT_DETECT_SCREAM);
                }

                if (Input.GetKeyDown(KeyCode.H))
                {
                    HarmonyPatches.Patches.OtherPatches.UnityEngine_RandomPatchOne.SetSeed = 1;
                    PlayerVoiceManager.WantToSay(2, AK.EVENTS.PLAY_FALLDAMAGEGRUNT02_5A);
                }
            }
            


            if (Input.GetKeyDown(KeyCode.F10))
            {
                FocusStateManager.ToggleFreeflight();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                FocusStateManager.ToggleDebugMenu();
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                enableVoiceBinds = !enableVoiceBinds;
                ArchiveLogger.Notice($"Voice binds enabled: {enableVoiceBinds}");
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
