using System;
using TheArchive.Core.Models;
using TheArchive.Core.Settings;
using TheArchive.Utilities;
using static TheArchive.Utilities.PresenceFormatter;

namespace TheArchive.Core.Managers
{
    public class PresenceManager
    {
        public static PresenceGameState LastState { get; private set; }
        public static PresenceGameState CurrentState { get; private set; }

        public static RichPresenceSettings Settings { get; private set; } = new RichPresenceSettings();

        public static DateTimeOffset CurrentStateStartTime { get; private set; }

        public static void UpdateGameState(PresenceGameState state, bool keepTimer = false)
        {
            ArchiveLogger.Msg(ConsoleColor.DarkMagenta, $"[{nameof(PresenceManager)}] UpdateGameState(): {CurrentState} --> {state}, keepTimer: {keepTimer}");
            LastState = CurrentState;
            CurrentState = state;
            if (!keepTimer)
            {
                CurrentStateStartTime = DateTimeOffset.UtcNow;
            }
        }

        internal static void Setup()
        {
            Settings = LocalFiles.LoadConfig<RichPresenceSettings>(out var fileExists, false).FillDefaultDictValues();
            if (!fileExists)
            {
                LocalFiles.SaveConfig(Settings);
            }

#warning TODO: implement new settings system and remove this jank:
            ArchiveMod.Settings.EnableDiscordRichPresence = Settings.EnableDiscordRichPresence;

            if (Settings.UseFormatStringForCopyLobbyIDButton)
            {
                ArchiveMod.Settings.LobbyIdFormatString = Settings.CopyLobbyIDFormatString;
            }
            else
            {
                ArchiveMod.Settings.LobbyIdFormatString = string.Empty;
            }
        }

        internal static void OnApplicationQuit()
        {
            LocalFiles.SaveConfig(Settings);
        }

        public static int GetPercentFromInts(string val, string max)
        {
            var value = (float) Get<int>(val);
            var maximum = (float) Get<int>(max);
            return GetPercent(value, maximum);
        }

        public static int GetPercentFromFloats(string val, string max)
        {
            var value = Get<float>(val);
            var maximum = Get<float>(max);
            return GetPercent(value, maximum);
        }

        public static int GetPercent(float val, float max) => (int) Math.Round(val / max * 100f);

        #region weapons
        [FallbackPresenceFormatProvider(nameof(EquippedMeleeWeaponName))]
        public static string EquippedMeleeWeaponName => "None";

        [FallbackPresenceFormatProvider(nameof(EquippedMeleeWeaponID))]
        public static string EquippedMeleeWeaponID => "None";

        [PresenceFormatProvider(nameof(MeleeWeaponKey))]
        public static string MeleeWeaponKey
        {
            get
            {
                string weaponId = Get<string>(nameof(EquippedMeleeWeaponID))?.ToLower();

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

        [FallbackPresenceFormatProvider(nameof(EquippedToolName))]
        public static string EquippedToolName => "None";

        [FallbackPresenceFormatProvider(nameof(EquippedToolID))]
        public static string EquippedToolID => "None";

        [PresenceFormatProvider(nameof(ToolKey))]
        public static string ToolKey
        {
            get
            {
                string toolId = Get<string>(nameof(EquippedToolID))?.ToLower();

                if (toolId != null)
                {
                    if (toolId.Contains("sentry"))
                        return "tool_sentry";
                    if (toolId.Contains("bio"))
                        return "tool_bio";
                    if (toolId.Contains("mine"))
                        return "tool_mine";
                    if (toolId.Contains("glue"))
                        return "tool_glue";
                }

                return "please_just_work";
            }
        }
        #endregion weapons

        #region player_ammo
        [FallbackPresenceFormatProvider(nameof(PrimaryAmmo))]
        public static int PrimaryAmmo => -1;

        [FallbackPresenceFormatProvider(nameof(MaxPrimaryAmmo))]
        public static int MaxPrimaryAmmo => -1;

        [PresenceFormatProvider(nameof(PrimaryAmmoPercent))]
        public static int PrimaryAmmoPercent => GetPercentFromInts(nameof(PrimaryAmmo), nameof(MaxPrimaryAmmo));

        [FallbackPresenceFormatProvider(nameof(SpecialAmmo))]
        public static int SpecialAmmo => -1;

        [FallbackPresenceFormatProvider(nameof(MaxSpecialAmmo))]
        public static int MaxSpecialAmmo => -1;

        [PresenceFormatProvider(nameof(SpecialAmmoPercent))]
        public static int SpecialAmmoPercent => GetPercentFromInts(nameof(SpecialAmmo), nameof(MaxSpecialAmmo));

        [FallbackPresenceFormatProvider(nameof(ToolAmmo))]
        public static int ToolAmmo => -1;

        [FallbackPresenceFormatProvider(nameof(MaxToolAmmo))]
        public static int MaxToolAmmo => -1;

        [PresenceFormatProvider(nameof(ToolAmmoOrStatus))]
        public static string ToolAmmoOrStatus
        {
            get
            {
                var toolKey = Get<string>(nameof(ToolKey));
                var toolAmmo = Get<int>(nameof(ToolAmmo));
                var maxToolAmmo = Get<int>(nameof(MaxToolAmmo));

                switch (toolKey)
                {
                    case "tool_bio":
                        return "∞";
                    case "tool_sentry":
                        if (toolAmmo > 0)
                            return $"{toolAmmo}/{maxToolAmmo}";
                        return "Deployed/Empty";
                    default:
                        if (toolAmmo > 0)
                            return $"{toolAmmo}/{maxToolAmmo}";
                        break;
                }

                return "Empty";
            }
        }

        [PresenceFormatProvider(nameof(ToolAmmoPercent))]
        public static int ToolAmmoPercent => GetPercentFromInts(nameof(ToolAmmo), nameof(MaxToolAmmo));

        [PresenceFormatProvider(nameof(ToolAmmoPercentOrStatus))]
        public static string ToolAmmoPercentOrStatus
        {
            get
            {
                var toolKey = Get<string>(nameof(ToolKey));
                var toolAmmo = (float) Get<int>(nameof(ToolAmmo));
                var maxToolAmmo = (float) Get<int>(nameof(MaxToolAmmo));

                switch (toolKey)
                {
                    case "tool_bio":
                        return "∞";
                    case "tool_sentry":
                        if (toolAmmo > 0)
                            return $"{GetPercent(toolAmmo, maxToolAmmo)}%";
                        return "Deployed/Empty";
                    default:
                        if (toolAmmo > 0)
                            return $"{GetPercent(toolAmmo, maxToolAmmo)}%";
                        break;
                }

                return "Empty";
            }
        }
        #endregion player_ammo

        #region player
        [FallbackPresenceFormatProvider(nameof(LocalCharacterID))]
        public static int LocalCharacterID { get; set; } = 0;

        [PresenceFormatProvider(nameof(CharacterImageKey))]
        public static string CharacterImageKey
        {
            get
            {
                int charId = Get<int>(nameof(LocalCharacterID));
                return charId switch
                {
                    0 => "char_woods",
                    1 => "char_dauda",
                    2 => "char_hackett",
                    3 => "char_bishop",
                    _ => "please_just_work",
                };
            }
        }

        [PresenceFormatProvider(nameof(CharacterName))]
        public static string CharacterName
        {
            get
            {
                int charId = Get<int>(nameof(LocalCharacterID));
                return charId switch
                {
                    0 => "Woods",
                    1 => "Dauda",
                    2 => "Hackett",
                    3 => "Bishop",
                    _ => "The Warden uwu",
                };
            }
        }

        [FallbackPresenceFormatProvider(nameof(HealthRaw))]
        public static float HealthRaw => -1;

        [FallbackPresenceFormatProvider(nameof(MaxHealthRaw))]
        public static float MaxHealthRaw => 25;

        [PresenceFormatProvider(nameof(HealthPercent))]
        public static int HealthPercent => GetPercentFromFloats(nameof(HealthRaw), nameof(MaxHealthRaw));
        #endregion player

        #region lobby
        [FallbackPresenceFormatProvider(nameof(LobbyID))]
        public static string LobbyID => "0123456789";

        [FallbackPresenceFormatProvider(nameof(OpenSlots))]
        public static int OpenSlots { get; set; } = 0;

        [FallbackPresenceFormatProvider(nameof(MaxPlayerSlots), true)]
        public static int MaxPlayerSlots { get; set; } = 4;
        #endregion lobby

        #region expedition
        [FallbackPresenceFormatProvider(nameof(ExpeditionTier))]
        public static string ExpeditionTier { get; set; } = string.Empty; // A, B, C, D, E

        [FallbackPresenceFormatProvider(nameof(ExpeditionTierIsSpecial), true)]
        public static bool ExpeditionTierIsSpecial { get; set; } = false; // True on R6 Extended Levels

        [FallbackPresenceFormatProvider(nameof(ExpeditionNumber))]
        public static string ExpeditionNumber { get; set; } = string.Empty; // 1, 2, 3, 4

        [FallbackPresenceFormatProvider(nameof(ExpeditionName))]
        public static string ExpeditionName { get; set; } = string.Empty; // "The Admin", "Crossways", "Deeper"


        [FallbackPresenceFormatProvider(nameof(ZonePrefix))]
        public static string ZonePrefix { get; set; } = string.Empty;

        [FallbackPresenceFormatProvider(nameof(ZonePrefixLong))]
        public static string ZonePrefixLong { get; set; } = string.Empty;

        [FallbackPresenceFormatProvider(nameof(ZoneAlias))]
        public static string ZoneAlias { get; set; } = string.Empty;

        [FallbackPresenceFormatProvider(nameof(AreaSuffix))]
        public static string AreaSuffix { get; set; } = string.Empty;

        [PresenceFormatProvider(nameof(CurrentZoneShort))]
        public static string CurrentZoneShort
        {
            get
            {
                return $"{Get(nameof(ZonePrefix))}_{Get(nameof(ZoneAlias))}";
            }
        }

        [PresenceFormatProvider(nameof(CurrentZoneLong))]
        public static string CurrentZoneLong
        {
            get
            {
                return $"{Get(nameof(ZonePrefixLong))} {Get(nameof(ZoneAlias))}";
            }
        }

        [PresenceFormatProvider(nameof(CurrentArea))]
        public static string CurrentArea
        {
            get
            {
                return $"Area {Get(nameof(AreaSuffix))}";
            }
        }

        [PresenceFormatProvider(nameof(ExpeditionWithNumber))]
        public static string ExpeditionWithNumber
        {
            get
            {
                return $"{Get(nameof(ExpeditionTier))}{Get(nameof(ExpeditionNumber))}";
            }
        }

        [PresenceFormatProvider(nameof(Expedition))]
        public static string Expedition
        {
            get
            {
                if(Get<bool>(nameof(ExpeditionTierIsSpecial)))
                    return $"{Get(nameof(ExpeditionTier))}";
                return $"{ExpeditionWithNumber}";
            }
        }
        #endregion expedition

        #region rundown
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
                return (int) ArchiveMod.CurrentRundown;
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
        #endregion rundown
    }
}
