using GameData;
using Globals;
using System;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Managers;
using TheArchive.Managers;
using TheArchive.Utilities;
using UnityEngine;
using UnityEngine.CrashReportHandler;

[assembly: ModDefaultFeatureGroupName("TheArchive")]
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

            typeof(EnemyDataBlock).RegisterSelf();
            typeof(GameDataBlockBase<>).RegisterSelf();
            typeof(GameDataBlockWrapper<>).RegisterSelf();

            typeof(Features.RichPresenceCore).RegisterAllPresenceFormatProviders();

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
#if DEBUG
            if (Input.GetKeyDown(KeyCode.G) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.RightControl))
            {
                FeatureManager.Instance.DEBUG_DISABLE();
            }
            if (Input.GetKeyDown(KeyCode.H) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.RightControl))
            {
                FeatureManager.Instance.DEBUG_ENABLE();
            }
#endif
        }

        public void OnExit()
        {

        }

    }
}
