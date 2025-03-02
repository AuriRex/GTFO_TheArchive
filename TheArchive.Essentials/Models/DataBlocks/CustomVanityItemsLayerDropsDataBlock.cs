using System.Collections.Generic;

namespace TheArchive.Models.DataBlocks;

public class CustomVanityItemsLayerDropsDataBlock : CustomGameDataBlockBase
{
    public List<CustomLayerDropData> LayerDrops { get; set; }

    public class CustomLayerDropData
    {
        public DropServer.ExpeditionLayers Layer { get; set; }
        public int Count { get; set; }
        public bool IsAll { get; set; }
        public List<uint> Groups { get; set; }
    }
}