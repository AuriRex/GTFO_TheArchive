using GameData;
using Globals;
using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheArchive.Core;
using TheArchive.Core.Managers;
using TheArchive.Managers;
using TheArchive.Utilities;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.CrashReportHandler;

namespace TheArchive
{

    public class ArchiveIL2CPPModule : IArchiveModule
    {
        internal static ArchiveIL2CPPModule instance;

        public static event Action<eGameStateName> OnGameStateChanged;

        public bool ApplyHarmonyPatches => true;

        public ArchivePatcher Patcher { get; set; }
        public ArchiveMod Core { get; set; }

        [SubModule(Utils.RundownFlags.RundownFour, Utils.RundownFlags.RundownFive)]
        public static string R5SubModule => "TheArchive.Resources.TheArchive.IL2CPP.R5.dll";
        [SubModule(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
        public static string R6SubModule => "TheArchive.Resources.TheArchive.IL2CPP.R6.dll";

        public CustomBoosterDropper BoosterDropManager { internal get; set; } = null;

        public void Init()
        {
            instance = this;

            CosturaUtility.Initialize();

            CrashReportHandler.SetUserMetadata("Modded", "true");
            CrashReportHandler.enableCaptureExceptions = false;

            if (ArchiveMod.Settings.DisableGameAnalytics)
                Analytics.enabled = false;

            typeof(EnemyDataBlock).RegisterSelf();
            typeof(GameDataBlockBase<>).RegisterSelf();
            typeof(GameDataBlockWrapper<>).RegisterSelf();

            typeof(HarmonyPatches.Patches.RichPresencePatches).RegisterAllPresenceFormatProviders();

            CustomProgressionManager.Logger = (string msg) => {
                ArchiveLogger.Msg(ConsoleColor.Magenta, msg);
            };

            Core.GameDataInitialized += OnGameDataInitialized;
            Core.DataBlocksReady += OnDataBlocksReady;
            Core.GameStateChanged += (eGameStateName_state) => OnGameStateChanged?.Invoke((eGameStateName) eGameStateName_state);
        }

        private void OnGameDataInitialized(Utils.RundownID rundownId)
        {
            try
            {
                DataBlockManager.Setup();

                if (ArchiveMod.Settings.SkipMissionUnlockRequirements)
                {
                    Global.AllowFullRundown = true;
                }
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
        }

        private void OnDataBlocksReady()
        {
        
        }
/*
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void BoosterSetup()
        {
            try
            {
                CustomBoosterDropManager.Instance.Setup();
            }
            catch(Exception ex)
            {
                ArchiveLogger.Error($"Error while trying to Setup {nameof(CustomBoosterDropManager)}!");
                ArchiveLogger.Exception(ex);
            }
        }*/

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
        }

        public void OnExit()
        {

        }

    }
}
