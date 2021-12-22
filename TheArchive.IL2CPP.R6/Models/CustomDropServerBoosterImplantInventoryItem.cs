using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;

namespace TheArchive.IL2CPP.R6.Models
{
    public class CustomDropServerBoosterImplantInventoryItem : CustomBoosterImplant
    {
        [JsonConstructor]
        public CustomDropServerBoosterImplantInventoryItem() : base()
        {

        }

        public CustomDropServerBoosterImplantInventoryItem(uint templateId, uint instanceId, int uses, Effect[] effects, uint[] conditions) : base(templateId, instanceId, uses, effects, conditions)
        {

        }

        public CustomDropServerBoosterImplantInventoryItem(DropServer.BoosterImplants.BoosterImplantInventoryItem boosterImplantInventoryItem) : base(boosterImplantInventoryItem)
        {
            SetFromBaseGame(boosterImplantInventoryItem);
        }

        public DropServer.BoosterImplants.BoosterImplantInventoryItem ToBaseGame()
        {
            var biii = base.ToBaseGameInventoryItem();

            biii.Flags = Flags;

            return biii;
        }

        public void SetFromBaseGame(DropServer.BoosterImplants.BoosterImplantInventoryItem implantItem)
        {
            Flags = implantItem.Flags;
            IsTouched = implantItem.IsTouched;
        }

        public uint Flags { get; set; } = 0;
        
        [JsonIgnore]
        public bool IsTouched
        {
            get
            {
                return (Flags & 1) != 0;
            }
            set
            {
                if(value)
                {
                    Flags |= 1;
                }
                else
                {
                    uint tmp = ~(uint)1;
                    Flags &= tmp;
                }
            }
        }
    }
}
