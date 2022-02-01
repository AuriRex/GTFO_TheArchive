using TheArchive.Interfaces;
using TheArchive.Managers;

namespace TheArchive.Models.Boosters
{
    public class CustomBoosterTransaction
    {
        public uint MaxBackendTemplateId { get; set; }
        public uint[] AcknowledgeIds { get; set; }
        public uint[] TouchIds { get; set; }
        public uint[] DropIds { get; set; }
        public CustomMissed AcknowledgeMissed { get; set; }

        public object ToBaseGame() => ToBaseGame(this);

        public static object ToBaseGame(CustomBoosterTransaction customTrans)
        {
            return ImplementationInstanceManager.ToBaseGameConverter(customTrans);
        }

        public static CustomBoosterTransaction FromBaseGame(object baseGameTrans)
        {
            return ImplementationInstanceManager.FromBaseGameConverter<CustomBoosterTransaction>(baseGameTrans);
        }

        public class CustomMissed
        {
            public int Basic { get; set; }
            public int Advanced { get; set; }
            public int Specialized { get; set; }

            public object ToBaseGame() => ToBaseGame(this);

            public static object ToBaseGame(CustomMissed customMissed)
            {
                return ImplementationInstanceManager.ToBaseGameConverter(customMissed);
            }

            public static CustomMissed FromBaseGame(object baseGameMissed)
            {
                return ImplementationInstanceManager.FromBaseGameConverter<CustomMissed>(baseGameMissed);
            }
        }

    }
}
