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
    public class LocalBoosterManager : InitSingletonBase<LocalBoosterManager>, IInitAfterGameDataInitialized, IInitCondition, IInjectLogger
    {
        public IArchiveLogger Logger { get; set; }

        public static bool DoConsumeBoosters { get; set; } = true;

        private static LocalBoosterImplantPlayerData _localBoosterImplantPlayerData = null;
        public static LocalBoosterImplantPlayerData LocalBoosterImplantPlayerData => _localBoosterImplantPlayerData ??= LoadFromBoosterFile();

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


        public LocalBoosterDropper BoosterDropper => LocalBoosterDropper.Instance;

        public bool InitCondition()
        {
            return ArchiveMod.CurrentBuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownFive.ToLatest());
        }

        public void Init()
        {

        }

        internal void SaveBoostersToDisk()
        {
            SaveToBoosterFile(LocalBoosterImplantPlayerData);
        }

        public object UpdateBoosterImplantPlayerData(object transaction)
        {
            var customTrans = LocalBoosterTransaction.FromBaseGame(transaction);
            return UpdateBoosterImplantPlayerData(customTrans);
        }

        // called everytime a new booster is selected for the first time to update the value / missed boosters are aknowledged / a booster has been dropped
        public object UpdateBoosterImplantPlayerData(LocalBoosterTransaction transaction) // returns basegame BoosterImplantPlayerData
        {
            if (transaction.DropIds != null)
                LocalBoosterImplantPlayerData.DropBoostersWithIds(transaction.DropIds.ToArray());

            if (transaction.TouchIds != null)
                LocalBoosterImplantPlayerData.SetBoostersTouchedWithIds(transaction.TouchIds.ToArray());

            if (transaction.AcknowledgeIds != null)
                LocalBoosterImplantPlayerData.AcknowledgeBoostersWithIds(transaction.AcknowledgeIds.ToArray());

            if (transaction.AcknowledgeMissed != null)
                LocalBoosterImplantPlayerData.AcknowledgeMissedBoostersWithIds(transaction.AcknowledgeMissed);

            SaveBoostersToDisk();
            return LocalBoosterImplantPlayerData.ToBaseGame();
        }

        public object GetBoosterImplantPlayerData(uint maxBackendTemplateId) // returns basegame BoosterImplantPlayerData
        {
            SaveBoostersToDisk();
            return LocalBoosterImplantPlayerData.ToBaseGame();
        }

        public void ConsumeBoosters(string sessionBlob)
        {
            if(DoConsumeBoosters)
            {
                // remove boosters from the file & save

                LocalBoosterImplantPlayerData.ConsumeBoostersWithIds(BoostersToBeConsumed);

                SaveBoostersToDisk();
            }

            // clear used boosters list or something
            BoostersToBeConsumed = _noBoosterIds;
        }

        public void EndSession(EndSessionRequest.PerBoosterCategoryInt boosterCurrency, bool success, string sessionBlob, uint maxBackendBoosterTemplateId, int buildRev)
        {
            LocalBoosterImplantPlayerData.AddCurrency(boosterCurrency);

            // Test if currency exceeds 1000, remove that amount and generate & add a new randomly created booster
            // or add 1 to the Missed Counter if inventory is full (10 items max)
            var cats = LocalBoosterImplantPlayerData.GetCategoriesWhereCurrencyCostHasBeenReached();

            foreach(var cat in cats)
            {
                while(cat.HasEnoughCurrencyForDrop)
                {
                    cat.Currency -= LocalBoosterImplantPlayerData.CurrencyNewBoosterCost;

                    if (cat.InventoryIsFull)
                    {
                        Logger.Warning($"Inventory full, missed 1 {cat.CategoryType} booster.");
                        cat.Missed++;
                        continue;
                    }

                    Logger.Notice($"Generating 1 {cat.CategoryType} booster ... [CurrencyRemaining:{cat.Currency}]");
                    BoosterDropper.GenerateAndAddBooster(ref _localBoosterImplantPlayerData, cat.CategoryType);
                }
                
            }
            

            SaveBoostersToDisk();
        }

        public void StartSession(uint[] boosterIds, string sessionId)
        {
            BoostersToBeConsumed = boosterIds;
        }

        public static void SaveToBoosterFile(LocalBoosterImplantPlayerData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Instance.Logger.Msg(ConsoleColor.DarkRed, $"Saving boosters to disk at: {LocalFiles.BoostersPath}");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(LocalFiles.BoostersPath, json);
        }

        public static LocalBoosterImplantPlayerData LoadFromBoosterFile()
        {
            Instance.Logger.Msg(ConsoleColor.Green, $"Loading boosters from disk at: {LocalFiles.BoostersPath}");
            if (!File.Exists(LocalFiles.BoostersPath))
                return new LocalBoosterImplantPlayerData();
            var json = File.ReadAllText(LocalFiles.BoostersPath);

            return JsonConvert.DeserializeObject<LocalBoosterImplantPlayerData>(json);
        }
    }
}
