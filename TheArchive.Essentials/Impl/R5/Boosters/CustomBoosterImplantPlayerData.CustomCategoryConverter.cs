using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using TheArchive.Utilities;
#if Unhollower
using UnhollowerBaseLib;
#endif
#if Il2CppInterop
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif
using static TheArchive.Models.Boosters.LocalBoosterImplantPlayerData;

namespace TheArchive.Impl.R5.Boosters;

[RundownConstraint(Utils.RundownFlags.RundownFive)]
public class CustomCategoryConverter : IBaseGameConverter<CustomCategory>
{
    public Type BoosterImplantPlayerData;
    public Type BoosterImplantPlayerData_Category;

    public PropertyInfo Currency; // int
    public PropertyInfo Missed; // int
    public PropertyInfo MissedAck; // int

    public PropertyInfo Inventory; // Il2CppReferenceArray<DropServer.BoosterImplantInventoryItem>

    public Type BoosterImplantInventoryItem;

    public MethodInfo Il2CppReferenceArray_BoosterImplantInventoryItem_OP;

    public CustomCategoryConverter()
    {
        BoosterImplantPlayerData = ImplementationManager.FindTypeInCurrentAppDomain("DropServer.BoosterImplantPlayerData", exactMatch: true);
        BoosterImplantPlayerData_Category = BoosterImplantPlayerData.GetNestedType("Category", Utils.AnyBindingFlagss);

        Currency = BoosterImplantPlayerData_Category.GetProperty(nameof(Currency), Utils.AnyBindingFlagss);
        Missed = BoosterImplantPlayerData_Category.GetProperty(nameof(Missed), Utils.AnyBindingFlagss);
        MissedAck = BoosterImplantPlayerData_Category.GetProperty(nameof(MissedAck), Utils.AnyBindingFlagss);

        Inventory = BoosterImplantPlayerData_Category.GetProperty(nameof(Inventory), Utils.AnyBindingFlagss);


        BoosterImplantInventoryItem = ImplementationManager.FindTypeInCurrentAppDomain("DropServer.BoosterImplantInventoryItem", exactMatch: true);


        Il2CppReferenceArray_BoosterImplantInventoryItem_OP = typeof(Il2CppReferenceArray<>).MakeGenericType(BoosterImplantInventoryItem).GetMethod("op_Implicit", Utils.AnyBindingFlagss);
    }

    public CustomCategory FromBaseGame(object baseGame, CustomCategory existingCC = null)
    {
        var cat = baseGame;

        var customCat = existingCC ?? new CustomCategory();

        customCat.Currency = (int) Currency.GetValue(cat); // cat.Currency;
        customCat.Missed = (int)Missed.GetValue(cat); // cat.Missed;
        customCat.MissedAck = (int)MissedAck.GetValue(cat); // cat.MissedAck;

        IEnumerable categoryInventory = (IEnumerable) Inventory.GetValue(cat);
        if(categoryInventory != null)
        {
            var customItems = new List<LocalDropServerBoosterImplantInventoryItem>();
            foreach (var item in categoryInventory)
            {
                customItems.Add(LocalDropServerBoosterImplantInventoryItem.FromBaseGame(item));
            }
            customCat.Inventory = customItems.ToArray();
        }
        else
        {
            customCat.Inventory = Array.Empty<LocalDropServerBoosterImplantInventoryItem>();
        }
            

        return customCat;
    }

    public Type GetBaseGameType() => BoosterImplantPlayerData_Category;

    public Type GetCustomType() => typeof(CustomCategory);

    public object ToBaseGame(CustomCategory customCat, object existingBaseGameCat = null)
    {
        var cat = existingBaseGameCat ?? Activator.CreateInstance(BoosterImplantPlayerData_Category);

        // inv = new BoosterImplantInventoryItem[customCat.Inventory.Length]
        Array inv = (Array) Activator.CreateInstance(BoosterImplantInventoryItem.MakeArrayType(), new object[] { customCat.Inventory.Length });
            
        for (int i = 0; i < customCat.Inventory.Length; i++)
        {
            inv.SetValue(customCat.Inventory[i].ToBaseGame(), i);
        }

        // cast to Il2CppReferenceArray<BoosterImplantInventoryItem> using implicit operator and assign inventory to game type
        Inventory.SetValue(cat, Il2CppReferenceArray_BoosterImplantInventoryItem_OP.Invoke(null, new object[] { inv }));

        Currency.SetValue(cat, customCat.Currency);
        Missed.SetValue(cat, customCat.Missed);
        MissedAck.SetValue(cat, customCat.MissedAck);

        return cat;
    }
}