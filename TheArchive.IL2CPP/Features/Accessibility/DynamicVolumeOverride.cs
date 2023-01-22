using CellMenu;
using System;
using System.Collections;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Accessibility
{
    [EnableFeatureByDefault]
    public class DynamicVolumeOverride : Feature
    {
        public override string Name => "Loud Volume Override";

        public override string Group => FeatureGroups.Accessibility;

        public override string Description => "Lower the game volume during loud sections:\n- game intro\n- elevator drop";

        public override bool PlaceSettingsInSubMenu => true;

        public static bool IsOverrideActive { get; private set; } = false;
        public static bool IsLerpActive { get; private set; } = false;

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static DynamicVolumeSettings Settings { get; set; }

        public class DynamicVolumeSettings
        {
            [FSSlider(0f, 100f)]
            [FSDisplayName("Volume Override")]
            [FSDescription("Volume percent to use during the games' intro and elevator ride sequences.")]
            public float VolumeOverride { get; set; } = 10f;

            [FSDisplayName("Volume Interpolation (Seconds)")]
            [FSDescription("The time it takes to transition from the override volume to the game settings volume.\n(Interpolation time)")]
            public float LerpTime { get; set; } = 5f;

            [FSDisplayName("Focus Change Behaviour")]
            [FSDescription($"Set what happens to audio whenever you tab out.\n\n<color=orange>{nameof(FocusChangeMode.Default)}</color> Use the games implementation\n<color=orange>{nameof(FocusChangeMode.MuteInBackground)}</color> Mute audio when not focused\n<color=orange>{nameof(FocusChangeMode.DoNothingInBackground)}</color> Keep audio on unfocus\n<color=orange>{nameof(FocusChangeMode.OverrideInBackground)}</color> Use override volume on unfocus")]
            [FSIdentifier(nameof(FocusChangeBehaviour))]
            public FocusChangeMode FocusChangeBehaviour { get; set; } = FocusChangeMode.OverrideInBackground;

            [FSDisplayName("Reset Game Volume")]
            [FSDescription("Press if for some reason the audio volume settings don't seem to be back at their default values even though they should be.")]
            public FButton OopsMySoundSomehowBrokePleaseReset => new FButton("Reset Volume");

            public enum FocusChangeMode
            {
                Default,
                MuteInBackground,
                DoNothingInBackground,
                OverrideInBackground
            }
        }

        public override void OnButtonPressed(ButtonSetting setting)
        {
            if(setting.ButtonID == nameof(DynamicVolumeSettings.OopsMySoundSomehowBrokePleaseReset))
            {
                ResetAllVolume();
                IsOverrideActive = false;
            }
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            if(setting.Identifier == nameof(DynamicVolumeSettings.FocusChangeBehaviour))
            {
                ResetAllVolume();
            }
        }

        public override void OnDisable()
        {
            ResetAllVolume();
        }

        public override void OnApplicationFocusChanged(bool focus)
        {
            if (Settings.FocusChangeBehaviour == DynamicVolumeSettings.FocusChangeMode.Default)
                return;

            if (focus)
            {
                if (IsOverrideActive)
                {
                    SetSFXVolume(Settings.VolumeOverride);
                    return;
                }
                ResetAllVolume();
            }
            else
            {
                switch(Settings.FocusChangeBehaviour)
                {
                    case DynamicVolumeSettings.FocusChangeMode.Default:
                        break;
                    case DynamicVolumeSettings.FocusChangeMode.MuteInBackground:
                        SetAllVolume(0f);
                        break;
                    case DynamicVolumeSettings.FocusChangeMode.DoNothingInBackground:
                        ResetAllVolume();
                        break;
                    case DynamicVolumeSettings.FocusChangeMode.OverrideInBackground:
                        SetAllVolume(Settings.VolumeOverride);
                        break;
                }
                return;
            }
        }

        public static float GetPlayerSFXSettings()
        {
            return CellSettingsManager.SettingsData.Audio.SFXVolume.Value * 100f;
        }

        /// <summary>
        /// 0 to 100
        /// </summary>
        /// <param name="volume"></param>
        public static void SetSFXVolume(float volume, string debug = "")
        {
            if(!string.IsNullOrWhiteSpace(debug))
                FeatureLogger.Debug($"Setting SFX volume to {volume} percent. ({debug})");
            CellSound.SetGlobalRTPCValue(AK.GAME_PARAMETERS.VOLUME_SETTING_SFX, volume);
        }

        /// <summary>
        /// 0 to 100
        /// </summary>
        /// <param name="volume"></param>
        public static void SetAllVolume(float volume)
        {
            CellSound.SetGlobalRTPCValue(AK.GAME_PARAMETERS.VOLUME_SETTING_SFX, volume);
            CellSound.SetGlobalRTPCValue(AK.GAME_PARAMETERS.VOLUME_SETTING_MUSIC, volume);
            CellSound.SetGlobalRTPCValue(AK.GAME_PARAMETERS.VOLUME_SETTING_DIALOG, volume);
        }

        public static void ResetAllVolume()
        {
            CellSound.SetGlobalRTPCValue(AK.GAME_PARAMETERS.VOLUME_SETTING_SFX, CellSettingsManager.SettingsData.Audio.SFXVolume.Value * 100f);
            CellSound.SetGlobalRTPCValue(AK.GAME_PARAMETERS.VOLUME_SETTING_MUSIC, CellSettingsManager.SettingsData.Audio.MusicVolume.Value * 100f);
            CellSound.SetGlobalRTPCValue(AK.GAME_PARAMETERS.VOLUME_SETTING_DIALOG, CellSettingsManager.SettingsData.Audio.DialogVolume.Value * 100f);
        }

        private static readonly eGameStateName _eGameStateName_Offline = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.Offline));
        private static readonly eGameStateName _eGameStateName_StopElevatorRide = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.StopElevatorRide));
        private static readonly eGameStateName _eGameStateName_InLevel = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.InLevel));

        public void OnGameStateChanged(eGameStateName state)
        {
            if(state == _eGameStateName_Offline)
            {
                IsOverrideActive = true;
                // Change volume for intro seq.
                SetSFXVolume(Settings.VolumeOverride, "OnGameStateChanged, Offline");
            }

            if(state == _eGameStateName_StopElevatorRide)
            {
                // Lerp to game settings on elevator ride stop
                LerpVolume(Settings.VolumeOverride, GetPlayerSFXSettings(), Settings.LerpTime, onDone: () =>
                {
                    IsOverrideActive = false;
                });
            }

            if (state == _eGameStateName_InLevel && !IsLerpActive)
            {
                IsOverrideActive = false;
            }
        }

        private static void LerpVolume(float from, float to, float duration, Action onDone = null)
        {
            Loader.LoaderWrapper.StartCoroutine(LerpVolumeCoroutine(from, to, duration, onDone));
        }

        private static IEnumerator LerpVolumeCoroutine(float from, float to, float duration, Action onDone = null)
        {
            if(duration <= 0)
            {
                SetSFXVolume(to, "Lerp Aborted, duration <= 0");
                yield break;
            }

            IsLerpActive = true;

            var timePassed = 0f;

            SetSFXVolume(from, "Lerp Start");

            while (timePassed < duration)
            {
                var factor = timePassed / duration;

                var volume = Mathf.Lerp(from, to, factor);

                if(Settings.FocusChangeBehaviour != DynamicVolumeSettings.FocusChangeMode.MuteInBackground
                    || IsApplicationFocused)
                    SetSFXVolume(volume);

                timePassed += Mathf.Min(Time.deltaTime, duration - timePassed);

                yield return null;
            }

            SetSFXVolume(to, "Lerp End");
            IsLerpActive = false;
            onDone?.Invoke();
            yield break;
        }

        [ArchivePatch(typeof(CM_PageRundown_New), "OnEnable")]
        internal static class CM_PageRundown_New_OnEnable_Patch
        {
            private static int _state = 0;
            public static void Prefix()
            {
                if (_state < 2)
                {
                    // Gets called once early when the gameobject is created.
                    // And a second time later once enabled for real (also everytime you switch in the menu)
                    if (_state == 1)
                    {
                        LerpVolume(Settings.VolumeOverride, GetPlayerSFXSettings(), Settings.LerpTime, onDone: () =>
                        {
                            IsOverrideActive = false;
                        });
                    }

                    _state++;
                }
            }
        }

        [ArchivePatch(typeof(ElevatorRide), nameof(ElevatorRide.StartPreReleaseSequence))]
        internal static class ElevatorRide_StartPreReleaseSequence_Patch
        {
            public static void Prefix()
            {
                IsOverrideActive = true;
                SetSFXVolume(Settings.VolumeOverride, "ElevatorRide.StartPreReleaseSequence");
            }
        }

        //private void OnApplicationFocus(bool focus)
        [RundownConstraint(Utils.RundownFlags.RundownTwo, Utils.RundownFlags.Latest)]
        [ArchivePatch(typeof(CellSettingsManager), UnityMessages.OnApplicationFocus)]
        internal static class CellSettingsManager_OnApplicationFocus_Patch
        {
            public static bool Prefix(bool focus)
            {
                if (Settings.FocusChangeBehaviour == DynamicVolumeSettings.FocusChangeMode.Default)
                    return ArchivePatch.RUN_OG;

                return ArchivePatch.SKIP_OG;
            }
        }
    }
}
