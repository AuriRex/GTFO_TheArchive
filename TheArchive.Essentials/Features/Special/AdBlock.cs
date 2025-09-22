using CellMenu;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Features.Dev;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Special;

[RundownConstraint(RundownFlags.RundownEight, RundownFlags.Latest)]
public class AdBlock : Feature
{
    public override string Name => "AdBlock";

    public override GroupBase Group => GroupManager.Special;

    public override string Description => "Removes the Den of Wolves button from the rundown screen.";

#if IL2CPP
    public static bool IsEnabled { get; set; }

    public override void OnEnable()
    {
        if (!DataBlocksReady)
            return;

        ToggleDOWImage(MainMenuGuiLayer.Current.PageRundownNew);
    }

    public override void OnDisable()
    {
        if (IsApplicationQuitting)
            return;

        ToggleDOWImage(MainMenuGuiLayer.Current.PageRundownNew, true);
    }

    [ArchivePatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.SetPageActive))]
    internal static class CM_PageRundown_New_ABC_Patch
    {
        public static void Postfix(CM_PageRundown_New __instance)
        {
            ToggleDOWImage(__instance);
        }
    }

    private static void ToggleDOWImage(CM_PageRundown_New __instance, bool setActive = false)
    {
        var buttonImageGO = __instance.m_movingContentHolder
            ?.GetChildWithExactName("PasteAndJoinOnLobbyID")
            ?.GetChildWithExactName("ButtonGIF")
            //?.GetChildWithExactName("Image")
            ?.gameObject;

        if (buttonImageGO == null)
            return;

        var enabledListener = buttonImageGO.GetComponent<ModSettings.OnEnabledListener>();

        if (enabledListener != null && setActive)
        {
            UnityEngine.Object.Destroy(enabledListener);
        }
        else if (enabledListener == null)
        {
            enabledListener = buttonImageGO.AddComponent<ModSettings.OnEnabledListener>();
            enabledListener.OnEnabledSelf += OnButtonEnabled;
        }

        buttonImageGO.SetActive(setActive);
    }

    private static void OnButtonEnabled(GameObject go)
    {
        if (!IsEnabled)
            return;

        go.SetActive(false);
    }
#endif
}