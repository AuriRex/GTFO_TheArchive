using Newtonsoft.Json;
using System;
using System.IO;
using TheArchive.Core;
using TheArchive.Interfaces;
using TheArchive.Models;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Managers
{
    public class CustomVanityItemManager : InitSingletonBase<CustomVanityItemManager>, IInitAfterDataBlocksReady, IInitCondition
    {

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

        private CustomVanityItemDropper Dropper => CustomVanityItemDropper.Instance;

        public bool InitCondition()
        {
            return ArchiveMod.CurrentRundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest());
        }

        public void Init()
        {
            Instance = this;
        }

        public void OnExpeditionFirstTimeCompletion(GameData.ExpeditionInTierData expeditionData)
        {
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
            ArchiveLogger.Msg(ConsoleColor.DarkRed, $"Saving VanityItems to disk at: {LocalFiles.VanityItemsPath}");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(LocalFiles.VanityItemsPath, json);
        }

        public static LocalVanityItemStorage LoadFromLocalFile()
        {
            ArchiveLogger.Msg(ConsoleColor.Green, $"Loading VanityItems from disk at: {LocalFiles.VanityItemsPath}");
            if (!File.Exists(LocalFiles.VanityItemsPath))
                throw new FileNotFoundException();
            var json = File.ReadAllText(LocalFiles.VanityItemsPath);

            return JsonConvert.DeserializeObject<LocalVanityItemStorage>(json);
        }
    }
}
