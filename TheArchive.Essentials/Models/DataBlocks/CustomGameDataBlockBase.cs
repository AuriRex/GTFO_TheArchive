namespace TheArchive.Models.DataBlocks;

public class CustomGameDataBlockBase
{
    public string Name { get; set; }

    public bool InternalEnabled { get; set; }

    public uint PersistentID { get; set; }
}