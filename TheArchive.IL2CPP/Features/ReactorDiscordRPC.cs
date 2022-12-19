using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using static TheArchive.Utilities.PresenceFormatter;

namespace TheArchive.Features
{
    [EnableFeatureByDefault, HideInModSettings]
    public class ReactorDiscordRPC : Feature
    {
        public override string Name => nameof(ReactorDiscordRPC);

        public static new IArchiveLogger FeatureLogger { get; set; }

        private static IValueAccessor<LG_WardenObjective_Reactor, float> A_m_currentWaveProgress;
        private static IValueAccessor<LG_WardenObjective_Reactor, float> A_m_currentDuration;

        private static IValueAccessor<LG_WardenObjective_Reactor, int> A_m_currentWaveCount;
        private static IValueAccessor<LG_WardenObjective_Reactor, int> A_m_waveCountMax;

        private static IValueAccessor<LG_WardenObjective_Reactor, string> A_m_itemKey;
        private static IValueAccessor<LG_WardenObjective_Reactor, ReactorWaveData> A_m_currentWaveData;
        private static IValueAccessor<LG_WardenObjective_Reactor, pReactorState> A_m_currentState;

        public override void Init()
        {
            if(BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownOne))
            {
                A_m_currentWaveProgress = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, float>("m_currentStateProgress");
                A_m_currentDuration = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, float>("m_currentStateDuration");

                A_m_currentWaveCount = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, int>("m_currentStateCount");
                A_m_waveCountMax = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, int>("m_stateCountMax");
            }
            else
            {
                // Renamed in R2
                A_m_currentWaveProgress = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, float>("m_currentWaveProgress");
                A_m_currentDuration = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, float>("m_currentDuration");

                A_m_currentWaveCount = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, int>("m_currentWaveCount");
                A_m_waveCountMax = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, int>("m_waveCountMax");
            }

            A_m_itemKey = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, string>("m_itemKey");
            A_m_currentWaveData = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, ReactorWaveData>("m_currentWaveData");
            A_m_currentState = AccessorBase.GetValueAccessor<LG_WardenObjective_Reactor, pReactorState>("m_currentState");

            typeof(ReactorDiscordRPC).RegisterAllPresenceFormatProviders();
        }

        [PresenceFormatProvider(nameof(ReactorWaveCountMax))]
        public static int ReactorWaveCountMax => GetWaveCountMax(GetActiveReactor());

        [PresenceFormatProvider(nameof(ReactorWaveCountCurrent))]
        public static int ReactorWaveCountCurrent => GetCurrentWaveCount(GetActiveReactor());

        [PresenceFormatProvider(nameof(ReactorWaveSecondsRemaining))]
        public static float ReactorWaveSecondsRemaining => GetWaveSecondsRemaining(GetActiveReactor());

        [PresenceFormatProvider(nameof(ReactorType))]
        public static string ReactorType => GetReactorType(GetActiveReactor()).ToString();

        [PresenceFormatProvider(nameof(ReactorWaveEndTime))]
        public static long ReactorWaveEndTime => DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)GetWaveSecondsRemaining(GetActiveReactor());

        [PresenceFormatProvider(nameof(IsReactorActive))]
        public static bool IsReactorActive => TryGetActiveReactor(out _);

        [PresenceFormatProvider(nameof(IsReactorInIntro))]
        public static bool IsReactorInIntro => IsReactorInStatus(GetActiveReactor(), eReactorStatus.Startup_intro, eReactorStatus.Shutdown_intro);

        [PresenceFormatProvider(nameof(IsReactorWaveOrChaosActive))]
        public static bool IsReactorWaveOrChaosActive => IsReactorInStatus(GetActiveReactor(), eReactorStatus.Startup_intense, eReactorStatus.Shutdown_puzzleChaos);

        [PresenceFormatProvider(nameof(IsReactorAwaitingVerify))]
        public static bool IsReactorAwaitingVerify => IsReactorInStatus(GetActiveReactor(), eReactorStatus.Startup_waitForVerify, eReactorStatus.Shutdown_waitForVerify);

        [PresenceFormatProvider(nameof(IsReactorCompleted))]
        public static bool IsReactorCompleted => IsReactorInStatus(GetActiveReactor(), eReactorStatus.Startup_complete, eReactorStatus.Shutdown_complete);

        [PresenceFormatProvider(nameof(IsReactorTypeStartup))]
        public static bool IsReactorTypeStartup => GetReactorType(GetActiveReactor()) == ReactorTypes.Startup;

        [PresenceFormatProvider(nameof(IsReactorInVerifyFailState))]
        public static bool IsReactorInVerifyFailState
        {
            get
            {
                if (!TryGetReactorState(GetActiveReactor(), out var state))
                    return false;
                
                return state.verifyFailed;
            }
        }

        [PresenceFormatProvider(nameof(ReactorVerificationString))]
        public static string ReactorVerificationString
        {
            get
            {
                var codeOrTerm = GetNextCodeOrTerminalSerial(GetActiveReactor(), out var isTerminal);

                if(isTerminal)
                {
                    return $"LOG in {codeOrTerm}";
                }

                return $"Free Code: {codeOrTerm.ToUpper()}";
            }
        }

        public enum ReactorTypes
        {
            Error,
            Startup,
            Shutdown
        }

        public static string GetNextCodeOrTerminalSerial(LG_WardenObjective_Reactor reactor, out bool isTerminal)
        {
            isTerminal = false;
            if (reactor == null)
                return "Error";

            if (BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownTwo.ToLatest()))
            {
                return GetNextCodeOrTerminalR2(reactor, out isTerminal);
            }

            return reactor.CurrentStateOverrideCode;
        }

        public static string GetNextCodeOrTerminalR2(LG_WardenObjective_Reactor reactor, out bool isTerminal)
        {
            isTerminal = false;
            if (reactor == null)
                return "Error";

            var waveData = A_m_currentWaveData.Get(reactor);

            if(waveData.HasVerificationTerminal)
            {
                isTerminal = true;
                return waveData.VerificationTerminalSerial;
            }

            return reactor.CurrentStateOverrideCode;
        }

        public static bool IsReactorInStatus(LG_WardenObjective_Reactor reactor, params eReactorStatus[] statuses)
        {
            if (!TryGetReactorStatus(reactor, out var currentStatus))
                return false;

            foreach(var status in statuses)
            {
                if (status == currentStatus)
                    return true;
            }

            return false;
        }

        public static bool TryGetReactorStatus(LG_WardenObjective_Reactor reactor, out eReactorStatus status)
        {
            if (reactor == null || !TryGetReactorState(reactor, out var state))
            {
                status = eReactorStatus.Inactive_Idle;
                return false;
            }

            status = state.status;
            return true;
        }

        public static int GetCurrentWaveCount(LG_WardenObjective_Reactor reactor)
        {
            if (reactor == null)
                return -1;
            return A_m_currentWaveCount.Get(reactor);
        }

        public static int GetWaveCountMax(LG_WardenObjective_Reactor reactor)
        {
            if (reactor == null)
                return -1;
            return A_m_waveCountMax.Get(reactor);
        }

        public static float GetWaveSecondsRemaining(LG_WardenObjective_Reactor reactor)
        {
            if (reactor == null)
                return -1f;
            return (1f - A_m_currentWaveProgress.Get(reactor)) * A_m_currentDuration.Get(reactor);
        }

        public static HashSet<LG_WardenObjective_Reactor> ReactorsInLevel { get; } = new HashSet<LG_WardenObjective_Reactor>();

        private static readonly HashSet<eReactorStatus> _idleReactorStatuses = new HashSet<eReactorStatus>()
        {
            Utils.GetEnumFromName<eReactorStatus>(nameof(eReactorStatus.Inactive_Idle)),
            Utils.GetEnumFromName<eReactorStatus>(nameof(eReactorStatus.Active_Idle)),
        };

        public static ReactorTypes GetReactorType(LG_WardenObjective_Reactor reactor)
        {
            if(TryGetReactorState(reactor, out var state))
            {
                if (state.status.ToString().StartsWith("Start"))
                    return ReactorTypes.Startup;

                if (state.status.ToString().StartsWith("Shut"))
                    return ReactorTypes.Shutdown;
            }

            return ReactorTypes.Error;
        }

        public static bool TryGetReactorState(LG_WardenObjective_Reactor reactor, out pReactorState state)
        {
            if (reactor == null)
            {
                state = default;
                return false;
            }

            state = A_m_currentState.Get(reactor);
            return true;
        }

        private static int _lastFrameCount = 0;
        private static LG_WardenObjective_Reactor _lastReturnedActiveReactor;
        public static LG_WardenObjective_Reactor GetActiveReactor()
        {
            var currentFrameCount = UnityEngine.Time.frameCount;
            if (_lastFrameCount == currentFrameCount)
            {
                return _lastReturnedActiveReactor;
            }

            var currentActiveReactor = ReactorsInLevel.FirstOrDefault(reactor => {
                if (!TryGetReactorState(reactor, out var state))
                    return false;
                return !_idleReactorStatuses.Contains(state.status);
            });

            _lastFrameCount = currentFrameCount;
            _lastReturnedActiveReactor = currentActiveReactor;
            return currentActiveReactor; 
        }

        public static bool TryGetActiveReactor(out LG_WardenObjective_Reactor activeReactor)
        {
            activeReactor = GetActiveReactor();
            return activeReactor != null;
        }

        public static string GetReactorItemKey(LG_WardenObjective_Reactor reactor)
        {
            return A_m_itemKey.Get(reactor);
        }

        [ArchivePatch(typeof(LG_WardenObjective_Reactor), nameof(LG_WardenObjective_Reactor.Start))]
        internal static class LG_WardenObjective_Reactor_Start_Patch
        {
            public static void Postfix(LG_WardenObjective_Reactor __instance)
            {
                ReactorsInLevel.Add(__instance); 
                FeatureLogger.Debug($"Added Reactor to Set: {GetReactorItemKey(__instance)}");
            }
        }

        [ArchivePatch(typeof(LG_WardenObjective_Reactor), "OnDestroy")]
        internal static class LG_WardenObjective_Reactor_OnDestroy_Patch
        {
            public static void Postfix(LG_WardenObjective_Reactor __instance)
            {
                var reactorToRemove = ReactorsInLevel.FirstOrDefault(reactor => GetReactorItemKey(reactor) == GetReactorItemKey(__instance));
                ReactorsInLevel.Remove(reactorToRemove);
                FeatureLogger.Debug($"Removed Reactor from Set: {GetReactorItemKey(reactorToRemove)}");
            }
        }
    }
}
