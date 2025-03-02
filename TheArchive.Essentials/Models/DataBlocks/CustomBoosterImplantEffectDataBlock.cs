namespace TheArchive.Models.DataBlocks;

public class CustomBoosterImplantEffectDataBlock : CustomGameDataBlockBase
{

    public int Effect { get; set; }
    public string PublicShortName { get; set; }
    public string PublicName { get; set; }
    public string Description { get; set; }
    public string DescriptionNegative { get; set; }
    // enum (BoosterEffectCategory)
    public int BoosterEffectCategory { get; set; }

}