using DropServer;
using System;
using System.Collections.Generic;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using static TheArchive.Utilities.LoaderWrapper;
using static TheArchive.Models.Boosters.LocalBoosterImplantPlayerData;

namespace TheArchive.IL2CPP.R5.Factories
{
    public class CustomCategoryFactory : IBaseGameConverter<CustomCategory>
    {
        public CustomCategory FromBaseGame(object baseGame, CustomCategory existingCC = null)
        {
            var cat = (BoosterImplantPlayerData.Category) baseGame;

            var customCat = existingCC ?? new CustomCategory();

            customCat.Currency = cat.Currency;
            customCat.Missed = cat.Missed;
            customCat.MissedAck = cat.MissedAck;

            var customItems = new List<LocalDropServerBoosterImplantInventoryItem>();
            foreach(var item in cat.Inventory)
            {
                customItems.Add(LocalDropServerBoosterImplantInventoryItem.FromBaseGame(item));
            }
            customCat.Inventory = customItems.ToArray();

            return customCat;
        }

        public Type GetBaseGameType() => typeof(BoosterImplantPlayerData.Category);

        public Type GetCustomType() => typeof(CustomCategory);

        public object ToBaseGame(CustomCategory customCat, object existingBaseGameCat = null)
        {
            var cat = (BoosterImplantPlayerData.Category) existingBaseGameCat ?? new BoosterImplantPlayerData.Category(ClassInjector.DerivedConstructorPointer<BoosterImplantPlayerData.Category>());

            cat.Inventory = new (customCat.Inventory.Length);
            for (int i = 0; i < customCat.Inventory.Length; i++)
            {
                cat.Inventory[i] = (DropServer.BoosterImplantInventoryItem) customCat.Inventory[i].ToBaseGame();
            }

            cat.Currency = customCat.Currency;
            cat.Missed = customCat.Missed;
            cat.MissedAck = customCat.MissedAck;

            return cat;
        }
    }
}
