using CellMenu;
using System.Collections;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
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

        public override string Description => "Lower the game volume during loud sections:\n- game intro\n-elevator drop";

        public override bool PlaceSettingsInSubMenu => true;

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

        private static eGameStateName _eGameStateName_Offline = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.Offline));
        private static eGameStateName _eGameStateName_StopElevatorRide = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.StopElevatorRide));

        public void OnGameStateChanged(eGameStateName state)
        {
            if(state == _eGameStateName_Offline)
            {
                // Change volume for intro seq.
                SetSFXVolume(Settings.VolumeOverride, "OnGameStateChanged, Offline");
            }

            if(state == _eGameStateName_StopElevatorRide)
            {
                // Lerp to game settings on elevator ride stop
                LerpVolume(Settings.VolumeOverride, GetPlayerSFXSettings(), Settings.LerpTime);
            }
        }

        private static void LerpVolume(float from, float to, float duration)
        {
            Loader.LoaderWrapper.StartCoroutine(LerpVolumeCoroutine(from, to, duration));
        }

        private static IEnumerator LerpVolumeCoroutine(float from, float to, float duration)
        {
            if(duration <= 0)
            {
                SetSFXVolume(to, "Lerp Aborted, duration <= 0");
                yield break;
            }

            var timePassed = 0f;

            SetSFXVolume(from, "Lerp Start");

            while (timePassed < duration)
            {
                var factor = timePassed / duration;

                var volume = Mathf.Lerp(from, to, factor);

                SetSFXVolume(volume);

                timePassed += Mathf.Min(Time.deltaTime, duration - timePassed);

                yield return null;
            }

            SetSFXVolume(to, "Lerp End");
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
                        LerpVolume(Settings.VolumeOverride, GetPlayerSFXSettings(), Settings.LerpTime);
                        //SetSFXVolume(GetPlayerSFXSettings(), "CM_PageRundown_New.OnEnable, _state = 1");

                    _state++;
                }
            }
        }

        [ArchivePatch(typeof(ElevatorRide), nameof(ElevatorRide.StartPreReleaseSequence))]
        internal static class ElevatorRide_StartPreReleaseSequence_Patch
        {
            public static void Prefix()
            {
                SetSFXVolume(Settings.VolumeOverride, "ElevatorRide.StartPreReleaseSequence");
            }
        }
    }
}
