using CellMenu;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Special
{
    // TODO: Custom Images / Sounds?
    public class ExpeditionResultsScreenTweaks : Feature
    {
        public override string Name => "Results Screen Tweaks";

        public override string Group => FeatureGroups.Special;

        public override string Description => "Tweak the Expedition Fail/Success screens!";

        public override bool PlaceSettingsInSubMenu => true;

        public override bool SkipInitialOnEnable => true;

        public static new IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static ExpeditionResultsScreenTweaksSettings Settings { get; set; }

        public class ExpeditionResultsScreenTweaksSettings
        {
            [FSDisplayName("Add Failed Expedition Timer")]
            [FSDescription("Adds an expedition timer to the fail screen that displayes total time spent in the level.\n\n<color=orange>(Requires Enhanced Expedition Timer to be enabled!)</color>")]
            public bool ShowTimerOnFailScreen { get; set; } = true;

            [FSDisplayName("Success Expedition Timer")]
            [FSDescription("Replaces the default timer with the enhanced version.\n(Adds a timer to R1-R3)\n\n<color=orange>(Requires Enhanced Expedition Timer to be enabled!)</color>")]
            public bool OverrideEnhancedTimerOnSuccessScreen { get; set; } = true;

            [FSDisplayName("Disable Success Background")]
            [FSDescription("Removes the background image on the <b>Expedition Success Screen</b>.\n(background turns black instead)")]
            [FSRundownHint(RundownFlags.RundownSix, RundownFlags.Latest)]
            [FSIdentifier(nameof(DisableSuccessMask))]
            public bool DisableSuccessMask { get; set; } = false;

            [FSDisplayName("Disable Fail Background")]
            [FSDescription("Removes the background image on the <b>Expedition Fail Screen</b>.\n(background turns black instead)")]
            [FSRundownHint(RundownFlags.RundownSix, RundownFlags.Latest)]
            [FSIdentifier(nameof(DisableFailSkull))]
            public bool DisableFailSkull { get; set; } = false;
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            if(setting.Identifier == nameof(ExpeditionResultsScreenTweaksSettings.DisableSuccessMask))
            {
                SetSuccessMaskActive(!Settings.DisableSuccessMask);
            }
            if (setting.Identifier == nameof(ExpeditionResultsScreenTweaksSettings.DisableFailSkull))
            {
                SetFailMaskActive(!Settings.DisableFailSkull);
            }
        }

        public override void OnEnable()
        {
            if(Settings.DisableSuccessMask)
                SetSuccessMaskActive(false);
            if (Settings.DisableFailSkull)
                SetFailMaskActive(false);
            ShowTimerOnExpdFailScreen(true);
        }

        public override void OnDisable()
        {
            SetSuccessMaskActive(true);
            SetFailMaskActive(true);
            ShowTimerOnExpdFailScreen(false);
        }

        private static IStaticValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer> A_MainMenuGuiLayer_Current;

        public override void Init()
        {
            A_MainMenuGuiLayer_Current = AccessorBase.GetStaticValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer>("Current");
        }

        public const string EXPEDITION_TIMER_GO_NAME = "ExpeditionTimer";
        public const string EXPEDITION_TIMER_UNVERIFIED = "<#aaa>(Unverified)</color>";

        private static void ShowTimerOnExpdFailScreen(bool show, CM_PageExpeditionFail fail = null)
        {
            var instance = fail ?? A_MainMenuGuiLayer_Current.GetStaticValue()?.PageExpeditionFail;

            if (instance == null)
                return;

            var expdTimerTrans = instance.m_missionFailed_text.transform.parent.GetChildWithExactName(EXPEDITION_TIMER_GO_NAME);

            var expdTimerTMP = expdTimerTrans?.GetComponent<TMPro.TextMeshPro>();

            if (expdTimerTrans == null)
            {
                expdTimerTMP = GameObject.Instantiate(instance.m_missionFailed_text, instance.m_missionFailed_text.transform.parent);
                expdTimerTMP.name = EXPEDITION_TIMER_GO_NAME;
                expdTimerTrans = expdTimerTMP.transform;
                expdTimerTrans.position += new Vector3(0, 130, 0);
            }

            expdTimerTMP.gameObject.SetActive(false);

            if (show && Settings.ShowTimerOnFailScreen)
            {
                Loader.LoaderWrapper.StartCoroutine(ShowTimerDelayed(expdTimerTMP));
            }
        }

        private static IEnumerator ShowTimerDelayed(TMPro.TextMeshPro tmp)
        {
            yield return new WaitForSeconds(1f);

            string time;
            if (FeatureManager.IsFeatureEnabled(nameof(Backport.EnhancedExpeditionTimer)))
            {
                time = Backport.EnhancedExpeditionTimer.TotalElapsedMissionTimeFormatted;
                if (!Backport.EnhancedExpeditionTimer.IsTimerAccurate)
                {
                    time = $"{time} {EXPEDITION_TIMER_UNVERIFIED}";
                }
            }
            else
            {
                time = GetFallbackExpeditionTime();
                if (string.IsNullOrWhiteSpace(time))
                    time = "<size=20%>No timer available, enable 'EnhancedExpeditionTimer' or disable 'ResultsScreenTweaks > Add Failed Expedition Timer' in mod settings.</size>";
            }
            tmp.text = $"<size=30%>{time}</size>";
            Dev.ModSettings.JankTextMeshProUpdaterOnce.UpdateMesh(tmp);

            yield return CoroutineManager.BlinkIn(tmp.gameObject, 0f);
            yield break;
        }

        [ArchivePatch(typeof(CM_PageExpeditionFail), nameof(UnityMessages.OnEnable))]
        internal static class CM_PageExpeditionFail_OnEnable_Patch
        {
            public static void Postfix(CM_PageExpeditionFail __instance)
            {
                ShowTimerOnExpdFailScreen(true, __instance);
            }
        }

        [ArchivePatch(typeof(CM_PageExpeditionSuccess), nameof(UnityMessages.OnEnable))]
        internal static class CM_PageExpeditionSuccess_OnEnable_Patch
        {
            public static void Postfix(CM_PageExpeditionSuccess __instance)
            {
                if (!Settings.OverrideEnhancedTimerOnSuccessScreen)
                    return;

                try
                {
                    var text = __instance.m_expeditionName.text;

                    var parts = text.Split('-');

                    string time;

                    if (FeatureManager.IsFeatureEnabled(nameof(Backport.EnhancedExpeditionTimer)))
                    {
                        time = Backport.EnhancedExpeditionTimer.TotalElapsedMissionTimeFormatted;
                        if (!Backport.EnhancedExpeditionTimer.IsTimerAccurate)
                        {
                            time = $"{time} {EXPEDITION_TIMER_UNVERIFIED}";
                        }
                    }
                    else
                    {
                        time = GetFallbackExpeditionTime();
                        if (string.IsNullOrWhiteSpace(time))
                            time = "<size=20%>No timer available, enable 'EnhancedExpeditionTimer' or disable 'ResultsScreenTweaks > Success Expedition Timer' in mod settings.</size>";
                    }

                    string extraForOldBuilds = string.Empty;
                    if (Is.R1OrLater && !Is.R4OrLater)
                    {
                        extraForOldBuilds = "<color=#ccc><size=50%> ";
                    }

                    var newText = $"{parts[0]}{extraForOldBuilds}- {time}</size></color>";
                    __instance.m_expeditionName.text = newText;
                }
                catch(Exception ex)
                {
                    FeatureLogger.Exception(ex);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetFallbackExpeditionTime()
        {
#if IL2CPP
            return TimeSpan.FromSeconds(Clock.ExpeditionProgressionTime + Clock.ExpeditionCheckpointWastedTime).ToString("hh':'mm':'ss");
#else
            return string.Empty;
#endif
        }

        public static void SetSuccessMaskActive(bool active) => SetMaskBackgroundActive(A_MainMenuGuiLayer_Current.Get(null)?.PageExpeditionSuccess?.transform, active);

        public static void SetFailMaskActive(bool active) => SetMaskBackgroundActive(A_MainMenuGuiLayer_Current.Get(null)?.PageExpeditionFail?.transform, active);

        public static void SetMaskBackgroundActive(Transform trans, bool active)
        {
            trans?.GetChildWithExactName("Backgrounds")?.GetChildWithExactName("Middle")?.gameObject?.SetActive(active);
        }

        [ArchivePatch(typeof(CM_PageExpeditionSuccess), nameof(CM_PageExpeditionSuccess.Setup))]
        internal static class CM_PageExpeditionSuccess_Setup_Patch
        {
            public static void Postfix(CM_PageExpeditionSuccess __instance)
            {
                if (Settings.DisableSuccessMask)
                    SetMaskBackgroundActive(__instance.transform, false);
            }
        }

        [ArchivePatch(typeof(CM_PageExpeditionFail), nameof(CM_PageExpeditionFail.Setup))]
        internal static class CM_PageExpeditionFail_Setup_Patch
        {
            public static void Postfix(CM_PageExpeditionFail __instance)
            {
                if (Settings.DisableFailSkull)
                    SetMaskBackgroundActive(__instance.transform, false);
            }
        }
    }
}
