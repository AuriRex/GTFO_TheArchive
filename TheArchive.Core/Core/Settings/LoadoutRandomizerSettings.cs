using System.Collections.Generic;

namespace TheArchive.Core.Settings
{
    // Todo auto config System :>
    public class LoadoutRandomizerSettings
    {
        public bool Enable { get; set; } = true;
        public List<InventorySlots> ExcludedSlots { get; set; } = new List<InventorySlots>();
        public RandomizerMode Mode { get; set; } = RandomizerMode.NoDuplicate;

        public enum InventorySlots
        {
            Primary,
            Special,
            Tool,
            Melee
        }

        public enum RandomizerMode
        {
            True,
            NoDuplicate
        }
    }
}
