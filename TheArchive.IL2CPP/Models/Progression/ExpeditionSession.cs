using System;
using System.Collections.Generic;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Models.Progression
{
    public class ExpeditionSession
    {
        private ExpeditionSessionData SavedData { get; set; } = null;
        public ExpeditionSessionData CurrentData { get; private set; } = null;
        public bool HasCheckpointBeenUsed { get; private set; } = false;
        public bool ExpeditionSurvived { get; private set; } = false;
        public DateTimeOffset StartTime { get; private set; }
        public DateTimeOffset EndTime { get; private set; }

        private readonly IArchiveLogger _logger;
        public string RundownId { get; private set; } = string.Empty;
        public string ExpeditionId { get; private set; } = string.Empty;
        public string SessionId { get; private set; } = string.Empty;

        public HashSet<Layers> DiscoveredLayers { get; private set; } = new HashSet<Layers>();

        private ExpeditionSession(string rundownId, string expeditionId, string sessionId, IArchiveLogger logger)
        {
            RundownId = rundownId;
            ExpeditionId = expeditionId;
            SessionId = sessionId;
            _logger = logger;
            StartTime = DateTimeOffset.UtcNow;

            CurrentData = new ExpeditionSessionData(logger);

            SetLayer(Layers.Main, LayerState.Entered);
        }

        public static ExpeditionSession InitNewSession(string rundownId, string expeditionId, string sessionId, IArchiveLogger logger)
        {
            var session = new ExpeditionSession(rundownId, expeditionId, sessionId, logger);

            logger.Info($"[{nameof(ExpeditionSession)}] New expedition session started! (R:{rundownId}, E:{expeditionId}, S:{sessionId})");
            
            return session;
        }

        public void OnCheckpointSave()
        {
            SavedData = CurrentData.Clone();
        }

        public void OnCheckpointReset()
        {
            HasCheckpointBeenUsed = true;
            if(SavedData != null)
            {
                CurrentData = SavedData.Clone();
            }
        }

        public void OnExpeditionCompleted(bool success)
        {
            EndTime = DateTimeOffset.UtcNow;

            _logger.Info($"[{nameof(ExpeditionSession)}] Expedition session has ended! (R:{RundownId}, E:{ExpeditionId}, S:{SessionId}){(success ? " Expedition Successful!" : string.Empty)}");

            if (!success) return;

            ExpeditionSurvived = true;
            SetLayer(Layers.Main, LayerState.Completed);
        }

        public void SetLayer(Layers layer, LayerState state)
        {
            if(state != LayerState.Undiscovered)
                DiscoveredLayers.Add(layer);
            CurrentData.SetOnlyIncreaseLayerState(layer, state);
        }

        public void DiscoverLayer(Layers layer)
        {
            DiscoveredLayers.Add(layer);
            CurrentData.SetOnlyIncreaseLayerState(layer, LayerState.Discovered);
        }

        public void EnterLayer(Layers layer)
        {
            CurrentData.SetOnlyIncreaseLayerState(layer, LayerState.Entered);
        }

        public void CompleteLayer(Layers layer)
        {
            CurrentData.SetOnlyIncreaseLayerState(layer, LayerState.Completed);
        }

        public class ExpeditionSessionData
        {
            public Dictionary<Layers, LayerState> LayerStates { get; private set; } = new Dictionary<Layers, LayerState>();

            private readonly IArchiveLogger _logger;

            public ExpeditionSessionData(IArchiveLogger logger)
            {
                _logger = logger;
            }

            public void SetOnlyIncreaseLayerState(Layers layer, LayerState state)
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

            public void SetLayerState(Layers layer, LayerState state)
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
}
