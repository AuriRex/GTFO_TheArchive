using System;
using TheArchive.Interfaces;
using TheArchive.Managers;
using TheArchive.Models.Boosters;
using UnhollowerRuntimeLib;

namespace TheArchive.IL2CPP.R5.Factories
{
    public class CustomDropServerBoosterImplantInventoryItemFactory : IBaseGameConverter<CustomDropServerBoosterImplantInventoryItem>
    {
        public CustomDropServerBoosterImplantInventoryItem FromBaseGame(object baseGame, CustomDropServerBoosterImplantInventoryItem existingCBII = null)
        {
            var boosterImplantInventoryItem = (DropServer.BoosterImplantInventoryItem) baseGame;

            var customInventoryItem = existingCBII ?? new CustomDropServerBoosterImplantInventoryItem();

            customInventoryItem = (CustomDropServerBoosterImplantInventoryItem) ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomBoosterImplant>>().FromBaseGame(baseGame, customInventoryItem);

            customInventoryItem.Flags = boosterImplantInventoryItem.Flags;

            return customInventoryItem;
        }

        public Type GetBaseGameType() => typeof(DropServer.BoosterImplantInventoryItem);

        public Type GetCustomType() => typeof(CustomDropServerBoosterImplantInventoryItem);

        public object ToBaseGame(CustomDropServerBoosterImplantInventoryItem customItem, object existingBaseGameItem = null)
        {
            var baseGameItem = new DropServer.BoosterImplantInventoryItem(ClassInjector.DerivedConstructorPointer<DropServer.BoosterImplantInventoryItem>());

            baseGameItem = (DropServer.BoosterImplantInventoryItem) ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomBoosterImplant>>().ToBaseGame(customItem, baseGameItem);

            baseGameItem.Flags = customItem.Flags;

            return baseGameItem;
        }
    }
}
