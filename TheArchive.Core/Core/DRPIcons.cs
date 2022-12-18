namespace TheArchive.Core
{
    /// <summary>
    /// Strings referring to icons used by Discord Rich Presence.
    /// </summary>
    public static class DRPIcons
    {
        public static string GTFOIcon => "gtfo_icon";

        public static string Debug => "please_just_work";

        public static string EnemyPing => "icon_enemy";

        public static class Weapons
        {
            private static string WeaponPrefix => "weapon_";

            public static string HeavyDutyHammer => $"{WeaponPrefix}hammer";
            public static string Knife => $"{WeaponPrefix}knife";
            public static string Bat => $"{WeaponPrefix}bat";
            public static string Spear => $"{WeaponPrefix}spear";
            
            public static string Maul => $"{WeaponPrefix}maul";
            public static string Sledge => $"{WeaponPrefix}sledge";
            public static string Gavel => $"{WeaponPrefix}gavel";
            public static string Mallet => $"{WeaponPrefix}mallet";

        }

        public static class Tools
        {
            private static string ToolPrefix => "tool_";

            public static string Biotracker => $"{ToolPrefix}bio";
            public static string CFoamLauncher => $"{ToolPrefix}glue";
            public static string MineDeployer => $"{ToolPrefix}mine";
            public static string SentryGun => $"{ToolPrefix}sentry";
        }

        public static class Resources
        {
            private static string ResourcePrefix => "res_";

            public static string Ammo => $"{ResourcePrefix}ammo";
            public static string Tool => $"{ResourcePrefix}tool";
            public static string Meds => $"{ResourcePrefix}meds";
            //Todo: Add Disinfection Packs
        }

        public static class Expedition
        {
            public static string ElevatorDropping => "icon_dropping";
            public static string Failed => "icon_failed";
            public static string Survived => "icon_survived";
            public static string Lobby => "icon_lobby";
            public static string Reactor => "icon_reactor";
        }

        public static class Characters
        {
            private static string CharacterPrefix => "char_";

            public static string Woods => $"{CharacterPrefix}woods";
            public static string Dauda => $"{CharacterPrefix}dauda";
            public static string Hackett => $"{CharacterPrefix}hackett";
            public static string Bishop => $"{CharacterPrefix}bishop";
        }
    }
}
