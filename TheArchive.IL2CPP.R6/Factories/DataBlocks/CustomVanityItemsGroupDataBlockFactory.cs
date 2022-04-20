
using GameData;
using System;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using TheArchive.Utilities;

namespace TheArchive.IL2CPP.R6.Factories.DataBlocks
{
    public class CustomVanityItemsGroupDataBlockFactory : IBaseGameConverter<CustomVanityItemsGroupDataBlock>
    {
        public CustomVanityItemsGroupDataBlock FromBaseGame(object baseGame, CustomVanityItemsGroupDataBlock existingCT = null)
        {
            var baseBlock = (VanityItemsGroupDataBlock)baseGame;

            var customBlock = existingCT ?? new CustomVanityItemsGroupDataBlock();

            customBlock = (CustomVanityItemsGroupDataBlock) ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

            customBlock.Items = baseBlock.Items.ToSystemList();

            return customBlock;
        }

        public Type GetBaseGameType() => typeof(VanityItemsGroupDataBlock);

        public Type GetCustomType() => typeof(CustomVanityItemsGroupDataBlock);

        public object ToBaseGame(CustomVanityItemsGroupDataBlock customType, object existingBaseGame = null) => throw new NotImplementedException();
    }
}
