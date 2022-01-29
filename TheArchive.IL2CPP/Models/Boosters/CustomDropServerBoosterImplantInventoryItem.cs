using Newtonsoft.Json;
using TheArchive.Interfaces;
using TheArchive.Managers;

namespace TheArchive.Models.Boosters
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

        public object ToBaseGame() => ToBaseGame(this);

        public static object ToBaseGame(CustomDropServerBoosterImplantInventoryItem custom)
        {
            return ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomDropServerBoosterImplantInventoryItem>>().ToBaseGame(custom);
        }

        public static CustomDropServerBoosterImplantInventoryItem FromBaseGame(object baseGame)
        {
            return ImplementationInstanceManager.GetOrFindImplementation<IBaseGameConverter<CustomDropServerBoosterImplantInventoryItem>>().FromBaseGame(baseGame);
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
