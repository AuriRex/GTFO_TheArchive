using System.Collections.Generic;
using System.Linq;
using TheArchive.Models.Vanity;

namespace TheArchive.Models.DataBlocks
{
    public class CustomVanityItemsGroupDataBlock : CustomGameDataBlockBase
    {
        public List<uint> Items { get; set; }

        public List<uint> GetNonOwned(LocalVanityItemStorage playerData)
        {
            var value = new List<uint>();
            foreach(var item in Items)
            {
                if(!playerData.Items.Any(i => i.ItemID == item))
                {
                    value.Add(item);
                }
            }
            return value;
        }

        public bool HasAllOwned(LocalVanityItemStorage playerData)
        {
            foreach (var itemId in Items)
            {
                if (!playerData.Items.Any(i => i.ItemID == itemId))
                    return false;
            }
            return true;
        }
    }
}
