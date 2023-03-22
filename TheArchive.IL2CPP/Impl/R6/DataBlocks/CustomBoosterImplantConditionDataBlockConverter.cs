using GameData;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using TheArchive.Utilities;

namespace TheArchive.Impl.R6.DataBlocks
{
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    public class CustomBoosterImplantConditionDataBlockConverter : IBaseGameConverter<CustomBoosterImplantConditionDataBlock>
    {
        public CustomBoosterImplantConditionDataBlock FromBaseGame(object baseGame, CustomBoosterImplantConditionDataBlock existingCT = null)
        {
            var baseBlock = (BoosterImplantConditionDataBlock)baseGame;

            var customBlock = existingCT ?? new CustomBoosterImplantConditionDataBlock();

            customBlock = (CustomBoosterImplantConditionDataBlock)ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

            customBlock.Condition = (int)baseBlock.Condition;

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
