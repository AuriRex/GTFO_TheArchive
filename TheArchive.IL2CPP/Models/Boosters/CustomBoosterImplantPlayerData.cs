using DropServer;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Interfaces;
using TheArchive.Managers;

namespace TheArchive.Models.Boosters
{
    public class CustomBoosterImplantPlayerData
    {
        public static int CurrencyNewBoosterCost { get; set; } = 1000;
        public static float CurrencyGainMultiplier { get; set; } = 1f;

        public CustomBoosterImplantPlayerData()
        {

        }

        public object ToBaseGame() => ToBaseGame(this);

        public static object ToBaseGame(CustomBoosterImplantPlayerData customData)
        {
            return ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomBoosterImplantPlayerData>>().ToBaseGame(customData);
        }

        public static CustomBoosterImplantPlayerData FromBaseGame(object BoosterImplantPlayerData)
        {
            return ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomBoosterImplantPlayerData>>().FromBaseGame(BoosterImplantPlayerData);
        }

        public void AcknowledgeMissedBoostersWithIds(CustomBoosterTransaction.CustomMissed acknowledgeMissed)
        {
            Basic.MissedAck = acknowledgeMissed.Basic;
            Advanced.MissedAck = acknowledgeMissed.Advanced;
            Specialized.MissedAck = acknowledgeMissed.Specialized;
        }

        public CustomCategory[] GetCategoriesWhereCurrencyCostHasBeenReached()
        {
            var cats = new List<CustomCategory>();

            if (Basic.Currency > CurrencyNewBoosterCost) cats.Add(Basic);
            if (Advanced.Currency > CurrencyNewBoosterCost) cats.Add(Advanced);
            if (Specialized.Currency > CurrencyNewBoosterCost) cats.Add(Specialized);

            return cats.ToArray();
        }

        public void AcknowledgeBoostersWithIds(uint[] boostersToAcknowledge)
        {
            var newNew = new List<uint>();
            foreach(var id in New)
            {
                if(boostersToAcknowledge.Any(idToAcknowledge => id == idToAcknowledge))
                {
                    continue;
                }
                newNew.Add(id);
            }
            New = newNew.ToArray();
        }

        public void SetBoostersTouchedWithIds(uint[] boostersThatWereTouched)
        {
            Basic.SetBoostersTouchedWithIds(boostersThatWereTouched);
            Advanced.SetBoostersTouchedWithIds(boostersThatWereTouched);
            Specialized.SetBoostersTouchedWithIds(boostersThatWereTouched);
        }

        public void ConsumeBoostersWithIds(uint[] boostersToBeConsumed)
        {
            Basic.ConsumeOrDropBoostersWithIds(boostersToBeConsumed);
            Advanced.ConsumeOrDropBoostersWithIds(boostersToBeConsumed);
            Specialized.ConsumeOrDropBoostersWithIds(boostersToBeConsumed);
        }

        public void DropBoostersWithIds(uint[] boostersToBeDropped)
        {
            Basic.DropBoostersWithIds(boostersToBeDropped);
            Advanced.DropBoostersWithIds(boostersToBeDropped);
            Specialized.DropBoostersWithIds(boostersToBeDropped);
        }

        public void AddCurrency(EndSessionRequest.PerBoosterCategoryInt boosterCurrency)
        {
            Basic.Currency += (int) (boosterCurrency.Basic * CurrencyGainMultiplier);
            Advanced.Currency += (int) (boosterCurrency.Advanced * CurrencyGainMultiplier);
            Specialized.Currency += (int) (boosterCurrency.Specialized * CurrencyGainMultiplier);
        }

        public bool AddBooster(CustomDropServerBoosterImplantInventoryItem newBooster)
        {
            if(!GetCategory(newBooster.Category).AddBooster(newBooster))
                return false;

            var newNew = new uint[New.Length + 1];

            for(int i = 0; i < New.Length; i++)
            {
                newNew[i] = New[i];
            }

            newNew[New.Length] = newBooster.InstanceId;

            New = newNew;

            return true;
        }

        public uint[] GetUsedIds()
        {
            var cats = GetAllCategories();

            var usedIds = new List<uint>();
            foreach (var cat in cats)
            {
                foreach(var id in cat.GetUsedIds())
                {
                    usedIds.Add(id);
                }
            }

            return usedIds.ToArray();
        }

        private CustomCategory[] GetAllCategories()
        {
            return new CustomCategory[]
            {
                Basic,
                Advanced,
                Specialized
            };
        }

        /// <summary>
        /// Muted Boosters
        /// </summary>
        public CustomCategory Basic { get; set; } = new CustomCategory(BoosterImplantCategory.Muted);
        /// <summary>
        /// Bold Boosters
        /// </summary>
        public CustomCategory Advanced { get; set; } = new CustomCategory(BoosterImplantCategory.Bold);
        /// <summary>
        /// Agressive Boosters
        /// </summary>
        public CustomCategory Specialized { get; set; } = new CustomCategory(BoosterImplantCategory.Aggressive);
        /// <summary>
        /// Array of Boosters InstanceIds that are new (in game popup in lobby screen)
        /// </summary>
        public uint[] New { get; set; } = new uint[0];

        public CustomCategory GetCategory(BoosterImplantCategory category)
        {
            switch(category)
            {
                case BoosterImplantCategory.Muted:
                    return Basic;
                case BoosterImplantCategory.Bold:
                    return Advanced;
                case BoosterImplantCategory.Aggressive:
                    return Specialized;
            }
            return null;
        }

        public class CustomCategory
        {
            public CustomCategory()
            {

            }

            public CustomCategory(BoosterImplantCategory cat)
            {
                CategoryType = cat;
            }

            public uint[] GetUsedIds()
            {
                uint[] usedIds = new uint[Inventory.Length];
                for(int i = 0; i < Inventory.Length; i++)
                {
                    usedIds[i] = Inventory[i].InstanceId;
                }
                return usedIds;
            }

            public bool AddBooster(CustomDropServerBoosterImplantInventoryItem newBooster)
            {
                if (InventoryIsFull) return false;

                var newInventory = new CustomDropServerBoosterImplantInventoryItem[Inventory.Length + 1];

                for(int i = 0; i < Inventory.Length; i++)
                {
                    newInventory[i] = Inventory[i];
                }

                newInventory[newInventory.Length - 1] = newBooster;

                Inventory = newInventory;

                return true;
            }

            public CustomDropServerBoosterImplantInventoryItem[] Inventory { get; set; } = new CustomDropServerBoosterImplantInventoryItem[0];
            /// <summary> 1000 -> 100% -> new booster </summary>
            public int Currency { get; set; } = 0;
            /// <summary> Number of missed boosters </summary>
            public int Missed { get; set; } = 0;
            /// <summary> Number of missed boosters that have been acknowledged by the player (displays missed boosters popup ingame if unequal with <see cref="Missed"/>) </summary>
            public int MissedAck { get; set; } = 0;
            
            // Helper
            public BoosterImplantCategory CategoryType { get; set; } = BoosterImplantCategory.Muted;
            
            [JsonIgnore]
            public bool InventoryIsFull
            {
                get
                {
                    return Inventory.Length >= 10;
                }
            }

            internal void ConsumeOrDropBoostersWithIds(uint[] boostersToBeConsumed)
            {
                var newInventory = new List<CustomDropServerBoosterImplantInventoryItem>();

                foreach(var item in Inventory)
                {
                    if (boostersToBeConsumed.Any(toConsumeId => item.InstanceId == toConsumeId))
                    {
                        if (item.Uses > 1)
                            item.Uses -= 1;
                        else
                            continue;
                    }

                    newInventory.Add(item);
                }

                Inventory = newInventory.ToArray();
            }

            internal void DropBoostersWithIds(uint[] boostersToBeDropped)
            {
                var newInventory = new List<CustomDropServerBoosterImplantInventoryItem>();

                foreach (var item in Inventory)
                {
                    if (boostersToBeDropped.Any(toDropId => item.InstanceId == toDropId))
                    {
                        continue;
                    }

                    newInventory.Add(item);
                }

                Inventory = newInventory.ToArray();
            }

            internal void SetBoostersTouchedWithIds(uint[] boostersThatWereTouched)
            {
                foreach (var item in Inventory)
                {
                    if (boostersThatWereTouched.Any(toTouchId => item.InstanceId == toTouchId))
                    {
                        item.IsTouched = true;
                    }
                }
            }

            public object ToBaseGame() => ToBaseGame(this);

            public static CustomCategory FromBaseGame(object baseGame)
            {
                return ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomCategory>>().FromBaseGame(baseGame);
            }

            public static object ToBaseGame(CustomCategory customCat)
            {
                return ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomCategory>>().ToBaseGame(customCat);
            }
        }
    }
}
