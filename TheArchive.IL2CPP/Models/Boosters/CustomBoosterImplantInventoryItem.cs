using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArchive.Models.Boosters
{
    public class CustomBoosterImplantInventoryItem
    {
        public CustomBoosterImplantInventoryItem(BoosterImplantInventoryItem boosterImplantInventoryItem)
        {
            SetFromBaseGame(boosterImplantInventoryItem);
        }

        public void SetFromBaseGame(BoosterImplantInventoryItem implantItem)
        {
            Touched = implantItem?.Touched ?? false;
            Prepared = implantItem?.Prepared ?? false;
#warning TODO
            Implant = null;/* TODO */ // new CustomBoosterImplant(implantItem?.Implant);
            InstanceId = implantItem?.InstanceId ?? 0;
        }

        public bool Touched { get; set; }
        public bool Prepared { get; set; }
        public CustomBoosterImplant Implant { get; set; }
        public uint InstanceId { get; set; } // Readonly in basegame
    }
}
