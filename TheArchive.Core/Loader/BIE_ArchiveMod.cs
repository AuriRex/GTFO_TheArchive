#if BepInEx
using BepInEx;
using BepInEx.Unity.IL2CPP;
using System;
using JetBrains.Annotations;
using UnityEngine;

namespace TheArchive.Loader;

/// <summary>
/// BepInEx entry point.
/// </summary>
[BepInPlugin(ArchiveMod.GUID, ArchiveMod.MOD_NAME, ArchiveMod.VERSION_STRING)]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ArchiveMod.MTFO_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[UsedImplicitly]
public class BIE_ArchiveMod : BasePlugin
{
    internal static MonoBehaviour MainComponent { get; private set; }

    /// <inheritdoc/>
    public override void Load()
    {
        var harmony = new HarmonyLib.Harmony(ArchiveMod.GUID);
        ArchiveMod.OnApplicationStart(LoaderWrapper.WrapLogger(Log, ConsoleColor.DarkMagenta), harmony);

        Application.add_quitting((Il2CppSystem.Action) (() => ArchiveMod.OnApplicationQuit()));

        MainComponent = AddComponent<TheArchive_BIE_Controller>();
    }

    /// <inheritdoc/>
    public override bool Unload()
    {
        ArchiveMod.OnApplicationQuit();

        return base.Unload();
    }

    internal class TheArchive_BIE_Controller : MonoBehaviour
    {
        public TheArchive_BIE_Controller(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            DontDestroyOnLoad(this);
            hideFlags = HideFlags.HideAndDontSave;
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
}
#endif

