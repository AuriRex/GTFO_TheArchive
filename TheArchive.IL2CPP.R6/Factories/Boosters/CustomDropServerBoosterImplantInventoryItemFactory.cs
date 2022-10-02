using System;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using static TheArchive.Loader.LoaderWrapper;

namespace TheArchive.IL2CPP.R6.Factories
{
    public class CustomDropServerBoosterImplantInventoryItemFactory : IBaseGameConverter<LocalDropServerBoosterImplantInventoryItem>
    {
        public LocalDropServerBoosterImplantInventoryItem FromBaseGame(object baseGame, LocalDropServerBoosterImplantInventoryItem existingCBII = null)
        {
            var boosterImplantInventoryItem = (DropServer.BoosterImplants.BoosterImplantInventoryItem) baseGame;

            var customInventoryItem = existingCBII ?? new LocalDropServerBoosterImplantInventoryItem();

            customInventoryItem = (LocalDropServerBoosterImplantInventoryItem) ImplementationManager.FromBaseGameConverter<LocalBoosterImplant>(baseGame, customInventoryItem);

            customInventoryItem.Flags = boosterImplantInventoryItem.Flags;

            return customInventoryItem;
        }

        public Type GetBaseGameType() => typeof(DropServer.BoosterImplants.BoosterImplantInventoryItem);

        public Type GetCustomType() => typeof(LocalDropServerBoosterImplantInventoryItem);

        public object ToBaseGame(LocalDropServerBoosterImplantInventoryItem customItem, object existingBaseGameItem = null)
        {
            var baseGameItem = new DropServer.BoosterImplants.BoosterImplantInventoryItem(ClassInjector.DerivedConstructorPointer<DropServer.BoosterImplants.BoosterImplantInventoryItem>());

            baseGameItem = (DropServer.BoosterImplants.BoosterImplantInventoryItem) ImplementationManager.ToBaseGameConverter<LocalBoosterImplant>(customItem, baseGameItem);

            baseGameItem.Flags = customItem.Flags;

            return baseGameItem;
        }
    }
}
