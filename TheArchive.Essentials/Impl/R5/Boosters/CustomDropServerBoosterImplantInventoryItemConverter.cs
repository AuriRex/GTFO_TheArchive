using System;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using TheArchive.Utilities;

namespace TheArchive.Impl.R5.Boosters;

[RundownConstraint(Utils.RundownFlags.RundownFive)]
public class CustomDropServerBoosterImplantInventoryItemConverter : IBaseGameConverter<LocalDropServerBoosterImplantInventoryItem>
{
    //DropServer.BoosterImplantInventoryItem
    // extends: DropServer.BoosterImplantBase
    public Type BoosterImplantInventoryItem;

    public PropertyInfo Flags; // uint

    public CustomDropServerBoosterImplantInventoryItemConverter()
    {
        BoosterImplantInventoryItem = ImplementationManager.FindTypeInCurrentAppDomain("DropServer.BoosterImplantInventoryItem", exactMatch: true);

        Flags = BoosterImplantInventoryItem.GetProperty(nameof(Flags), Utils.AnyBindingFlagss);
    }

    public LocalDropServerBoosterImplantInventoryItem FromBaseGame(object baseGame, LocalDropServerBoosterImplantInventoryItem existingCBII = null)
    {
        var boosterImplantInventoryItem = baseGame;

        var customInventoryItem = existingCBII ?? new LocalDropServerBoosterImplantInventoryItem();

        customInventoryItem = (LocalDropServerBoosterImplantInventoryItem) ImplementationManager.FromBaseGameConverter<LocalBoosterImplant>(baseGame, customInventoryItem);

        customInventoryItem.Flags = (uint) Flags.GetValue(boosterImplantInventoryItem);

        return customInventoryItem;
    }

    public Type GetBaseGameType() => BoosterImplantInventoryItem;

    public Type GetCustomType() => typeof(LocalDropServerBoosterImplantInventoryItem);

    public object ToBaseGame(LocalDropServerBoosterImplantInventoryItem customItem, object existingBaseGameItem = null)
    {
        var baseGameItem = Activator.CreateInstance(BoosterImplantInventoryItem);

        baseGameItem = ImplementationManager.ToBaseGameConverter<LocalBoosterImplant>(customItem, baseGameItem);

        Flags.SetValue(baseGameItem, customItem.Flags);

        return baseGameItem;
    }
}