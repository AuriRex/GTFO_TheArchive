using System;
using System.Collections.Generic;
using System.Linq;
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
using static TheArchive.Models.Boosters.LocalBoosterImplant;

namespace TheArchive.Impl.R5.Boosters;

[RundownConstraint(Utils.RundownFlags.RundownFive)]
public class CustomBoosterImplantPlayerDataConverter : IBaseGameConverter<LocalBoosterImplantPlayerData>
{
    private Type BoosterImplantPlayerData;

    private PropertyInfo Basic; // Category
    private PropertyInfo Advanced; // Category
    private PropertyInfo Specialized; // Category

    private PropertyInfo New; // Il2CppStructArray<uint>

    private MethodInfo Il2CppStructArray_uint_OP;

    public CustomBoosterImplantPlayerDataConverter()
    {
        BoosterImplantPlayerData = ImplementationManager.FindTypeInCurrentAppDomain("DropServer.BoosterImplantPlayerData", exactMatch: true);

        Basic = BoosterImplantPlayerData.GetProperty(nameof(Basic), Utils.AnyBindingFlagss);
        Advanced = BoosterImplantPlayerData.GetProperty(nameof(Advanced), Utils.AnyBindingFlagss);
        Specialized = BoosterImplantPlayerData.GetProperty(nameof(Specialized), Utils.AnyBindingFlagss);

        New = BoosterImplantPlayerData.GetProperty(nameof(New), Utils.AnyBindingFlagss);

        Il2CppStructArray_uint_OP = typeof(Il2CppStructArray<>).MakeGenericType(typeof(uint)).GetMethod("op_Implicit", Utils.AnyBindingFlagss);
    }

    public LocalBoosterImplantPlayerData FromBaseGame(object baseGame, LocalBoosterImplantPlayerData existingCBIP = null)
    {
        var data = baseGame;

        var customData = existingCBIP ?? new LocalBoosterImplantPlayerData();

        customData.Basic = LocalBoosterImplantPlayerData.CustomCategory.FromBaseGame(Basic.GetValue(data));
        customData.Basic.CategoryType = A_BoosterImplantCategory.Muted;
        customData.Advanced = LocalBoosterImplantPlayerData.CustomCategory.FromBaseGame(Advanced.GetValue(data));
        customData.Advanced.CategoryType = A_BoosterImplantCategory.Bold;
        customData.Specialized = LocalBoosterImplantPlayerData.CustomCategory.FromBaseGame(Specialized.GetValue(data));
        customData.Specialized.CategoryType = A_BoosterImplantCategory.Aggressive;

        customData.New = ((IEnumerable<uint>) New.GetValue(data))?.ToArray() ?? Array.Empty<uint>();

        return customData;
    }

    public Type GetBaseGameType() => BoosterImplantPlayerData;

    public Type GetCustomType() => typeof(LocalBoosterImplantPlayerData);

    public object ToBaseGame(LocalBoosterImplantPlayerData customBoosterImplantPlayerData, object existingBaseGame = null)
    {
        var bipd = existingBaseGame ?? Activator.CreateInstance(BoosterImplantPlayerData);

        var basic = customBoosterImplantPlayerData.Basic.ToBaseGame();
        var advanced = customBoosterImplantPlayerData.Advanced.ToBaseGame();
        var specialized = customBoosterImplantPlayerData.Specialized.ToBaseGame();

        Basic.SetValue(bipd, basic);
        Advanced.SetValue(bipd, advanced);
        Specialized.SetValue(bipd, specialized);

        New.SetValue(bipd, Il2CppStructArray_uint_OP.Invoke(null, new object[] { customBoosterImplantPlayerData.New }));

        return bipd;
    }
}