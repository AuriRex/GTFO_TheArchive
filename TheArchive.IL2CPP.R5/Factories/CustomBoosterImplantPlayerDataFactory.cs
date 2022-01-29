using DropServer;
using System;
using TheArchive.Interfaces;
using TheArchive.Models.Boosters;
using TheArchive.Utilities;
using UnhollowerRuntimeLib;

namespace TheArchive.IL2CPP.R5.Factories
{
    public class CustomBoosterImplantPlayerDataFactory : IBaseGameConverter<CustomBoosterImplantPlayerData>
    {
        public CustomBoosterImplantPlayerData FromBaseGame(object baseGame)
        {
            var data = (BoosterImplantPlayerData) baseGame;

            var customData = new CustomBoosterImplantPlayerData();

            customData.Basic = CustomBoosterImplantPlayerData.CustomCategory.FromBaseGame(data.Basic);
            customData.Basic.CategoryType = BoosterImplantCategory.Muted;
            customData.Advanced = CustomBoosterImplantPlayerData.CustomCategory.FromBaseGame(data.Advanced);
            customData.Advanced.CategoryType = BoosterImplantCategory.Bold;
            customData.Specialized = CustomBoosterImplantPlayerData.CustomCategory.FromBaseGame(data.Specialized);
            customData.Specialized.CategoryType = BoosterImplantCategory.Aggressive;

            customData.New = new uint[data.New.Count];
            for (int i = 0; i < data.New.Count; i++)
            {
                customData.New[i] = data.New[i];
            }

            return customData;
        }

        public Type GetBaseGameType() => typeof(BoosterImplantPlayerData);

        public Type GetCustomType() => typeof(CustomBoosterImplantPlayerData);

        public object ToBaseGame(CustomBoosterImplantPlayerData customBoosterImplantPlayerData, object existingBaseGame = null)
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
