using TheArchive.Utilities;
using static TheArchive.Utilities.PresenceFormatter;

namespace TheArchive.Core.Managers
{
    public class PresenceManager
    {

        [FallbackPresenceFormatProvider("LobbyID")]
        public static string LobbyID => "0123456789";

        [FallbackPresenceFormatProvider("ExpeditionTier")]
        public static string ExpeditionTier { get; set; } = string.Empty; // A, B, C, D, E

        [FallbackPresenceFormatProvider("ExpeditionNumber")]
        public static string ExpeditionNumber { get; set; } = string.Empty; // 1, 2, 3, 4

        [FallbackPresenceFormatProvider("ExpeditionName")]
        public static string ExpeditionName { get; set; } = string.Empty; // "The Admin", "Crossways", "Deeper"

        [FallbackPresenceFormatProvider("OpenSlots")]
        public static int OpenSlots { get; set; } = 0;

        [FallbackPresenceFormatProvider("MaxPlayerSlots", true)]
        public static int MaxPlayerSlots { get; set; } = 4;

        [PresenceFormatProvider("Expedition")]
        public static string Expedition
        {
            get
            {
                return $"{Get("ExpeditionTier")}{Get("ExpeditionNumber")}";
            }
        }

        [PresenceFormatProvider("Rundown")]
        public static string Rundown
        {
            get
            {
                return $"R{RundownNumber}";
            }
        }

        [PresenceFormatProvider("RundownNumber")]
        public static int RundownNumber
        {
            get
            {
                return ArchiveMod.CurrentRundown.GetIntValue();
            }
        }

        [PresenceFormatProvider("RundownName")]
        public static string RundownName => RundownTitle;

        [PresenceFormatProvider("RundownTitle")]
        public static string RundownTitle
        {
            get
            {
                return Utilities.Utils.GetRundownTitle(ArchiveMod.CurrentRundown);
            }
        }
    }
}
