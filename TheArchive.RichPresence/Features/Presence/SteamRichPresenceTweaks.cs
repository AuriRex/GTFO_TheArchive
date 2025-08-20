using SNetwork;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using static TheArchive.Utilities.PresenceFormatter;

namespace TheArchive.Features.Presence;

[EnableFeatureByDefault]
public class SteamRichPresenceTweaks : Feature
{
    public class SteamRPCSettings
    {
        [FSDisplayName("Disable <color=orange>ALL</color> of Steam RPC")]
        [FSDescription("Enabling this will completely disable relaying your current in-game status as well as <b>prevent anyone from joining on you via steam</b>.")]
        public bool DisableSteamRPC { get; set; } = false;
        [FSDisplayName("Custom Status Format")]
        public string CustomSteamRPCFormat { get; set; } = "%Rundown%%Expedition% \"%ExpeditionName%\"";
    }

    public override string Name => "Steam Rich Presence Tweaks";

    public override FeatureGroup Group => FeatureGroups.Presence;

    public override string Description => "Set a custom text for Steams' presence system.";

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
                data = $"{FormatPresenceString(Config.CustomSteamRPCFormat)}";
            }
        }
    }
}