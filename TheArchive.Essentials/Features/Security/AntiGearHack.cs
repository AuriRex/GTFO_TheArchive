using GameData;
using Gear;
using Player;
using SNetwork;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.Security;

[EnableFeatureByDefault]
public class AntiGearHack : Feature
{
    public override string Name => "Anti Gear Hack";

    public override string Description => "Prevents clients use modified gears.";

    public override FeatureGroup Group => FeatureGroups.Security;

    public new static IArchiveLogger FeatureLogger { get; set; }

    [FeatureConfig]
    public static AntiGearHackSettings Settings { get; set; }

    public class AntiGearHackSettings
    {
        [FSDisplayName("Punish Friends")]
        [FSDescription("If (Steam) Friends should be affected as well.")]
        public bool PunishFriends { get; set; } = false;

        [FSDisplayName("Punishment")]
        [FSDescription("What to do with griefers that are using modified gears.")]
        public PunishmentMode Punishment { get; set; } = PunishmentMode.Kick;

        [Localized]
        public enum PunishmentMode
        {
            NoneAndLog,
            Kick,
            KickAndBan
        }
    }

    public override void Init()
    {
        if (ArchiveMod.IsPlayingModded)
        {
            RequestDisable("Playing Modded");
        }
    }

    public override void OnGameDataInitialized()
    {
        LoadData();
    }

    [ArchivePatch(typeof(PlayerBackpackManager), nameof(PlayerBackpackManager.ReceiveInventorySync))]
    private class PlayerBackpackManager__ReceiveInventorySync__Patch
    {
        private static void Prefix(pInventorySync data)
        {
            if (!SNet.IsMaster || !data.sourcePlayer.TryGetPlayer(out var player) || player.IsLocal || player.IsBot
                || !SNet.Replication.TryGetLastSender(out var sender) || sender.IsLocal || sender.IsBot)
            {
                return;
            }
            List<GearIDRange> list = new List<GearIDRange>
            {
                new GearIDRange(data.gearStandard),
                new GearIDRange(data.gearSpecial),
                new GearIDRange(data.gearMelee),
                new GearIDRange(data.gearClass),
                new GearIDRange(data.gearHacking)
            };
            foreach (var id in list)
            {
                if (!CheckGearIDRange(id))
                {
                    PunishPlayer(sender);
                    return;
                }
            }
        }
    }

    public static bool PunishPlayer(SNet_Player player)
    {
        if (player == null)
            return true;

        if (player.IsFriend() && !Settings.PunishFriends)
        {
            FeatureLogger.Notice($"Friend \"{player.NickName}\" \"{player.Lookup}\" is using modified gears!");
            return false;
        }

        switch (Settings.Punishment)
        {
            case AntiGearHackSettings.PunishmentMode.KickAndBan:
                PlayerLobbyManagement.BanPlayer(player);
                goto default;
            case AntiGearHackSettings.PunishmentMode.Kick:
                PlayerLobbyManagement.KickPlayer(player);
                goto default;
            default:
            case AntiGearHackSettings.PunishmentMode.NoneAndLog:
                FeatureLogger.Notice($"Player \"{player.NickName}\" \"{player.Lookup}\" is using modified gears! ({Settings.Punishment})");
                return true;
        }
    }

    public static bool CheckIsValidWeaponGearIDRangeDataForPlayer(SNet_Player player)
    {
        if (!player.HasCharacterSlot)
        {
            return false;
        }
        foreach (var id in GearManager.Current.m_gearPerSlot[player.PlayerSlotIndex()])
        {
            if (CheckGearIDRange(id))
                return false;
        }
        return true;
    }

    public static bool CheckGearIDRange(GearIDRange gearIDRange)
    {
        string text = gearIDRange.ToJSON();
        string text2 = "(?<=Comps\":)(.*?)(?=,\"MatTrans\")";
        Match match = Regex.Match(text, text2);
        //string text3 = "(?<=Name\":\")(.*?)(?=\")";
        //Regex.Match(text, text3);
        //string text4 = "(?<=data\":\")(.*?)(?=\"})";
        //Regex.Match(text, text4);
        string hashString = match.Value.HashString();
        return GearCompsHashLookup.ContainsKey(hashString);
    }

    public static void LoadData()
    {
        GearCompsHashLookup.Clear();
        foreach (PlayerOfflineGearDataBlock playerOfflineGearDataBlock in GameDataBlockBase<PlayerOfflineGearDataBlock>.GetAllBlocks())
        {
            string gearJSON = playerOfflineGearDataBlock.GearJSON;
            string text = "(?<=Comps\":)(.*?)(?=,\"MatTrans\")";
            Match capture = Regex.Match(gearJSON, text);
            //string text2 = "(?<=Name\":\")(.*?)(?=\")";
            //Regex.Match(gearJSON, text2);
            //string text3 = "(?<=data\":\")(.*?)(?=\"})";
            //Regex.Match(gearJSON, text3);
            string hashString = capture.Value.HashString();
            GearCompsHashLookup[hashString] = gearJSON;
        }
    }

    private static Dictionary<string, string> GearCompsHashLookup = new Dictionary<string, string>();
}