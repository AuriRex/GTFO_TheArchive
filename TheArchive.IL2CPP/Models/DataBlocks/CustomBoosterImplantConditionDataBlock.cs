namespace TheArchive.Models.DataBlocks
{
    public class CustomBoosterImplantConditionDataBlock : CustomGameDataBlockBase
    {

        // enum (BoosterCondition)
        public int Condition { get; set; }
        public string PublicShortName { get; set; }
        public string PublicName { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }

    }
}
