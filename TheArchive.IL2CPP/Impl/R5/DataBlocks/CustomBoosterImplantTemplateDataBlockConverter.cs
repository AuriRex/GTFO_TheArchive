using GameData;
using System;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using TheArchive.Utilities;
using static TheArchive.Models.Boosters.LocalBoosterImplant;

namespace TheArchive.Impl.R5.DataBlocks
{
    [RundownConstraint(Utils.RundownFlags.RundownFive)]
    public class CustomBoosterImplantTemplateDataBlockConverter : IBaseGameConverter<CustomBoosterImplantTemplateDataBlock>
    {
        public PropertyInfo Description;
        public PropertyInfo PublicName;

        public CustomBoosterImplantTemplateDataBlockConverter()
        {
            Description = typeof(BoosterImplantTemplateDataBlock).GetProperty(nameof(BoosterImplantTemplateDataBlock.Description), Utils.AnyBindingFlagss);
            PublicName = typeof(BoosterImplantTemplateDataBlock).GetProperty(nameof(BoosterImplantTemplateDataBlock.PublicName), Utils.AnyBindingFlagss);
        }

        public CustomBoosterImplantTemplateDataBlock FromBaseGame(object baseGame, CustomBoosterImplantTemplateDataBlock existingCT = null)
        {
            var baseBlock = (BoosterImplantTemplateDataBlock)baseGame;

            var customBlock = existingCT ?? new CustomBoosterImplantTemplateDataBlock();

            customBlock = (CustomBoosterImplantTemplateDataBlock)ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

            customBlock.Conditions = baseBlock.Conditions.ToSystemList();
            customBlock.Deprecated = baseBlock.Deprecated;

            customBlock.Description = Description.GetValue(baseBlock).ToString(); // baseBlock.DisplayDescription;

            customBlock.DropWeight = baseBlock.DropWeight;
            customBlock.DurationRange = baseBlock.DurationRange;
            var efx = new List<CustomBoosterImplantTemplateDataBlock.A_BoosterImplantEffectInstance>();
            foreach (var bgEffect in baseBlock.Effects)
            {
                efx.Add(new CustomBoosterImplantTemplateDataBlock.A_BoosterImplantEffectInstance
                {
                    BoosterImplantEffect = bgEffect.BoosterImplantEffect,
                    MaxValue = bgEffect.MaxValue,
                    MinValue = bgEffect.MinValue
                });
            }
            customBlock.Effects = efx;
            customBlock.ImplantCategory = (A_BoosterImplantCategory)baseBlock.ImplantCategory;
            customBlock.MainEffectType = (int)baseBlock.MainEffectType;

            customBlock.PublicName = PublicName.GetValue(baseBlock).ToString(); // baseBlock.PublicName;

            customBlock.RandomConditions = baseBlock.RandomConditions.ToSystemList();
            var randomEffects = new List<List<CustomBoosterImplantTemplateDataBlock.A_BoosterImplantEffectInstance>>();
            foreach (var list in baseBlock.RandomEffects)
            {
                var refx = new List<CustomBoosterImplantTemplateDataBlock.A_BoosterImplantEffectInstance>();
                foreach (var bgEffect in list)
                {
                    refx.Add(new CustomBoosterImplantTemplateDataBlock.A_BoosterImplantEffectInstance
                    {
                        BoosterImplantEffect = bgEffect.BoosterImplantEffect,
                        MaxValue = bgEffect.MaxValue,
                        MinValue = bgEffect.MinValue
                    });
                }
                randomEffects.Add(refx);
            }
            customBlock.RandomEffects = randomEffects;

            return customBlock;
        }
        public Type GetBaseGameType() => typeof(BoosterImplantTemplateDataBlock);
        public Type GetCustomType() => typeof(CustomBoosterImplantTemplateDataBlock);
        public object ToBaseGame(CustomBoosterImplantTemplateDataBlock customType, object existingBaseGame = null) => throw new NotImplementedException();
    }
}
