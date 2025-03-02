using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Interfaces;

namespace TheArchive.Models.Progression;

public class ExpeditionSession
{
    private ExpeditionSessionData SavedData { get; set; } = null;
    public ExpeditionSessionData CurrentData { get; private set; } = null;
    public bool HasCheckpointBeenUsed { get; private set; } = false;
    public bool ExpeditionSurvived { get; private set; } = false;
    public DateTimeOffset DropTime { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }

    private readonly IArchiveLogger _logger;
    public string RundownId { get; private set; } = string.Empty;
    public string ExpeditionId { get; private set; } = string.Empty;
    public string SessionId { get; private set; } = string.Empty;

    public int ArtifactsCollected { get; internal set; } = 0;

    public bool PrisonerEfficiencyCompleted
    {
        get
        {
            // PrisonerEfficiency => All 3 objectives (Main, Extreme, Overload) have been completed
            return CurrentData.LayerStates.Count() == 3
                   && CurrentData.LayerStates.All(x => x.Value == LayerState.Completed);
        }
    }

    private ExpeditionSession(string rundownId, string expeditionId, string sessionId, IArchiveLogger logger)
    {
        RundownId = rundownId;
        ExpeditionId = expeditionId;
        SessionId = sessionId;
        _logger = logger;
        DropTime = DateTimeOffset.UtcNow;

        CurrentData = new ExpeditionSessionData(logger);

        SetLayer(Layers.Main, LayerState.Entered);
    }

    internal static ExpeditionSession InitNewSession(string rundownId, string expeditionId, string sessionId, IArchiveLogger logger)
    {
        var session = new ExpeditionSession(rundownId, expeditionId, sessionId, logger);

        logger.Info($"[{nameof(ExpeditionSession)}] New expedition session started! (R:{rundownId}, E:{expeditionId}, S:{sessionId})");
            
        return session;
    }

    internal void OnLevelEntered()
    {
        StartTime = DateTimeOffset.UtcNow;
    }

    internal void OnCheckpointSave()
    {
        _logger.Info($"Saving current {nameof(ExpeditionSessionData)} at checkpoint.");
        SavedData = CurrentData.Clone();
    }

    internal void OnCheckpointReset()
    {
        if (!HasCheckpointBeenUsed)
            _logger.Notice("Checkpoint has been used!");
        HasCheckpointBeenUsed = true;
        if(SavedData != null)
        {
            _logger.Info($"Resetting previous {nameof(ExpeditionSessionData)} from checkpoint.");
            CurrentData = SavedData.Clone();
        }
    }

    internal void OnExpeditionCompleted(bool success)
    {
        EndTime = DateTimeOffset.UtcNow;

        _logger.Info($"[{nameof(ExpeditionSession)}] Expedition session has ended! (R:{RundownId}, E:{ExpeditionId}, S:{SessionId}){(success ? " Expedition Successful!" : string.Empty)}");

        if (success)
        {
            ExpeditionSurvived = true;
            SetLayer(Layers.Main, LayerState.Completed);
        }

        _logger.Info($"[{nameof(ExpeditionSession)}] Data: {CurrentData}");
    }

    internal void SetLayer(Layers layer, LayerState state)
    {
        CurrentData.SetOnlyIncreaseLayerState(layer, state);
    }

    public bool HasLayerBeenCompleted(Layers layer)
    {
        if (!CurrentData.LayerStates.TryGetValue(layer, out var state)) return false;

        return state == LayerState.Completed;
    }

    public class ExpeditionSessionData
    {
        public Dictionary<Layers, LayerState> LayerStates { get; private set; } = new Dictionary<Layers, LayerState>();

        private readonly IArchiveLogger _logger;

        internal ExpeditionSessionData(IArchiveLogger logger)
        {
            _logger = logger;
        }

        internal void SetOnlyIncreaseLayerState(Layers layer, LayerState state)
        {
            if (LayerStates.TryGetValue(layer, out var currentState))
            {
                if((int)currentState < (int)state)
                {
                    LayerStates.Remove(layer);
                    _logger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} from {currentState} to {state}");
                    LayerStates.Add(layer, state);
                }
                return;
            }

            _logger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} to {state}");
            LayerStates.Add(layer, state);
        }

        internal void SetLayerState(Layers layer, LayerState state)
        {
            if(LayerStates.TryGetValue(layer, out var currentState))
            {
                LayerStates.Remove(layer);
                _logger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} from {currentState} to {state}");
            }
            else
            {
                _logger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} to {state}");
            }
            LayerStates.Add(layer, state);
        }

        public override string ToString()
        {
            string ret = string.Empty;

            foreach(var kvp in LayerStates)
            {
                ret += $"{kvp.Key}: {kvp.Value}, ";
            }

            return ret.Substring(0, ret.Length-2);
        }

        public ExpeditionSessionData Clone()
        {
            var newExpSD = new ExpeditionSessionData(_logger);

            foreach(var kvp in LayerStates)
            {
                newExpSD.LayerStates.Add(kvp.Key, kvp.Value);
            }

            return newExpSD;
        }
    }
}