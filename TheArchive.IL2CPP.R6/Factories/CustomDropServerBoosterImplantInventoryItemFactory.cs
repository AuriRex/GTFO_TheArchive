using System;
using TheArchive.Interfaces;
using TheArchive.Managers;
using TheArchive.Models.Boosters;
using UnhollowerRuntimeLib;

namespace TheArchive.IL2CPP.R6.Factories
{
    public class CustomDropServerBoosterImplantInventoryItemFactory : IBaseGameConverter<CustomDropServerBoosterImplantInventoryItem>
    {
        public CustomDropServerBoosterImplantInventoryItem FromBaseGame(object baseGame)
        {
            var boosterImplantInventoryItem = (DropServer.BoosterImplants.BoosterImplantInventoryItem) baseGame;

            CustomBoosterImplant.Effect[] effects = new CustomBoosterImplant.Effect[boosterImplantInventoryItem.Effects.Length];

            for (int i = 0; i < boosterImplantInventoryItem.Effects.Length; i++)
            {
                var bgEfx = boosterImplantInventoryItem.Effects[i];
                effects[i] = new CustomBoosterImplant.Effect
                {
                    Id = bgEfx.Id,
                    Value = bgEfx.Param
                };
            }

            var customInventoryItem = new CustomDropServerBoosterImplantInventoryItem(
                boosterImplantInventoryItem.TemplateId,
                boosterImplantInventoryItem.Id,
                boosterImplantInventoryItem.UsesRemaining,
                effects,
                boosterImplantInventoryItem.Conditions
                );

            customInventoryItem.Flags = boosterImplantInventoryItem.Flags;

            return customInventoryItem;
        }

        public Type GetBaseGameType() => typeof(DropServer.BoosterImplants.BoosterImplantInventoryItem);

        public Type GetCustomType() => typeof(CustomDropServerBoosterImplantInventoryItem);

        public object ToBaseGame(CustomDropServerBoosterImplantInventoryItem customItem, object existingBaseGameItem = null)
        {
            var baseGameItem = new DropServer.BoosterImplants.BoosterImplantInventoryItem(ClassInjector.DerivedConstructorPointer<DropServer.BoosterImplants.BoosterImplantInventoryItem>());

            baseGameItem = (DropServer.BoosterImplants.BoosterImplantInventoryItem) ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomBoosterImplant>>().ToBaseGame(customItem, baseGameItem);

            baseGameItem.Flags = customItem.Flags;

            return baseGameItem;
        }
    }
}
