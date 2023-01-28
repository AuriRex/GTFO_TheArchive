using CellMenu;
using System;
using System.Collections;
using System.Diagnostics;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.SharedUtils;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    public class EnhancedExpeditionTimer : Feature
    {
        public override string Name => "Enhanced Expedition Timer";

        public override string Group => FeatureGroups.Backport;

        public override string Description => "A more accurate mission timer.";


        public static IStaticValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer> A_MainMenuGuiLayer_Current;

        public override void Init()
        {
            A_MainMenuGuiLayer_Current = AccessorBase.GetValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer>("Current") as IStaticValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer>;
        }

        public override void OnEnable()
        {
            eGameStateName currentState = (eGameStateName)CurrentGameState;
            if (currentState == _eGameStateName_InLevel
                || currentState == _eGameStateName_ExpeditionFail
                || currentState == _eGameStateName_ExpeditionSuccess)
            {
                IsTimerAccurate = false;

                if (!_expeditionTimer.IsRunning)
                    _expeditionTimer.Start();
            }

            MoveTimer(true);
        }

        public override void OnDisable()
        {
            if (_expeditionTimer.IsRunning)
                _expeditionTimer.Stop();

            MoveTimer(false);
        }

        public new static IArchiveLogger FeatureLogger { get; set; }

        private static readonly eGameStateName _eGameStateName_InLevel = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.InLevel));
        private static readonly eGameStateName _eGameStateName_ExpeditionSuccess = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.ExpeditionSuccess));
        private static readonly eGameStateName _eGameStateName_ExpeditionFail = Utils.GetEnumFromName<eGameStateName>(nameof(eGameStateName.ExpeditionFail));

        private static readonly Stopwatch _expeditionTimer = new Stopwatch();
        public static bool IsTimerActive => _expeditionTimer.IsRunning;
        public static TimeSpan ElapsedMissionTime => _expeditionTimer.Elapsed;

        public static string TotalElapsedMissionTimeFormatted => $"{ElapsedMissionTime:hh\\:mm\\:ss\\.ffffff}";

        /// <summary>
        /// False if Feature has been enabled mid run.
        /// </summary>
        public static bool IsTimerAccurate { get; private set; }

        public void OnGameStateChanged(eGameStateName state)
        {
            var previousState = (eGameStateName)PreviousGameState;

            if (state == _eGameStateName_InLevel)
            {
                if (previousState == _eGameStateName_ExpeditionFail)
                {
                    // Checkpoint was loaded, resume timer
                    FeatureLogger.Debug($"Checkpoint has been loaded, total current time spent in expedition: {TotalElapsedMissionTimeFormatted}");
                    _expeditionTimer.Start();
                    return;
                }

                FeatureLogger.Debug($"New expedition has begun, timer started!");
                IsTimerAccurate = true;
                _expeditionTimer.Reset();
                _expeditionTimer.Start();
                return;
            }

            if (state == _eGameStateName_ExpeditionSuccess || state == _eGameStateName_ExpeditionFail)
            {
                // Expedition completed or failed, pause timer.
                _expeditionTimer.Stop();
                FeatureLogger.Debug($"{state}, timer stopped.");
                FeatureLogger.Notice($"Total mission time: {TotalElapsedMissionTimeFormatted} {(!IsTimerAccurate ? "(Timer not accurate due to late start!)" : string.Empty)}");
            }
        }

#if MONO
        public static void MoveTimer(bool moveToEnhanced) { }
#endif

#if IL2CPP
        public static bool TimerHasBeenMoved { get; private set; } = false;

        public static void MoveTimer(bool moveToEnhanced, CM_PageObjectives objectives = null)
        {
            var instance = objectives ?? MainMenuGuiLayer.Current?.PageObjectives;

            if (instance == null)
                return;

            if(moveToEnhanced)
            {
                if(!TimerHasBeenMoved)
                {
                    TimerHasBeenMoved = true;
                    instance.m_expeditionTime.transform.position += new Vector3(140, 0, 0);
                }
                return;
            }

            if (TimerHasBeenMoved)
            {
                TimerHasBeenMoved = false;
                instance.m_expeditionTime.transform.position -= new Vector3(140, 0, 0);
            }
        }

        public override void LateUpdate()
        {
            if (((eGameStateName)CurrentGameState) != _eGameStateName_InLevel)
                return;

            var instance = MainMenuGuiLayer.Current.PageObjectives;

            instance.m_expeditionTime.text = TotalElapsedMissionTimeFormatted;

            Dev.ModSettings.JankTextMeshProUpdaterOnce.UpdateMesh(instance.m_expeditionTime);
        }

        [RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.Latest)]
        [ArchivePatch(typeof(CM_PageObjectives), nameof(CM_PageObjectives.Setup))]
        internal static class CM_PageObjectives_Setup_Patch
        {
            public static void Postfix(CM_PageObjectives __instance) => MoveTimer(true, __instance);
        }
#endif
    }
}
