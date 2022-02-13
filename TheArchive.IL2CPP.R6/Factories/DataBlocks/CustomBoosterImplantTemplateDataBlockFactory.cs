﻿using GameData;
using System;
using System.Collections.Generic;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using TheArchive.Utilities;

namespace TheArchive.IL2CPP.R6.Factories.DataBlocks
{
    public class CustomBoosterImplantTemplateDataBlockFactory : IBaseGameConverter<CustomBoosterImplantTemplateDataBlock>
    {
        public CustomBoosterImplantTemplateDataBlock FromBaseGame(object baseGame, CustomBoosterImplantTemplateDataBlock existingCT = null)
        {
            var baseBlock = (BoosterImplantTemplateDataBlock) baseGame;

            var customBlock = existingCT ?? new CustomBoosterImplantTemplateDataBlock();

            customBlock = (CustomBoosterImplantTemplateDataBlock) ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

            customBlock.Conditions = baseBlock.Conditions.ToSystemList();
            customBlock.Deprecated = baseBlock.Deprecated;
            customBlock.Description = baseBlock.Description;
            customBlock.DropWeight = baseBlock.DropWeight;
            customBlock.DurationRange = baseBlock.DurationRange;
            customBlock.Effects = baseBlock.Effects.ToSystemList();
            customBlock.ImplantCategory = baseBlock.ImplantCategory;
            customBlock.MainEffectType = baseBlock.MainEffectType;
            customBlock.PublicName = baseBlock.PublicName;
            customBlock.RandomConditions = baseBlock.RandomConditions.ToSystemList();
            var randomEffects = new List<List<BoosterImplantEffectInstance>>();
            foreach(var list in baseBlock.RandomEffects)
            {
                randomEffects.Add(list.ToSystemList());
            }
            customBlock.RandomEffects = randomEffects;

            return customBlock;
        }
        public Type GetBaseGameType() => typeof(BoosterImplantTemplateDataBlock);
        public Type GetCustomType() => typeof(CustomBoosterImplantTemplateDataBlock);
        public object ToBaseGame(CustomBoosterImplantTemplateDataBlock customType, object existingBaseGame = null) => throw new NotImplementedException();
    }
}
