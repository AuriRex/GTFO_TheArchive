using TheArchive.Core.Managers;

namespace TheArchive.Models.Boosters
{
    public class LocalBoosterTransaction
    {
        public uint MaxBackendTemplateId { get; set; }
        public uint[] AcknowledgeIds { get; set; }
        public uint[] TouchIds { get; set; }
        public uint[] DropIds { get; set; }
        public CustomMissed AcknowledgeMissed { get; set; }

        public object ToBaseGame() => ToBaseGame(this);

        public static object ToBaseGame(LocalBoosterTransaction customTrans)
        {
            return ImplementationManager.ToBaseGameConverter(customTrans);
        }

        public static LocalBoosterTransaction FromBaseGame(object baseGameTrans)
        {
            return ImplementationManager.FromBaseGameConverter<LocalBoosterTransaction>(baseGameTrans);
        }

        public class CustomMissed
        {
            public int Basic { get; set; }
            public int Advanced { get; set; }
            public int Specialized { get; set; }

            public object ToBaseGame() => ToBaseGame(this);

            public static object ToBaseGame(CustomMissed customMissed)
            {
                return ImplementationManager.ToBaseGameConverter(customMissed);
            }

            public static CustomMissed FromBaseGame(object baseGameMissed)
            {
                return ImplementationManager.FromBaseGameConverter<CustomMissed>(baseGameMissed);
            }
        }

    }
}
