using GameData;
using System;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using TheArchive.Utilities;

namespace TheArchive.IL2CPP.R6.Factories.DataBlocks
{
    public class CustomVanityItemsTemplateDataBlockFactory : IBaseGameConverter<CustomVanityItemsTemplateDataBlock>
    {
        public CustomVanityItemsTemplateDataBlock FromBaseGame(object baseGame, CustomVanityItemsTemplateDataBlock existingCT = null)
        {
            var baseBlock = (VanityItemsTemplateDataBlock)baseGame;

            var customBlock = existingCT ?? new CustomVanityItemsTemplateDataBlock();

            customBlock = (CustomVanityItemsTemplateDataBlock) ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

            customBlock.PublicName = baseBlock.publicName;
            customBlock.Type = baseBlock.type;
            customBlock.Prefab = baseBlock.prefab;
            customBlock.DropWeight = baseBlock.DropWeight;
            customBlock.Icon = baseBlock.icon;

            return customBlock;
        }

        public Type GetBaseGameType() => typeof(VanityItemsTemplateDataBlock);

        public Type GetCustomType() => typeof(CustomVanityItemsTemplateDataBlock);

        public object ToBaseGame(CustomVanityItemsTemplateDataBlock customType, object existingBaseGame = null) => throw new NotImplementedException();
    }
}
