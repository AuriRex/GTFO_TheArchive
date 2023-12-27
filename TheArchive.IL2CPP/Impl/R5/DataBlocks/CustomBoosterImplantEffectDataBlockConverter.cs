using GameData;
using System;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using TheArchive.Utilities;

namespace TheArchive.Impl.R5.DataBlocks
{
    [RundownConstraint(Utils.RundownFlags.RundownFive)]
    public class CustomBoosterImplantEffectDataBlockConverter : IBaseGameConverter<CustomBoosterImplantEffectDataBlock>
    {
        public PropertyInfo Description;
        public PropertyInfo DescriptionNegative;
        public PropertyInfo Effect;
        public PropertyInfo PublicName;
        public PropertyInfo PublicShortName;

        public CustomBoosterImplantEffectDataBlockConverter()
        {
            Description = typeof(BoosterImplantEffectDataBlock).GetProperty(nameof(BoosterImplantEffectDataBlock.Description), Utils.AnyBindingFlagss);
            DescriptionNegative = typeof(BoosterImplantEffectDataBlock).GetProperty(nameof(BoosterImplantEffectDataBlock.DescriptionNegative), Utils.AnyBindingFlagss);
            Effect = typeof(BoosterImplantEffectDataBlock).GetProperty(nameof(BoosterImplantEffectDataBlock.Effect), Utils.AnyBindingFlagss);
            PublicName = typeof(BoosterImplantEffectDataBlock).GetProperty(nameof(BoosterImplantEffectDataBlock.PublicName), Utils.AnyBindingFlagss);
            PublicShortName = typeof(BoosterImplantEffectDataBlock).GetProperty(nameof(BoosterImplantEffectDataBlock.PublicShortName), Utils.AnyBindingFlagss);
        }

        public CustomBoosterImplantEffectDataBlock FromBaseGame(object baseGame, CustomBoosterImplantEffectDataBlock existingCT = null)
        {
            var baseBlock = (BoosterImplantEffectDataBlock)baseGame;

            var customBlock = existingCT ?? new CustomBoosterImplantEffectDataBlock();

            customBlock = (CustomBoosterImplantEffectDataBlock)ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

            customBlock.BoosterEffectCategory = (int)baseBlock.BoosterEffectCategory;

            customBlock.Description = Description.GetValue(baseBlock).ToString(); // baseBlock.DisplayDescription;
            customBlock.DescriptionNegative = DescriptionNegative.GetValue(baseBlock).ToString(); // baseBlock.DescriptionNegative;

            customBlock.Effect = (int)Effect.GetValue(baseBlock); //baseBlock.Effect;

            customBlock.PublicName = PublicName.GetValue(baseBlock).ToString(); // baseBlock.PublicName;
            customBlock.PublicShortName = PublicShortName.GetValue(baseBlock).ToString(); // baseBlock.PublicShortName;

            return customBlock;
        }

        public Type GetBaseGameType() => typeof(BoosterImplantEffectDataBlock);

        public Type GetCustomType() => typeof(CustomBoosterImplantEffectDataBlock);

        public object ToBaseGame(CustomBoosterImplantEffectDataBlock customType, object existingBaseGame = null) => throw new NotImplementedException();
    }
}
