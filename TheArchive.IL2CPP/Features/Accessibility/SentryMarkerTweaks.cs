using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Accessibility
{
    public class SentryMarkerTweaks : Feature
    {
        public override string Name => "Sentry Markers";

        public override string Group => FeatureGroups.Accessibility;

        public override string Description => "Tweak how sentry markers are shown for you.";

        public override bool PlaceSettingsInSubMenu => true;

        public override bool SkipInitialOnEnable => true;

        [FeatureConfig]
        public static SentryMarkersSettings Settings { get; set; }

        public class SentryMarkersSettings
        {
            [FSDisplayName("Show sentry owner name")]
            [FSDescription("Displayes the name of the sentries owner above it.")]
            public bool DisplayPlayerNameAboveMarker { get; set; } = false;

            [FSDisplayName("Name text size")]
            [FSDescription("Adjust the size of the nametag where 1 is 100% (normal) size.")]
            [FSSlider(0.05f, 5f)]
            public float PlayerNameSize { get; set; } = 0.75f;

            [FSDisplayName("Use player color for markers")]
            [FSDescription("Colors the markers according to the players color.")]
            public bool UsePlayerColorForMarkers { get; set; } = true;

            [FSHeader("Marker Toggles")]
            [FSDisplayName("Show your own marker")]
            [FSDescription("Don't forget where you placed it down!")]
            public bool ShowOwnMarker { get; set; } = true;

            [FSDisplayName("Show other players markers")]
            public bool ShowRemoteMarkers { get; set; } = true;

            [FSDisplayName("Show bots markers")]
            public bool ShowBotMarkers { get; set; } = true;
        }

        public static Color MARKER_GREEN => new Color(0.341f, 0.576f, 0.137f);

        private static readonly eGameStateName _eGameStateName_InLevel = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.InLevel));

        private static List<SentryGunInstance> _sentryGunInstances = new List<SentryGunInstance>();

        public override void OnEnable()
        {
            if(((eGameStateName)CurrentGameState) == _eGameStateName_InLevel)
            {
                _sentryGunInstances = GameObject.FindObjectsOfType<SentryGunInstance>().ToArray().Where(sgi => sgi.gameObject.name != "SentryGunInstance").ToList();
            }

            UpdateAllKnownMarkers();
        }

        public override void OnDisable()
        {
            if (IsApplicationQuitting)
                return;

            foreach (var sgi in _sentryGunInstances)
            {
                if (sgi == null)
                    continue;

                if(sgi.IsLocallyOwned && !SharedUtils.SafeIsBot(sgi.Owner.Owner))
                {
                    SentryGunInstance_CheckIsSetup_Patch.ExtResetMarker(sgi);
                    continue;
                }

                SentryGunInstance_CheckIsSetup_Patch.ExtHideMarker(sgi);
            }
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            UpdateAllKnownMarkers();
        }

        private void UpdateAllKnownMarkers()
        {
            foreach (var sgi in _sentryGunInstances)
            {
                if (sgi == null)
                    continue;

                SentryGunInstance_CheckIsSetup_Patch.Postfix(sgi);
            }
        }

        [ArchivePatch(typeof(SentryGunInstance), "CheckIsSetup")]
        internal static class SentryGunInstance_CheckIsSetup_Patch
        {
            private static IValueAccessor<SentryGunInstance, PlaceNavMarkerOnGO> A_PlaceNavMarkerOnGO;
            
            public static void Init()
            {
                A_PlaceNavMarkerOnGO = AccessorBase.GetValueAccessor<SentryGunInstance, PlaceNavMarkerOnGO>("m_navMarkerPlacer");
            }

            public static void Postfix(SentryGunInstance __instance)
            {
                if(!_sentryGunInstances.SafeContains(__instance))
                    _sentryGunInstances.Add(__instance);

                var navMarkerPlacer = A_PlaceNavMarkerOnGO.Get(__instance);

                var snetPlayer = __instance?.Owner?.Owner;
                
                if (snetPlayer == null)
                    return;

                bool isBot = SharedUtils.SafeIsBot(snetPlayer);

                if (!Settings.ShowBotMarkers && isBot)
                {
                    HideMarker(navMarkerPlacer);
                    return;
                }

                if(!Settings.ShowRemoteMarkers && !snetPlayer.IsLocal && !isBot)
                {
                    HideMarker(navMarkerPlacer);
                    return;
                }

                if(!Settings.ShowBotMarkers && isBot)
                {
                    HideMarker(navMarkerPlacer);
                    return;
                }

                if (!Settings.ShowOwnMarker && !isBot && snetPlayer.IsLocal)
                {
                    HideMarker(navMarkerPlacer);
                    return;
                }

                var col = MARKER_GREEN;
                var name = string.Empty;

                if (Settings.DisplayPlayerNameAboveMarker)
                {
                    name = $"<size={Settings.PlayerNameSize * 100f}%>{snetPlayer.NickName}</size>";
                }

                if (Settings.UsePlayerColorForMarkers)
                {
                    col = snetPlayer.PlayerColor;
                }
                else
                {
                    name = $"<color=white>{name}</color>";
                }

                navMarkerPlacer.PlaceMarker(null);
                navMarkerPlacer.SetMarkerVisible(true);

                navMarkerPlacer.UpdateName(name, string.Empty);
                navMarkerPlacer.UpdatePlayerColor(col);
            }

            public static void ExtResetMarker(SentryGunInstance sentryGunInstance)
            {
                var navMarkerPlacer = A_PlaceNavMarkerOnGO.Get(sentryGunInstance);
                if (navMarkerPlacer == null)
                    return;
                ResetMarker(navMarkerPlacer);
            }

            public static void ExtHideMarker(SentryGunInstance sentryGunInstance)
            {
                var navMarkerPlacer = A_PlaceNavMarkerOnGO.Get(sentryGunInstance);
                if (navMarkerPlacer == null)
                    return;
                HideMarker(navMarkerPlacer);
            }

            private static void ResetMarker(PlaceNavMarkerOnGO navMarkerPlacer)
            {
                navMarkerPlacer.PlaceMarker(null);
                navMarkerPlacer.UpdateName(string.Empty, string.Empty);
                navMarkerPlacer.UpdatePlayerColor(MARKER_GREEN);
                navMarkerPlacer.SetMarkerVisible(true);
            }

            private static void HideMarker(PlaceNavMarkerOnGO navMarkerPlacer)
            {
                navMarkerPlacer.PlaceMarker(null);
                navMarkerPlacer.UpdateName(string.Empty, string.Empty);
                navMarkerPlacer.SetMarkerVisible(false);
            }
        }
    }
}
