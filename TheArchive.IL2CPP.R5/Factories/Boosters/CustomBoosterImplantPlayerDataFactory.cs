using DropServer;
using System;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using TheArchive.Utilities;
using UnhollowerRuntimeLib;
using static TheArchive.Models.Boosters.LocalBoosterImplant;

namespace TheArchive.IL2CPP.R5.Factories
{
    public class CustomBoosterImplantPlayerDataFactory : IBaseGameConverter<LocalBoosterImplantPlayerData>
    {
        public LocalBoosterImplantPlayerData FromBaseGame(object baseGame, LocalBoosterImplantPlayerData existingCBIP = null)
        {
            var data = (BoosterImplantPlayerData) baseGame;

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
            var bipd = (BoosterImplantPlayerData) existingBaseGame ?? new BoosterImplantPlayerData(ClassInjector.DerivedConstructorPointer<BoosterImplantPlayerData>());

            var basic = (BoosterImplantPlayerData.Category) customBoosterImplantPlayerData.Basic.ToBaseGame();
            var advanced = (BoosterImplantPlayerData.Category) customBoosterImplantPlayerData.Advanced.ToBaseGame();
            var specialized = (BoosterImplantPlayerData.Category) customBoosterImplantPlayerData.Specialized.ToBaseGame();

            Il2CppUtils.SetFieldUnsafe(bipd, basic, nameof(BoosterImplantPlayerData.Basic));
            Il2CppUtils.SetFieldUnsafe(bipd, advanced, nameof(BoosterImplantPlayerData.Advanced));
            Il2CppUtils.SetFieldUnsafe(bipd, specialized, nameof(BoosterImplantPlayerData.Specialized));

            // Throws invalid IL exception for some reason:
            // System.InvalidProgramException: Invalid IL code in DropServer.BoosterImplantPlayerData:set_Basic (DropServer.BoosterImplantPlayerData/Category): IL_0029: call      0x0a000053
            //bipd.Basic = Basic.ToBaseGame();
            //bipd.Advanced = Advanced.ToBaseGame();
            //bipd.Specialized = Specialized.ToBaseGame();
            bipd.New = customBoosterImplantPlayerData.New;

            return bipd;
        }
    }
}
