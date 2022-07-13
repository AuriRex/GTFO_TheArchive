using System;
using System.Collections.Generic;
using TheArchive.Utilities;

namespace TheArchive.Models.Progression
{
    public class ExpeditionSession
    {
        private ExpeditionSessionData SavedData { get; set; } = null;
        public ExpeditionSessionData CurrentData { get; private set; } = new ExpeditionSessionData();
        public bool HasCheckpointBeenUsed { get; private set; } = false;
        public bool ExpeditionSurvived { get; private set; } = false;
        public DateTimeOffset StartTime { get; private set; }
        public DateTimeOffset EndTime { get; private set; }

        public string SessionId { get; private set; } = string.Empty;

        private ExpeditionSession(string sessionId)
        {
            SessionId = sessionId;
            StartTime = DateTimeOffset.UtcNow;

            SetLayer(Layers.Main, LayerState.Entered);
        }

        public static ExpeditionSession InitNewSession(string sessionId)
        {
            var session = new ExpeditionSession(sessionId);

            ArchiveLogger.Info($"[{nameof(ExpeditionSession)}] New expedition session started! ({sessionId})");
            
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

            ArchiveLogger.Info($"[{nameof(ExpeditionSession)}] Expedition session has ended! ({SessionId}){(success ? " Expedition Successful!" : string.Empty)}");

            if (!success) return;

            ExpeditionSurvived = true;
            SetLayer(Layers.Main, LayerState.Completed);
        }

        public void SetLayer(Layers layer, LayerState state)
        {
            CurrentData.SetOnlyIncreaseLayerState(layer, state);
        }

        public void DiscoverLayer(Layers layer)
        {
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

            public void SetOnlyIncreaseLayerState(Layers layer, LayerState state)
            {
                if (LayerStates.TryGetValue(layer, out var currentState))
                {
                    if((int)currentState < (int)state)
                    {
                        LayerStates.Remove(layer);
                        ArchiveLogger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} from {currentState} to {state}");
                        LayerStates.Add(layer, state);
                    }
                    return;
                }

                ArchiveLogger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} to {state}");
                LayerStates.Add(layer, state);
            }

            public void SetLayerState(Layers layer, LayerState state)
            {
                if(LayerStates.TryGetValue(layer, out var currentState))
                {
                    LayerStates.Remove(layer);
                    ArchiveLogger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} from {currentState} to {state}");
                }
                else
                {
                    ArchiveLogger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} to {state}");
                }
                LayerStates.Add(layer, state);
            }

            public ExpeditionSessionData Clone()
            {
                var newExpSD = new ExpeditionSessionData();

                foreach(var kvp in LayerStates)
                {
                    newExpSD.LayerStates.Add(kvp.Key, kvp.Value);
                }

                return newExpSD;
            }
        }
    }
}
