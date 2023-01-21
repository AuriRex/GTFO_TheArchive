using System;
using System.IO;
using TheArchive.Core;
using TheArchive.Interfaces;
using TheArchive.Models;
using TheArchive.Models.Progression;
using TheArchive.Utilities;

namespace TheArchive.Managers
{
    public class LocalProgressionManager : InitSingletonBase<LocalProgressionManager>, IInitAfterGameDataInitialized, IInjectLogger
    {
        public ExpeditionSession CurrentActiveSession { get; private set; }

        public IArchiveLogger Logger { get; set; }

        public static event Action<ExpeditionCompletionData> OnExpeditionCompleted;

        public LocalRundownProgression CurrentLoadedLocalProgressionData { get; private set; } = null;

        public bool HasLocalRundownProgressionLoaded => CurrentLoadedLocalProgressionData != null;

        private string _loadedRundownKey;

        public LocalRundownProgression LoadOrGetAndSaveCurrentFile(string rundownKeyToLoad)
        {
            if (string.IsNullOrEmpty(rundownKeyToLoad))
                throw new ArgumentException(nameof(rundownKeyToLoad));

            if(!HasLocalRundownProgressionLoaded)
            {
                CurrentLoadedLocalProgressionData = LoadFromProgressionFile(rundownKeyToLoad);
                return CurrentLoadedLocalProgressionData;
            }

            if (rundownKeyToLoad == _loadedRundownKey)
                return CurrentLoadedLocalProgressionData;

            SaveToProgressionFile(CurrentLoadedLocalProgressionData);
            
            CurrentLoadedLocalProgressionData = LoadFromProgressionFile(rundownKeyToLoad);

            return CurrentLoadedLocalProgressionData;
        }

        public void Init()
        {
            Logger.Msg(ConsoleColor.Magenta, "New Progression Manager has inited!");
        }

        public void StartNewExpeditionSession(string rundownId, string expeditionId, string sessionId)
        {
            CurrentActiveSession = ExpeditionSession.InitNewSession(rundownId, expeditionId, sessionId, Logger);
        }

        public void OnLevelEntered()
        {
            CurrentActiveSession?.OnLevelEntered();
        }

        public void IncreaseLayerProgression(string strLayer, string strState)
        {
            if(!Enum.TryParse<Layers>(strLayer, out var layer)
                | !Enum.TryParse<LayerState>(strState, out var state))
            {
                Logger.Error($"Either {nameof(Layers)} and/or {nameof(LayerState)} could not be parsed! ({strLayer}, {strState})");
                return;
            }

            CurrentActiveSession?.SetLayer(layer, state);
        }

        public void SaveAtCheckpoint()
        {
            CurrentActiveSession?.OnCheckpointSave();
        }

        public void ReloadFromCheckpoint()
        {
            CurrentActiveSession?.OnCheckpointReset();
        }

        public void ArtifactCountUpdated(int count)
        {
            if (CurrentActiveSession == null) return;
            CurrentActiveSession.ArtifactsCollected = count;
            Logger.Info($"current Artifact count: {count}");
        }

        public void EndCurrentExpeditionSession(bool success)
        {
            CurrentActiveSession?.OnExpeditionCompleted(success);

            var hasCompletionData = CurrentLoadedLocalProgressionData.AddSessionResults(CurrentActiveSession, out var completionData);

            CurrentActiveSession = null;

            SaveToProgressionFile(CurrentLoadedLocalProgressionData);

            if (hasCompletionData)
            {
                Logger.Notice($"Expedition time: {completionData.RawSessionData.EndTime - completionData.RawSessionData.StartTime}");

                OnExpeditionCompleted?.Invoke(completionData);
            }
        }

        public void SaveToProgressionFile(LocalRundownProgression data)
        {
            SaveToProgressionFile(data, _loadedRundownKey, out var path);
            Instance.Logger.Msg(ConsoleColor.DarkRed, $"Saved progression file to disk at: {path}");
        }

        public static void SaveToProgressionFile(LocalRundownProgression data, string rundownKeyToSave, out string path)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(rundownKeyToSave))
                throw new InvalidOperationException(nameof(rundownKeyToSave));

            path = LocalFiles.GetLocalProgressionPathForKey(rundownKeyToSave);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public LocalRundownProgression LoadFromProgressionFile(string rundownKey)
        {
            var loadedLocalProgressionData = LoadFromProgressionFile(rundownKey, out var path, out var isNew);
            _loadedRundownKey = rundownKey;

            if (isNew)
            {
                Instance.Logger.Msg(ConsoleColor.Green, $"Created progression file at: {path}");
                SaveToProgressionFile(loadedLocalProgressionData, _loadedRundownKey, out var initialSavePath);
                Instance.Logger.Msg(ConsoleColor.DarkRed, $"Saved fresh progression file to disk at: {initialSavePath}");
            }
            else
            {
                Instance.Logger.Msg(ConsoleColor.Green, $"Loaded progression file from disk at: {path}");
            }

            return loadedLocalProgressionData;
        }

        public static LocalRundownProgression LoadFromProgressionFile(string rundownKey, out string path, out bool isNew)
        {
            path = LocalFiles.GetLocalProgressionPathForKey(rundownKey);

            if (!File.Exists(path))
            {
                isNew = true;
                return new LocalRundownProgression();
            }

            isNew = false;

            var json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<LocalRundownProgression>(json);
        }
    }
}
