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
    public class CustomBoosterImplantEffectDataBlockConverter : IBaseGameConverter<CustomBoosterImplantEffectDataBlock>
    {
        public CustomBoosterImplantEffectDataBlock FromBaseGame(object baseGame, CustomBoosterImplantEffectDataBlock existingCT = null)
        {
            var baseBlock = (BoosterImplantEffectDataBlock)baseGame;

            var customBlock = existingCT ?? new CustomBoosterImplantEffectDataBlock();

            customBlock = (CustomBoosterImplantEffectDataBlock)ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

            customBlock.BoosterEffectCategory = (int)baseBlock.BoosterEffectCategory;

            customBlock.Description = baseBlock.Description;
            customBlock.DescriptionNegative = baseBlock.DescriptionNegative;

            customBlock.Effect = (int)baseBlock.Effect;

            customBlock.PublicName = baseBlock.PublicName;
            customBlock.PublicShortName = baseBlock.PublicShortName;

            return customBlock;
        }

        public Type GetBaseGameType() => typeof(BoosterImplantEffectDataBlock);

        public Type GetCustomType() => typeof(CustomBoosterImplantEffectDataBlock);

        public object ToBaseGame(CustomBoosterImplantEffectDataBlock customType, object existingBaseGame = null) => throw new NotImplementedException();
    }
}
