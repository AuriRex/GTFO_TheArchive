using TheArchive.Core.Managers;

namespace TheArchive.Models.Boosters
{
    public class LocalDropServerBoosterImplantInventoryItem : LocalBoosterImplant
    {
        [JsonConstructor]
        public LocalDropServerBoosterImplantInventoryItem() : base()
        {

        }

        public LocalDropServerBoosterImplantInventoryItem(uint templateId, uint instanceId, int uses, Effect[] effects, uint[] conditions) : base(templateId, instanceId, uses, effects, conditions)
        {

        }

        public object ToBaseGame() => ToBaseGame(this);

        public static object ToBaseGame(LocalDropServerBoosterImplantInventoryItem custom)
        {
            return ImplementationManager.ToBaseGameConverter(custom);
        }

        public static LocalDropServerBoosterImplantInventoryItem FromBaseGame(object baseGame)
        {
            return ImplementationManager.FromBaseGameConverter<LocalDropServerBoosterImplantInventoryItem>(baseGame);
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
