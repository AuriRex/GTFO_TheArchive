namespace TheArchive.Models.DataBlocks
{
    public class CustomVanityItemsTemplateDataBlock : CustomGameDataBlockBase
    {
        public string PublicName { get; set; }
        public A_ClothesType Type { get; set; }
        public string Prefab { get; set; }
        public float DropWeight { get; set; }
        public string Icon { get; set; }

        public enum A_ClothesType
        {
            Helmet,
            Torso,
            Legs,
            Backpack,
            Palette,
            Face
        }
    }
}
