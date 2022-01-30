using GameData;
using System;
using TheArchive.Interfaces;
using TheArchive.Managers;
using TheArchive.Models.DataBlocks;

namespace TheArchive.IL2CPP.R5.Factories.DataBlocks
{
    public class CustomBoosterImplantConditionDataBlockFactory : IBaseGameConverter<CustomBoosterImplantConditionDataBlock>
    {
        public CustomBoosterImplantConditionDataBlock FromBaseGame(object baseGame, CustomBoosterImplantConditionDataBlock existingCT = null)
        {
            var baseBlock = (BoosterImplantConditionDataBlock) baseGame;

            var customBlock = existingCT ?? new CustomBoosterImplantConditionDataBlock();

            customBlock = (CustomBoosterImplantConditionDataBlock) ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomGameDataBlockBase>>().FromBaseGame(baseGame, customBlock);

            customBlock.Condition = baseBlock.Condition;
            customBlock.Description = baseBlock.Description;
            customBlock.PublicName = baseBlock.PublicName;
            customBlock.PublicShortName = baseBlock.PublicShortName;
            customBlock.IconPath = baseBlock.IconPath;

            return customBlock;
        }

        public Type GetBaseGameType() => typeof(BoosterImplantConditionDataBlock);

        public Type GetCustomType() => typeof(CustomBoosterImplantConditionDataBlock);

        public object ToBaseGame(CustomBoosterImplantConditionDataBlock customType, object existingBaseGame = null) => throw new NotImplementedException();
    }
}
