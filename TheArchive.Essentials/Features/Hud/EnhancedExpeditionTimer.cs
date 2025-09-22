using CellMenu;
using System;
using System.Diagnostics;
using SNetwork;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Hud;

[EnableFeatureByDefault]
public class EnhancedExpeditionTimer : Feature
{
    public override string Name => "Enhanced Expedition Timer";

    public override GroupBase Group => GroupManager.Hud;

    public override string Description => "A more accurate mission timer.";


    public static IStaticValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer> A_MainMenuGuiLayer_Current;

    public override void Init()
    {
        A_MainMenuGuiLayer_Current = AccessorBase.GetValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer>("Current") as IStaticValueAccessor<MainMenuGuiLayer, MainMenuGuiLayer>;
    }

    public override void OnEnable()
    {
        var currentState = (eGameStateName)CurrentGameState;
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
    private static TimeSpan? _stagedLateJoinTimePassed;
    private static TimeSpan? _lateJoinTimePassed;
    public static bool IsTimerActive => _expeditionTimer.IsRunning;
    public static TimeSpan ElapsedMissionTime => _lateJoinTimePassed == null ? _expeditionTimer.Elapsed : _expeditionTimer.Elapsed.Add(_lateJoinTimePassed.Value);

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
            _lateJoinTimePassed = _stagedLateJoinTimePassed;
            _stagedLateJoinTimePassed = null;
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
    internal static class CM_PageObjectives__Setup__Patch
    {
        public static void Postfix(CM_PageObjectives __instance) => MoveTimer(true, __instance);
    }

    [RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.Latest)]
    [ArchivePatch(typeof(SNet_Capture), nameof(SNet_Capture.OnReceiveBufferCompletion))]
    internal static class SNet_Capture__OnReceiveBufferCompletion__Patch
    {
        private static void Postfix(pBufferCompletion completion)
        {
            if (completion.type != eBufferType.DropIn)
            {
                return;
            }
            
            _stagedLateJoinTimePassed = new TimeSpan((long)(completion.data.progressionTime * 1000) * TimeSpan.TicksPerMillisecond);
            IsTimerAccurate = true; // (Or at least should be lol)
        }
    }
#endif
}