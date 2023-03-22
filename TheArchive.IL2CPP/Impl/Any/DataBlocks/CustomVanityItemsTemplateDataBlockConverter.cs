using GameData;
using System;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using static TheArchive.Models.DataBlocks.CustomVanityItemsTemplateDataBlock;

namespace TheArchive.Impl.Any.DataBlocks
{
    public class CustomVanityItemsTemplateDataBlockConverter : IBaseGameConverter<CustomVanityItemsTemplateDataBlock>
    {
        public CustomVanityItemsTemplateDataBlock FromBaseGame(object baseGame, CustomVanityItemsTemplateDataBlock existingCT = null)
        {
            var baseBlock = (VanityItemsTemplateDataBlock)baseGame;

            var customBlock = existingCT ?? new CustomVanityItemsTemplateDataBlock();

            customBlock = (CustomVanityItemsTemplateDataBlock)ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

            customBlock.PublicName = baseBlock.publicName;
            customBlock.Type = (A_ClothesType)baseBlock.type;
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
