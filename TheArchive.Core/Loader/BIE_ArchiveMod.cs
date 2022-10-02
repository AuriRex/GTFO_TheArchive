#if BepInEx
using BepInEx;
using BepInEx.Unity.IL2CPP;
using System;

namespace TheArchive.Loader
{
    [BepInPlugin(ArchiveMod.GUID, ArchiveMod.MOD_NAME, ArchiveMod.VERSION_STRING)]
    public class BIE_ArchiveMod : BasePlugin
    {
        public override void Load()
        {
            var harmony = new HarmonyLib.Harmony(ArchiveMod.GUID);
            ArchiveMod.OnApplicationStart(LoaderWrapper.WrapLogger(Log), harmony);

            AddComponent<TheArchive_BIE_Controller>();
        }

        public override bool Unload()
        {
            ArchiveMod.OnApplicationQuit();

            return base.Unload();
        }

        public class TheArchive_BIE_Controller : UnityEngine.MonoBehaviour
        {
            public TheArchive_BIE_Controller(IntPtr ptr) : base(ptr) { }

            public void Awake()
            {
                DontDestroyOnLoad(this);
                hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            }

            public void Update()
            {
                ArchiveMod.OnUpdate();
            }

            public void LateUpdate()
            {
                ArchiveMod.OnLateUpdate();
            }
        }

        /*public override void OnUpdate()
        {
            ArchiveMod.OnUpdate();
        }

        public override void OnLateUpdate()
        {
            ArchiveMod.OnLateUpdate();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            ArchiveMod.OnSceneWasLoaded(buildIndex, sceneName);
        }*/


    }
}
#endif

