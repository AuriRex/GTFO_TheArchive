using DropServer.BoosterImplants;
using System;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using static TheArchive.Loader.LoaderWrapper;
using static TheArchive.Models.Boosters.LocalBoosterImplant;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Impl.R6.Boosters;

[RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
public class CustomBoosterImplantPlayerDataConverter : IBaseGameConverter<LocalBoosterImplantPlayerData>
{
    private static bool _reflectionInitDone = false;
    private static PropertyInfo Basic;
    private static PropertyInfo Advanced;
    private static PropertyInfo Specialized;

    private static void ReflectionInit()
    {
        if (_reflectionInitDone) return;
        Basic = typeof(BoosterImplantPlayerData).GetProperty(nameof(BoosterImplantPlayerData.Basic), AnyBindingFlagss);
        Advanced = typeof(BoosterImplantPlayerData).GetProperty(nameof(BoosterImplantPlayerData.Advanced), AnyBindingFlagss);
        Specialized = typeof(BoosterImplantPlayerData).GetProperty(nameof(BoosterImplantPlayerData.Specialized), AnyBindingFlagss);
        _reflectionInitDone = true;
    }

    public LocalBoosterImplantPlayerData FromBaseGame(object baseGame, LocalBoosterImplantPlayerData existingCBIP = null)
    {
        var data = (BoosterImplantPlayerData)baseGame;

        var customData = existingCBIP ?? new LocalBoosterImplantPlayerData();

        customData.Basic = LocalBoosterImplantPlayerData.CustomCategory.FromBaseGame(data.Basic);
        customData.Basic.CategoryType = A_BoosterImplantCategory.Muted;
        customData.Advanced = LocalBoosterImplantPlayerData.CustomCategory.FromBaseGame(data.Advanced);
        customData.Advanced.CategoryType = A_BoosterImplantCategory.Bold;
        customData.Specialized = LocalBoosterImplantPlayerData.CustomCategory.FromBaseGame(data.Specialized);
        customData.Specialized.CategoryType = A_BoosterImplantCategory.Aggressive;

        customData.New = new uint[data.New.Count];
        for (int i = 0; i < data.New.Count; i++)
        {
            customData.New[i] = data.New[i];
        }

        return customData;
    }

    public Type GetBaseGameType() => typeof(BoosterImplantPlayerData);

    public Type GetCustomType() => typeof(LocalBoosterImplantPlayerData);

    public object ToBaseGame(LocalBoosterImplantPlayerData customBoosterImplantPlayerData, object existingBaseGame = null)
    {
        ReflectionInit();

        var bipd = (BoosterImplantPlayerData)existingBaseGame ?? new BoosterImplantPlayerData(ClassInjector.DerivedConstructorPointer<BoosterImplantPlayerData>());

        var basic = (BoosterImplantPlayerData.Category)customBoosterImplantPlayerData.Basic.ToBaseGame();
        var advanced = (BoosterImplantPlayerData.Category)customBoosterImplantPlayerData.Advanced.ToBaseGame();
        var specialized = (BoosterImplantPlayerData.Category)customBoosterImplantPlayerData.Specialized.ToBaseGame();

        Basic.SetValue(bipd, basic);
        Advanced.SetValue(bipd, advanced);
        Specialized.SetValue(bipd, specialized);

        bipd.New = customBoosterImplantPlayerData.New;

        return bipd;
    }
}