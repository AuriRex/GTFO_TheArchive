using System;
using TheArchive.Utilities;
using static TheArchive.Utilities.PresenceFormatter;

namespace TheArchive.Core.Managers
{
    public class PresenceManager
    {
        [FallbackPresenceFormatProvider(nameof(EquippedMeleeWeaponName))]
        public static string EquippedMeleeWeaponName => "None";

        [FallbackPresenceFormatProvider(nameof(EquippedMeleeWeaponID))]
        public static string EquippedMeleeWeaponID => "None";

        [PresenceFormatProvider(nameof(MeleeWeaponKey))]
        public static string MeleeWeaponKey
        {
            get
            {
                string weaponId = ((string) Get(nameof(EquippedMeleeWeaponID)))?.ToLower();

                if (weaponId != null)
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
        }

        [FallbackPresenceFormatProvider(nameof(LocalCharacterID))]
        public static int LocalCharacterID { get; set; } = 0;

        [PresenceFormatProvider(nameof(CharacterImageKey))]
        public static string CharacterImageKey
        {
            get
            {
                int charId = (int) Get(nameof(LocalCharacterID));
                switch (charId)
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
        }

        [PresenceFormatProvider(nameof(CharacterName))]
        public static string CharacterName
        {
            get
            {
                int charId = (int) Get(nameof(LocalCharacterID));
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
        }

        [FallbackPresenceFormatProvider(nameof(HealthRaw))]
        public static float HealthRaw => -1;

        [FallbackPresenceFormatProvider(nameof(MaxHealthRaw))]
        public static float MaxHealthRaw => 25;

        [PresenceFormatProvider(nameof(HealthPercent))]
        public static int HealthPercent
        {
            get
            {
                var health = (float) Get(nameof(HealthRaw));
                var healthMax = (float) Get(nameof(MaxHealthRaw));
                return (int) Math.Round(health / healthMax * 100f);
            }
        }

        [FallbackPresenceFormatProvider(nameof(LobbyID))]
        public static string LobbyID => "0123456789";

        [FallbackPresenceFormatProvider(nameof(ExpeditionTier))]
        public static string ExpeditionTier { get; set; } = string.Empty; // A, B, C, D, E

        [FallbackPresenceFormatProvider(nameof(ExpeditionNumber))]
        public static string ExpeditionNumber { get; set; } = string.Empty; // 1, 2, 3, 4

        [FallbackPresenceFormatProvider(nameof(ExpeditionName))]
        public static string ExpeditionName { get; set; } = string.Empty; // "The Admin", "Crossways", "Deeper"

        [FallbackPresenceFormatProvider(nameof(OpenSlots))]
        public static int OpenSlots { get; set; } = 0;

        [FallbackPresenceFormatProvider(nameof(MaxPlayerSlots), true)]
        public static int MaxPlayerSlots { get; set; } = 4;

        [PresenceFormatProvider(nameof(Expedition))]
        public static string Expedition
        {
            get
            {
                return $"{Get(nameof(ExpeditionTier))}{Get(nameof(ExpeditionNumber))}";
            }
        }

        [PresenceFormatProvider(nameof(Rundown))]
        public static string Rundown
        {
            get
            {
                return $"R{RundownNumber}";
            }
        }

        [PresenceFormatProvider(nameof(RundownNumber))]
        public static int RundownNumber
        {
            get
            {
                return ArchiveMod.CurrentRundown.GetIntValue();
            }
        }

        [PresenceFormatProvider(nameof(RundownName))]
        public static string RundownName => RundownTitle;

        [PresenceFormatProvider(nameof(RundownTitle))]
        public static string RundownTitle
        {
            get
            {
                return Utilities.Utils.GetRundownTitle(ArchiveMod.CurrentRundown);
            }
        }
    }
}
