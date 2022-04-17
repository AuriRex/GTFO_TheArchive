using DropServer.VanityItems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnhollowerRuntimeLib;

namespace TheArchive.Models
{
    public class LocalVanityItemStorage
    {
        public List<LocalVanityItem> Items { get; set; } = new List<LocalVanityItem>();

        public void SetFlag(uint id, VanityItemFlags flag)
        {
            Items.Where(x => x.ItemID == id).ToList().ForEach(x => x.Flags |= flag);
        }

        public void UnsetFlag(uint id, VanityItemFlags flag)
        {
            Items.Where(x => x.ItemID == id).ToList().ForEach(x => x.Flags &= ~flag);
        }



        public class LocalVanityItem
        {
            public uint ItemID { get; set; } = 0;
            public VanityItemFlags Flags { get; set; } = VanityItemFlags.None;
        }

        [Flags]
        public enum VanityItemFlags
        {
            None = 0,
            Acknowledged = 1,
            Touched = 2
        }

        public object ToBaseGame() => ToBaseGame(this);

        public static /*object*/ VanityItemPlayerData ToBaseGame(LocalVanityItemStorage customData)
        {
            //return ImplementationManager.ToBaseGameConverter(customData);

            var vipd = new VanityItemPlayerData(ClassInjector.DerivedConstructorPointer<VanityItemPlayerData>());

            vipd.Items = new UnhollowerBaseLib.Il2CppReferenceArray<DropServer.VanityItems.VanityItem>(customData.Items.Count);

            for(int i = 0; i < customData.Items.Count; i++)
            {
                var current = customData.Items[i];

                var item = new DropServer.VanityItems.VanityItem()
                {
                    ItemId = current.ItemID,
                    Flags = (InventoryItemFlags) current.Flags
                };

                vipd.Items[i] = item;
            }

            return vipd;
        }

        public static LocalVanityItemStorage FromBaseGame(/*object*/ VanityItemPlayerData VanityItemPlayerData)
        {
            //return ImplementationManager.FromBaseGameConverter<LocalVanityItemStorage>(VanityItemPlayerData);

            var items = new List<LocalVanityItem>();

            foreach(var item in VanityItemPlayerData.Items)
            {
                items.Add(new LocalVanityItem() {
                    ItemID = item.ItemId,
                    Flags = (VanityItemFlags) item.Flags
                });
            }

            return new LocalVanityItemStorage()
            {
                Items = items
            };
        }
    }
}
