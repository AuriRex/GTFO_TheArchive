using System;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using UnhollowerRuntimeLib;

namespace TheArchive.IL2CPP.R6.Factories
{
    public class CustomDropServerBoosterImplantInventoryItemFactory : IBaseGameConverter<CustomDropServerBoosterImplantInventoryItem>
    {
        public CustomDropServerBoosterImplantInventoryItem FromBaseGame(object baseGame, CustomDropServerBoosterImplantInventoryItem existingCBII = null)
        {
            var boosterImplantInventoryItem = (DropServer.BoosterImplants.BoosterImplantInventoryItem) baseGame;

            var customInventoryItem = existingCBII ?? new CustomDropServerBoosterImplantInventoryItem();

            customInventoryItem = (CustomDropServerBoosterImplantInventoryItem) ImplementationManager.FromBaseGameConverter<CustomBoosterImplant>(baseGame, customInventoryItem);

            customInventoryItem.Flags = boosterImplantInventoryItem.Flags;

            return customInventoryItem;
        }

        public Type GetBaseGameType() => typeof(DropServer.BoosterImplants.BoosterImplantInventoryItem);

        public Type GetCustomType() => typeof(CustomDropServerBoosterImplantInventoryItem);

        public object ToBaseGame(CustomDropServerBoosterImplantInventoryItem customItem, object existingBaseGameItem = null)
        {
            var baseGameItem = new DropServer.BoosterImplants.BoosterImplantInventoryItem(ClassInjector.DerivedConstructorPointer<DropServer.BoosterImplants.BoosterImplantInventoryItem>());

            baseGameItem = (DropServer.BoosterImplants.BoosterImplantInventoryItem) ImplementationManager.ToBaseGameConverter<CustomBoosterImplant>(customItem, baseGameItem);

            baseGameItem.Flags = customItem.Flags;

            return baseGameItem;
        }
    }
}
