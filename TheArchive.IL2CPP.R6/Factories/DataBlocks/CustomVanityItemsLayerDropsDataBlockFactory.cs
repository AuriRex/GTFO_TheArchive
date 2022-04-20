using GameData;
using System;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using TheArchive.Models.DataBlocks;
using System.Collections.Generic;
using TheArchive.Utilities;

namespace TheArchive.IL2CPP.R6.Factories.DataBlocks
{
    public class CustomVanityItemsLayerDropsDataBlockFactory : IBaseGameConverter<CustomVanityItemsLayerDropsDataBlock>
    {
        public CustomVanityItemsLayerDropsDataBlock FromBaseGame(object baseGame, CustomVanityItemsLayerDropsDataBlock existingCT = null)
        {
            var baseBlock = (VanityItemsLayerDropsDataBlock) baseGame;

            var customBlock = existingCT ?? new CustomVanityItemsLayerDropsDataBlock();

            customBlock = (CustomVanityItemsLayerDropsDataBlock)ImplementationManager.FromBaseGameConverter<CustomGameDataBlockBase>(baseGame, customBlock);

            customBlock.LayerDrops = new List<CustomVanityItemsLayerDropsDataBlock.CustomLayerDropData>();

            foreach(var item in baseBlock.LayerDrops)
            {
                var cItem = new CustomVanityItemsLayerDropsDataBlock.CustomLayerDropData
                {
                    Layer = item.Layer,
                    Count = item.Count,
                    IsAll = item.IsAll,
                    Groups = item.Groups.ToSystemList()
                };

                customBlock.LayerDrops.Add(cItem);
            }

            return customBlock;
        }

        public Type GetBaseGameType() => typeof(VanityItemsLayerDropsDataBlock);

        public Type GetCustomType() => typeof(CustomVanityItemsLayerDropsDataBlock);

        public object ToBaseGame(CustomVanityItemsLayerDropsDataBlock customType, object existingBaseGame = null) => throw new NotImplementedException();
    }
}
