using GameData;
using Newtonsoft.Json;
using System;
using System.IO;
using TheArchive.Core;
using TheArchive.Interfaces;
using TheArchive.Models;
using TheArchive.Models.Progression;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Managers
{
    public class LocalVanityItemManager : InitSingletonBase<LocalVanityItemManager>, IInitAfterDataBlocksReady, IInitCondition, IInjectLogger
    {
        public IArchiveLogger Logger { get; set; }

        private LocalVanityItemStorage _localVanityItemStorage;
        public LocalVanityItemStorage LocalVanityItemPlayerData
        {
            get
            {
                if (_localVanityItemStorage == null)
                {
                    // Load from disk
                    try
                    {
                        _localVanityItemStorage = LoadFromLocalFile();
                    }
                    catch (FileNotFoundException)
                    {
                        _localVanityItemStorage = new LocalVanityItemStorage();
                    }

                    if (_localVanityItemStorage.Items.Count == 0)
                    {
                        Dropper.DropFirstTimePlayingItems(_localVanityItemStorage);
                    }
                }

                return _localVanityItemStorage;
            }
        }

        private LocalVanityItemDropper Dropper => LocalVanityItemDropper.Instance;

        public bool InitCondition()
        {
            return ArchiveMod.CurrentBuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest());
        }

        public void Init()
        {
            LocalProgressionManager.OnExpeditionCompleted += OnExpeditionCompleted;
        }

        public void OnExpeditionCompleted(ExpeditionCompletionData data)
        {
            if (!data.WasFirstTimeCompletion) return;
            try
            {
                if (!uint.TryParse(data.RundownId.Replace("Local_", string.Empty), out var rundownId))
                {
                    Logger.Error($"[{nameof(OnExpeditionCompleted)}] Could not parse \"{data.RundownId}\"!");
                    return;
                }

                RundownDataBlock rddb = RundownDataBlock.GetBlock(rundownId);

                char tierCharacter = data.ExpeditionId[0];
                int.TryParse(data.ExpeditionId[1].ToString(), out var expeditionIndex);

                expeditionIndex--;

                var tier = Utils.GetEnumFromName<eRundownTier>($"Tier{tierCharacter}");

                OnExpeditionFirstTimeCompletion(rddb.GetExpeditionData(tier, expeditionIndex));
            }
            catch(Exception ex)
            {
                Logger.Error("Something went wrong:");
                Logger.Exception(ex);
            }
        }

        public void OnExpeditionFirstTimeCompletion(GameData.ExpeditionInTierData expeditionData)
        {
            Logger.Debug("Dropping first time completion drops (if any) ...");
            foreach(var group in expeditionData.VanityItemsDropData.Groups)
            {
                Dropper.DropRandomFromGroup(group, LocalVanityItemPlayerData);
            }
        }

        public object ProcessTransaction(DropServer.VanityItems.VanityItemServiceTransaction trans)
        {
            if(trans != null)
            {
                if(trans.AcknowledgeIds != null) AcknowledgeIds(trans.AcknowledgeIds);
                if(trans.TouchIds != null) TouchIds(trans.TouchIds);
            }

            return GetVanityItemPlayerData();
        }

        public object GetVanityItemPlayerData()
        {
            SaveToLocalFile(LocalVanityItemPlayerData);
            return LocalVanityItemPlayerData.ToBaseGame();
        }

        public void AcknowledgeIds(uint[] ids)
        {
            foreach(var id in ids)
            {
                LocalVanityItemPlayerData.SetFlag(id, LocalVanityItemStorage.VanityItemFlags.Acknowledged);
            }
        }

        public void TouchIds(uint[] ids)
        {
            foreach (var id in ids)
            {
                LocalVanityItemPlayerData.SetFlag(id, LocalVanityItemStorage.VanityItemFlags.Touched);
            }
        }

        public static void SaveToLocalFile(LocalVanityItemStorage data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Instance.Logger.Msg(ConsoleColor.DarkRed, $"Saving VanityItems to disk at: {LocalFiles.VanityItemsPath}");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(LocalFiles.VanityItemsPath, json);
        }

        public static LocalVanityItemStorage LoadFromLocalFile()
        {
            Instance.Logger.Msg(ConsoleColor.Green, $"Loading VanityItems from disk at: {LocalFiles.VanityItemsPath}");
            if (!File.Exists(LocalFiles.VanityItemsPath))
                throw new FileNotFoundException();
            var json = File.ReadAllText(LocalFiles.VanityItemsPath);

            return JsonConvert.DeserializeObject<LocalVanityItemStorage>(json);
        }
    }
}
