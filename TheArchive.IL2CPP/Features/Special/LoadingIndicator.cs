using Player;
using SNetwork;
using System;
using System.Collections;
using System.Diagnostics;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Loader;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Special
{
    public class LoadingIndicator : Feature
    {
        public override string Name => "Loading Indicator";

        public override string Group => FeatureGroups.Special;

        public override string Description => "Displays a little indicator that shows if other players have finished loading yet.";


        public static Color Cutscene = new Color(0.2f, 0.2f, 1);
        public static Color Loading = new Color(1, 0, 0);
        public static Color Ready = new Color(0, 1, 0);

        public override void Init()
        {
#if IL2CPP
            LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<LoadTimerInfo>();
#endif
        }

        public class LoadTimerInfo : MonoBehaviour
        {
#if IL2CPP
            public LoadTimerInfo(IntPtr ptr) : base(ptr) { }
#endif

            private readonly Stopwatch _stopwatch = new Stopwatch();

            public void StartTimer()
            {
                _stopwatch.Restart();
            }

            public TimeSpan StopTimer()
            {
                _stopwatch.Stop();
                return _stopwatch.Elapsed;
            }

            public static LoadTimerInfo GetOrAdd(GameObject go)
            {
                return go.GetComponent<LoadTimerInfo>() ?? go.AddComponent<LoadTimerInfo>();
            }
        }

        public static PlaceNavMarkerOnGO GetOrCreateMarker(SNet_Player player)
        {
            if (player == null) return null;
            if (!player.HasPlayerAgent) return null;

            var agent = player.PlayerAgent.CastTo<PlayerAgent>();

            var navMarker = agent.gameObject.GetComponent<PlaceNavMarkerOnGO>() ?? agent.gameObject.AddComponent<PlaceNavMarkerOnGO>();

            navMarker.type = PlaceNavMarkerOnGO.eMarkerType.Waypoint;
            navMarker.PlaceMarker(agent.gameObject);

            return navMarker;
        }

        public static PlaceNavMarkerOnGO SetMarkerState(SNet_Player player, LoadMarkerState state)
        {
            var navMarker = GetOrCreateMarker(player);

            if(state == LoadMarkerState.Hide)
            {
                navMarker.SetMarkerVisible(false);
                return navMarker;
            }

            Color color;
            string timerInfo = string.Empty;
            switch(state)
            {
                case LoadMarkerState.Hide:
                    navMarker.SetMarkerVisible(false);
                    return navMarker;
                case LoadMarkerState.Cutscene:
                    color = Cutscene;
                    break;
                default:
                case LoadMarkerState.Loading:
                    color = Loading;
                    LoadTimerInfo.GetOrAdd(navMarker.gameObject).StartTimer();
                    break;
                case LoadMarkerState.Ready:
                    color = Ready;
                    var timerResult = LoadTimerInfo.GetOrAdd(navMarker.gameObject).StopTimer();
                    timerInfo = $"{timerResult:hh\\:mm\\:ss\\.ffffff}";
                    break;
            }

            navMarker.UpdateName($"<#{ColorUtility.ToHtmlStringRGB(player.PlayerColor)}>{player.NickName}</color>", $"<#{ColorUtility.ToHtmlStringRGB(color)}>{state}{(string.IsNullOrWhiteSpace(timerInfo) ? string.Empty : $"\n{timerInfo}")}</color>");

            navMarker.UpdatePlayerColor(color);
            navMarker.SetMarkerVisible(true);

            return navMarker;
        }

        public enum LoadMarkerState
        {
            Cutscene,
            Loading,
            Ready,
            Hide
        }

        private static readonly eGameStateName _eGameStateName_InLevel = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.InLevel));
        private static readonly eGameStateName _eGameStateName_ReadyToStopElevatorRide = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.ReadyToStopElevatorRide));
        private static readonly eGameStateName _eGameStateName_Generating = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.Generating));

        public void OnGameStateChanged(eGameStateName state)
        {
            if(state == _eGameStateName_InLevel)
            {
                var players = SNet.Lobby?.Players;

                if (players == null)
                    return;

                foreach (var player in players)
                {
                    if (player == null) continue;
                    if (player.IsLocal) continue;
                    if (!player.HasPlayerAgent) continue;

                    SetMarkerState(player, LoadMarkerState.Hide);
                }
            }
        }

        public static uint BlinkAmount = 3;
        public static float BlinkIntervalSeconds = 0.2f;

        public static IEnumerator RemoveMarkerCoroutine(PlaceNavMarkerOnGO navMarker, float secondsDelay = 0)
        {
            if(secondsDelay > 0)
                yield return new WaitForSeconds(secondsDelay);

            for(int i = 0; i < BlinkAmount * 2; i++)
            {
                navMarker?.SetMarkerVisible(i % 2 != 0);
                yield return new WaitForSeconds(BlinkIntervalSeconds);
            }
            navMarker?.SetMarkerVisible(false);
        }

        [ArchivePatch(typeof(GameStateManager), "OnPlayerGameStateChange")]
        internal static class GameStateManager_OnPlayerGameStateChange_Patch
        {
            public static void Postfix(SNet_Player player, pGameState data)
            {
                if(data.gameState == _eGameStateName_ReadyToStopElevatorRide)
                {
                    var marker = SetMarkerState(player, LoadMarkerState.Ready);
                    LoaderWrapper.StartCoroutine(RemoveMarkerCoroutine(marker, 5));
                }
                if (data.gameState == _eGameStateName_Generating)
                {
                    SetMarkerState(player, LoadMarkerState.Loading);
                }
            }
        }

        [ArchivePatch(typeof(GuiManager), nameof(GuiManager.OnFocusStateChanged))]
        internal static class GuiManager_OnFocusStateChanged_Patch
        {
            private static eFocusState _eFocusState_InElevator;

            public static void Init()
            {
                _eFocusState_InElevator = Utils.GetEnumFromName<eFocusState>(nameof(eFocusState.InElevator));
            }

            public static void Postfix(eFocusState state)
            {
                if (state == _eFocusState_InElevator)
                {
                    GuiManager.NavMarkerLayer.SetVisible(true);
                }
            }
        }

        [ArchivePatch(typeof(ElevatorRide), nameof(ElevatorRide.StartPreReleaseSequence))]
        internal static class ElevatorRide_StartPreReleaseSequence_Patch
        {
            public static void Postfix()
            {
                var players = SNet.Lobby?.Players;

                if (players == null)
                    return;

                foreach (var player in players)
                {
                    if (player == null) continue;
                    if (player.IsLocal) continue;
                    if (!player.HasPlayerAgent) continue;
                    if (player.IsInGame) continue;

                    SetMarkerState(player, LoadMarkerState.Cutscene);
                }
            }
        }
    }
}
