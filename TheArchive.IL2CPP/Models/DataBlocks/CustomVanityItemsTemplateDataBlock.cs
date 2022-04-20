namespace TheArchive.Models.DataBlocks
{
    public class CustomVanityItemsTemplateDataBlock : CustomGameDataBlockBase
    {
        public string PublicName { get; set; }
        public ClothesType Type { get; set; }
        public string Prefab { get; set; }
        public float DropWeight { get; set; }
        public string Icon { get; set; }
    }
}
