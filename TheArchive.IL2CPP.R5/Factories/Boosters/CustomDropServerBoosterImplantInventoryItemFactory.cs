using System;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using static TheArchive.Utilities.LoaderWrapper;

namespace TheArchive.IL2CPP.R5.Factories
{
    public class CustomDropServerBoosterImplantInventoryItemFactory : IBaseGameConverter<LocalDropServerBoosterImplantInventoryItem>
    {
        public LocalDropServerBoosterImplantInventoryItem FromBaseGame(object baseGame, LocalDropServerBoosterImplantInventoryItem existingCBII = null)
        {
            var boosterImplantInventoryItem = (DropServer.BoosterImplantInventoryItem) baseGame;

            var customInventoryItem = existingCBII ?? new LocalDropServerBoosterImplantInventoryItem();

            customInventoryItem = (LocalDropServerBoosterImplantInventoryItem) ImplementationManager.FromBaseGameConverter<LocalBoosterImplant>(baseGame, customInventoryItem);

            customInventoryItem.Flags = boosterImplantInventoryItem.Flags;

            return customInventoryItem;
        }

        public Type GetBaseGameType() => typeof(DropServer.BoosterImplantInventoryItem);

        public Type GetCustomType() => typeof(LocalDropServerBoosterImplantInventoryItem);

        public object ToBaseGame(LocalDropServerBoosterImplantInventoryItem customItem, object existingBaseGameItem = null)
        {
            var baseGameItem = new DropServer.BoosterImplantInventoryItem(ClassInjector.DerivedConstructorPointer<DropServer.BoosterImplantInventoryItem>());

            baseGameItem = (DropServer.BoosterImplantInventoryItem) ImplementationManager.ToBaseGameConverter<LocalBoosterImplant>(customItem, baseGameItem);

            baseGameItem.Flags = customItem.Flags;

            return baseGameItem;
        }
    }
}
