using System;
using System.Collections;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Loader;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev
{
    [HideInModSettings]
    [EnableFeatureByDefault]
    [DisallowInGameToggle]
    [RundownConstraint(RundownFlags.RundownFive, RundownFlags.Latest)]
    internal class PageRundownPopupManager : Feature
    {
        public override string Name => "PopupQueue";

        public override FeatureGroup Group => FeatureGroups.Dev;

        public override string Description => "Popups, yay!";

        public static new IArchiveLogger FeatureLogger { get; set; }

        public override void Init()
        {
            LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<ILoveWhenTheThingCrashesBecauseIdkYaaaaay>();
        }

        public class ILoveWhenTheThingCrashesBecauseIdkYaaaaay : MonoBehaviour
        {
            protected PopupMessage _0;
            protected PopupMessage _1;
            protected PopupMessage _2;
            protected PopupMessage _3;
            protected PopupMessage _4;
            protected PopupMessage _5;
            protected PopupMessage _6;
            protected PopupMessage _7;
            protected PopupMessage _8;
            protected PopupMessage _9;

            public const int MAX_POPUPS = 10;

            private int _count = 0;

            public ILoveWhenTheThingCrashesBecauseIdkYaaaaay(IntPtr ptr) : base(ptr) { }

            // Idk why but it seems to even work above 10 messages lmaoo
            public void MagicJankCache(PopupMessage popupMessage)
            {
                var field = typeof(ILoveWhenTheThingCrashesBecauseIdkYaaaaay).GetField($"_{_count}", AnyBindingFlagss);

                if (field == null)
                    throw new InvalidOperationException("Please fix this, thanks!");

                field.SetValue(this, popupMessage);

                _count++;
                if(_count >= MAX_POPUPS)
                {
                    _count = 0;
                }
            }
        }

        private static readonly Queue<PopupMessage> _popupQueue = new Queue<PopupMessage>();
        private static ILoveWhenTheThingCrashesBecauseIdkYaaaaay _cache;

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
                    _cache = pageRD.AddComponent<ILoveWhenTheThingCrashesBecauseIdkYaaaaay>();

                    enabledListener.OnEnabledSelf += PageRundownEnabled;
                }

                _cache.MagicJankCache(popupMessage);
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
}
