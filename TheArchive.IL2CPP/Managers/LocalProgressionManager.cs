using Newtonsoft.Json;
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

        private static LocalRundownProgression _customRundownProgression = null;
        public static LocalRundownProgression CustomRundownProgression
        {
            get
            {
                if (_customRundownProgression == null)
                {
                    try
                    {
                        _customRundownProgression = LoadFromProgressionFile();
                    }
                    catch (FileNotFoundException)
                    {
                        _customRundownProgression = new LocalRundownProgression();
                    }
                }

                return _customRundownProgression;
            }
        }

        public void Init()
        {
            Instance = this;
            Logger.Msg(ConsoleColor.Magenta, "New Progression Manager has inited!");
        }

        public void StartNewExpeditionSession(string rundownId, string expeditionId, string sessionId)
        {
            CurrentActiveSession = ExpeditionSession.InitNewSession(rundownId, expeditionId, sessionId, Logger);
        }

        public void IncreaseLayerProgression(string strLayer, string strState)
        {
            if(!Enum.TryParse<Layers>(strLayer, out var layer)
                | !Enum.TryParse<LayerState>(strState, out var state))
            {
                Logger.Error($"Either {nameof(Layers)} and/or {nameof(LayerState)} could not be parsed! ({strLayer}, {strState})");
                return;
            }

            CurrentActiveSession.SetLayer(layer, state);
        }

        public void SaveAtCheckpoint()
        {
            CurrentActiveSession.OnCheckpointSave();
        }

        public void ReloadFromCheckpoint()
        {
            CurrentActiveSession.OnCheckpointReset();
        }

        public void EndCurrentExpeditionSession(bool success)
        {
            CurrentActiveSession.OnExpeditionCompleted(success);
        }

        public static void SaveToProgressionFile(LocalRundownProgression data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Instance.Logger.Msg(ConsoleColor.DarkRed, $"Saving progression to disk at: {LocalFiles.LocalProgressionPath}");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(LocalFiles.LocalProgressionPath, json);
        }

        public static LocalRundownProgression LoadFromProgressionFile()
        {
            Instance.Logger.Msg(ConsoleColor.Green, $"Loading progression from disk at: {LocalFiles.LocalProgressionPath}");
            if (!File.Exists(LocalFiles.LocalProgressionPath))
                throw new FileNotFoundException();
            var json = File.ReadAllText(LocalFiles.LocalProgressionPath);

            return JsonConvert.DeserializeObject<LocalRundownProgression>(json);
        }
    }
}
