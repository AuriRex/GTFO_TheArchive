using System;
using System.Collections;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Interfaces;
using TheArchive.Loader;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev;

[HideInModSettings]
[EnableFeatureByDefault]
[DisallowInGameToggle]
[RundownConstraint(RundownFlags.RundownFive, RundownFlags.Latest)]
internal class PageRundownPopupManager : Feature
{
    public override string Name => "PopupQueue";

    public override GroupBase Group => GroupManager.Dev;

    public override string Description => "Popups, yay!";

    public new static IArchiveLogger FeatureLogger { get; set; }

    private static void Empty()
    {

    }

    public static Action EmptyAction { get; private set; } = new(Empty);

    private static readonly Queue<PopupMessage> _popupQueue = new Queue<PopupMessage>();

    public static void ShowPopup(PopupMessage popupMessage)
    {
        if(MainMenuGuiLayer.Current?.PageRundownNew == null)
        {
            FeatureLogger.Error("Called too early!");
            return;
        }

        if (!MainMenuGuiLayer.Current.PageRundownNew.isActiveAndEnabled)
        {
            var pageRD = MainMenuGuiLayer.Current.PageRundownNew.gameObject;

            var enabledListener = pageRD.GetComponent<ModSettings.OnEnabledListener>();

            if (enabledListener == null)
            {
                enabledListener = pageRD.AddComponent<ModSettings.OnEnabledListener>();

                enabledListener.OnEnabledSelf += PageRundownEnabled;
            }

            _popupQueue.Enqueue(popupMessage);
            return;
        }

        try
        {
            GlobalPopupMessageManager.ShowPopup(popupMessage);
        }
        catch (Exception ex)
        {
            FeatureLogger.Error("Failed to show single popup.");
            FeatureLogger.Exception(ex);
        }
    }

    private static bool _runningAllPopups = false;
    private static void PageRundownEnabled(GameObject go)
    {
        if (_popupQueue.Count <= 0)
            return;

        if (_runningAllPopups)
            return;
            
        LoaderWrapper.StartCoroutine(ShowAllPopups());
    }

    private static IEnumerator ShowAllPopups()
    {
        if (_runningAllPopups)
            yield break;

        _runningAllPopups = true;
        yield return new WaitForSeconds(0.1f);

        while (_popupQueue.Count > 0)
        {
            try
            {
                var popupMessage = _popupQueue.Dequeue();

                GlobalPopupMessageManager.ShowPopup(popupMessage);
            }
            catch (Exception ex)
            {
                FeatureLogger.Error("Failed to show popup.");
                FeatureLogger.Exception(ex);
            }
        }

        _runningAllPopups = false;
    }
}