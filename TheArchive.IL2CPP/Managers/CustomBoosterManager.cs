using DropServer;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using TheArchive.Core;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using TheArchive.Utilities;

namespace TheArchive.Managers
{
    public class CustomBoosterManager : InitSingletonBase<CustomBoosterManager>, IInitAfterGameDataInitialized, IInitCondition
    {

        public static bool DoConsumeBoosters { get; set; } = true;

        private static CustomBoosterImplantPlayerData _customBoosterImplantPlayerData = null;
        public static CustomBoosterImplantPlayerData CustomBoosterImplantPlayerData
        {
            get
            {
                if(_customBoosterImplantPlayerData == null)
                {
                    // Load from disk
                    try
                    {
                        _customBoosterImplantPlayerData = LoadFromBoosterFile();
                    }
                    catch(FileNotFoundException)
                    {
                        _customBoosterImplantPlayerData = new CustomBoosterImplantPlayerData();
                    }
                }

                return _customBoosterImplantPlayerData;
            }
        }

        private readonly static uint[] _noBoosterIds = new uint[0];
        private static uint[] _boostersToBeConsumed = null;
        public static uint[] BoostersToBeConsumed
        {
            get
            {
                if (_boostersToBeConsumed == null)
                    _boostersToBeConsumed = _noBoosterIds;
                return _boostersToBeConsumed;
            }
            private set
            {
                _boostersToBeConsumed = value;
            }
        }


        public CustomBoosterDropper BoosterDropper => CustomBoosterDropper.Instance;

        public bool InitCondition()
        {
            return ArchiveMod.CurrentBuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownFive.ToLatest());
        }

        public void Init()
        {
            Instance = this;
        }

        internal void SaveBoostersToDisk()
        {
            SaveToBoosterFile(CustomBoosterImplantPlayerData);
        }

        public object UpdateBoosterImplantPlayerData(object transaction)
        {
            var customTrans = CustomBoosterTransaction.FromBaseGame(transaction);
            return UpdateBoosterImplantPlayerData(customTrans);
        }

        // called everytime a new booster is selected for the first time to update the value / missed boosters are aknowledged / a booster has been dropped
        public object UpdateBoosterImplantPlayerData(CustomBoosterTransaction transaction) // returns basegame BoosterImplantPlayerData
        {
            if (transaction.DropIds != null)
                CustomBoosterImplantPlayerData.DropBoostersWithIds(transaction.DropIds.ToArray());

            if (transaction.TouchIds != null)
                CustomBoosterImplantPlayerData.SetBoostersTouchedWithIds(transaction.TouchIds.ToArray());

            if (transaction.AcknowledgeIds != null)
                CustomBoosterImplantPlayerData.AcknowledgeBoostersWithIds(transaction.AcknowledgeIds.ToArray());

            if (transaction.AcknowledgeMissed != null)
                CustomBoosterImplantPlayerData.AcknowledgeMissedBoostersWithIds(transaction.AcknowledgeMissed);

            SaveBoostersToDisk();
            return CustomBoosterImplantPlayerData.ToBaseGame();
        }

        public object GetBoosterImplantPlayerData(uint maxBackendTemplateId) // returns basegame BoosterImplantPlayerData
        {
            SaveBoostersToDisk();
            return CustomBoosterImplantPlayerData.ToBaseGame();
        }

        public void ConsumeBoosters(string sessionBlob)
        {
            if(DoConsumeBoosters)
            {
                // remove boosters from the file & save

                CustomBoosterImplantPlayerData.ConsumeBoostersWithIds(BoostersToBeConsumed);

                SaveBoostersToDisk();
            }

            // clear used boosters list or something
            BoostersToBeConsumed = _noBoosterIds;
        }

        public void EndSession(EndSessionRequest.PerBoosterCategoryInt boosterCurrency, bool success, string sessionBlob, uint maxBackendBoosterTemplateId, int buildRev)
        {
            CustomBoosterImplantPlayerData.AddCurrency(boosterCurrency);

            // Test if currency exceeds 1000, remove that amount and generate & add a new randomly created booster
            // or add 1 to the Missed Counter if inventory is full (10 items max)
            var cats = CustomBoosterImplantPlayerData.GetCategoriesWhereCurrencyCostHasBeenReached();

            foreach(var cat in cats)
            {
                while(cat.HasEnoughCurrencyForDrop)
                {
                    cat.Currency -= CustomBoosterImplantPlayerData.CurrencyNewBoosterCost;

                    if (cat.InventoryIsFull)
                    {
                        ArchiveLogger.Warning($"Inventory full, missed 1 {cat.CategoryType} booster.");
                        cat.Missed++;
                        continue;
                    }

                    ArchiveLogger.Notice($"Generating 1 {cat.CategoryType} booster ... [CurrencyRemaining:{cat.Currency}]");
                    BoosterDropper.GenerateAndAddBooster(ref _customBoosterImplantPlayerData, cat.CategoryType);
                }
                
            }
            

            SaveBoostersToDisk();
        }

        public void StartSession(uint[] boosterIds, string sessionId)
        {
            BoostersToBeConsumed = boosterIds;
        }

        public static void SaveToBoosterFile(CustomBoosterImplantPlayerData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            ArchiveLogger.Msg(ConsoleColor.DarkRed, $"Saving boosters to disk at: {LocalFiles.BoostersPath}");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(LocalFiles.BoostersPath, json);
        }

        public static CustomBoosterImplantPlayerData LoadFromBoosterFile()
        {
            ArchiveLogger.Msg(ConsoleColor.Green, $"Loading boosters from disk at: {LocalFiles.BoostersPath}");
            if (!File.Exists(LocalFiles.BoostersPath))
                throw new FileNotFoundException();
            var json = File.ReadAllText(LocalFiles.BoostersPath);

            return JsonConvert.DeserializeObject<CustomBoosterImplantPlayerData>(json);
        }
    }
}
