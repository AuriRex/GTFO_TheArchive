using SNetwork;
using System;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using static TheArchive.Utilities.PresenceFormatter;

namespace TheArchive.Features
{
    [EnableFeatureByDefault(true)]
    public class SteamRichPresenceTweaks : Feature
    {
        public class SteamRPCSettings
        {
            public bool DisableSteamRPC { get; set; } = false;
            public string CustomSteamRPCFormat { get; set; } = "%Rundown%%Expedition%";
        }

        public override string Name => "Steam Rich Presence Control";

        [FeatureConfig]
        public static SteamRPCSettings Config { get; set; }

        // Disables or changes Steam rich presence
        [ArchivePatch(typeof(SNet_Core_STEAM), "SetFriendsData", new Type[] { typeof(FriendsDataType), typeof(string) })]
        internal static class SNet_Core_STEAM_SetFriendsDataPatch
        {
            // string data here is the expedition name
            public static void Prefix(FriendsDataType type, ref string data)
            {
                if (Config.DisableSteamRPC)
                {
                    data = string.Empty;
                    return;
                }

                if (type == FriendsDataType.ExpeditionName)
                {
                    data = $"{FormatPresenceString(Config.CustomSteamRPCFormat)} \"{data}\"";
                }
            }
        }
    }
}
