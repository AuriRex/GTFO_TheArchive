using System;
using TheArchive.Utilities;
using static TheArchive.Utilities.PresenceFormatter;

namespace TheArchive.Core.Managers
{
    public class PresenceManager
    {

        public static string GetMeleeWeaponKey()
        {
            string weaponId = ((string) Get("EquippedMeleeWeaponID"))?.ToLower();
            
            if(weaponId != null)
            {
                // 😩
                switch (weaponId)
                {
                    case "heavydutyhammer":
                        weaponId = "hammer";
                        break;
                    case "sledgehammer":
                        weaponId = "sledge";
                        break;
                }
                return $"weapon_{weaponId}";
            }

            return "please_just_work";
        }

        public static string GetCharacterImageKey()
        {
            int charId = (int) Get("LocalCharacterID");
            switch(charId)
            {
                case 0:
                    return "char_woods";
                case 1:
                    return "char_dauda";
                case 2:
                    return "char_hackett";
                case 3:
                    return "char_bishop";
            }
            return "please_just_work";
        }

        public static string GetCharacterName()
        {
            int charId = (int) Get("LocalCharacterID");
            switch (charId)
            {
                case 0:
                    return "Woods";
                case 1:
                    return "Dauda";
                case 2:
                    return "Hackett";
                case 3:
                    return "Bishop";
            }
            return "The Warden uwu";
        }

        [FallbackPresenceFormatProvider("EquippedMeleeWeaponName")]
        public static string EquippedMeleeWeaponName => "None";

        [FallbackPresenceFormatProvider("EquippedMeleeWeaponID")]
        public static string EquippedMeleeWeaponID => "None";

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

        [FallbackPresenceFormatProvider("LocalCharacterID")]
        public static int LocalCharacterID { get; set; } = 0;

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
