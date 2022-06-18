using GameData;
using Globals;
using System;
using System.Runtime.CompilerServices;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Utilities;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using static TheArchive.Utilities.Utils;

[assembly: ModDefaultFeatureGroupName("TheArchive")]
namespace TheArchive
{
    public class ArchiveMONOModule : IArchiveModule
    {
        internal static ArchiveMONOModule instance;

        public static event Action<eGameStateName> OnGameStateChanged;

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

            typeof(EnemyDataBlock).RegisterSelf();
            typeof(GameDataBlockBase<>).RegisterSelf();
            typeof(GameDataBlockWrapper<>).RegisterSelf();

            typeof(Features.RichPresenceCore).RegisterAllPresenceFormatProviders();

            Core.GameDataInitialized += OnGameDataInitialized;
            Core.DataBlocksReady += OnDataBlocksReady;
            Core.GameStateChanged += (eGameStateName_state) => OnGameStateChanged?.Invoke((eGameStateName) eGameStateName_state);
        }

        private void OnDataBlocksReady()
        {
            try
            {
                DataBlockManager.Setup();
            }
            catch (Exception ex)
            {
                ArchiveLogger.Exception(ex);
            }
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
