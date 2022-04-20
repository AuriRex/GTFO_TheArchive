using System;
using System.Collections.Generic;
using TheArchive.Core;
using TheArchive.HarmonyPatches.Patches;
using TheArchive.Interfaces;

namespace TheArchive.Managers
{
    public class CustomProgressionManager : InitSingletonBase<CustomProgressionManager>, IInitAfterGameDataInitialized
    {
#warning TODO: Artifact Heat for R5
        public static Action<string> Logger = null;

        public ExpeditionSession CurrentActiveSession { get; private set; }

        public static event Action<ExpeditionSession> OnExpeditionCompleted;

        public void Init()
        {
            Instance = this;
        }

        public void StartNewExpeditionSession(string rundownId, string expeditionId, string sessionId)
        {
            CurrentActiveSession = new ExpeditionSession()
            {
                RundownId = rundownId,
                ExpeditionId = expeditionId,
                SessionId = sessionId
            };
            Logger?.Invoke($"New Expedition Session started: {CurrentActiveSession}");
        }

        public void SetLayeredDifficultyObjectiveState(DropServer.ExpeditionLayers layer, DropServer.LayerProgressionState layerProgressionState)
        {
            CurrentActiveSession.SetLayeredObjectiveState(layer, layerProgressionState);
        }

        public void CompleteCurrentActiveExpedition()
        {
            CurrentActiveSession.Complete();

            Logger?.Invoke($"Expedition Session Completed: {CurrentActiveSession}");

            OnExpeditionCompleted?.Invoke(CurrentActiveSession);
        }

        public class ExpeditionSession
        {
            public DateTime? CompletionTime { get; private set; } = null;
            public bool IsCompleted
            {
                get
                {
                    return CompletionTime.HasValue;
                }
            }
            public string RundownId { get; internal set; }
            public string ExpeditionId { get; internal set; }
            public string SessionId { get; internal set; }

            public Dictionary<DropServer.ExpeditionLayers, DropServer.LayerProgressionState> LatestLayeredDifficultyObjectivesStates { get; private set; } = new Dictionary<DropServer.ExpeditionLayers, DropServer.LayerProgressionState>();
            public Dictionary<DropServer.ExpeditionLayers, List<(DropServer.LayerProgressionState, DateTime)>> LayeredDifficultyObjectivesStatesHistory { get; private set; } = new Dictionary<DropServer.ExpeditionLayers, List<(DropServer.LayerProgressionState, DateTime)>>();

            public void SetLayeredObjectiveState(DropServer.ExpeditionLayers objectiveLayer, DropServer.LayerProgressionState state)
            {
                SetLayeredObjectiveHistory(objectiveLayer, state);

                if(LatestLayeredDifficultyObjectivesStates.TryGetValue(objectiveLayer, out DropServer.LayerProgressionState _))
                {
                    LatestLayeredDifficultyObjectivesStates[objectiveLayer] = state;
                    return;
                }
                LatestLayeredDifficultyObjectivesStates.Add(objectiveLayer, state);
            }

            public DropServer.ExpeditionLayerMask GetCompletedLayersMask()
            {
                var layersArray = new List<DropServer.ExpeditionLayers>();

                foreach(KeyValuePair<DropServer.ExpeditionLayers, DropServer.LayerProgressionState> kvp in LatestLayeredDifficultyObjectivesStates)
                {
                    if(kvp.Value == DropServer.LayerProgressionState.Completed)
                        layersArray.Add(kvp.Key);
                }

                return ProgressionMerger.LayerMaskFromLayers(layersArray);
            }

            private void SetLayeredObjectiveHistory(DropServer.ExpeditionLayers objectiveLayer, DropServer.LayerProgressionState state)
            {
                if (LayeredDifficultyObjectivesStatesHistory.TryGetValue(objectiveLayer, out List<(DropServer.LayerProgressionState, DateTime)> values))
                {
                    values.Add((state, DateTime.UtcNow));
                    return;
                }
                LayeredDifficultyObjectivesStatesHistory.Add(objectiveLayer, new List<(DropServer.LayerProgressionState, DateTime)>() {
                    (state, DateTime.UtcNow)
                });
            }

            public void Complete()
            {
                CompletionTime = DateTime.UtcNow;
            }

            public override string ToString()
            {
                return $"<Rundown:{RundownId}, Expedition:{ExpeditionId}, SessionId:{SessionId}>";
            }

        }

        public static class ProgressionMerger
        {

            public static void MergeIntoLocalRundownProgression()
            {
                MergeIntoLocalRundownProgression(CustomProgressionManager.Instance.CurrentActiveSession);
            }

            public static void MergeIntoLocalRundownProgression(ExpeditionSession expeditionSession)
            {

                DropServer.ExpeditionLayerMask layerMask = expeditionSession.GetCompletedLayersMask();

                DropServerPatches.CustomRundownProgression.UpdateExpeditionCompletion(expeditionSession.ExpeditionId, layerMask, layerMask == DropServer.ExpeditionLayerMask.All);

                Logger?.Invoke($"Expedition Data merged: {DropServerPatches.CustomRundownProgression}, Completed expeditions: {DropServerPatches.CustomRundownProgression.Expeditions.Count}");

                DropServerPatches.SaveLocalRundownProgression();
            }

            public static DropServer.ExpeditionLayerMask LayerMaskFromLayers(IEnumerable<DropServer.ExpeditionLayers> layersOrNull)
            {
                DropServer.ExpeditionLayerMask expeditionLayerMask = DropServer.ExpeditionLayerMask.None;
                if (layersOrNull != null)
                {
                    foreach (DropServer.ExpeditionLayers layer in layersOrNull)
                    {
                        expeditionLayerMask |= DropServer.RundownProgression.LayerMaskFromLayer(layer);
                    }
                }
                return expeditionLayerMask;
            }

        }

    }
}
