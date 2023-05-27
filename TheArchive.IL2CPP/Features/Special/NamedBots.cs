using Player;
using SNetwork;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.Special
{
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    public class NamedBots : Feature
    {
        public override string Name => "Named Bots";

        public override string Group => FeatureGroups.Special;

        public override string Description => "Change the Bots names.";

        public override bool PlaceSettingsInSubMenu => true;

        public override bool SkipInitialOnEnable => true;

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static NamedBotsSettings Settings { get; set; }

        public class NamedBotsSettings
        {
            [FSMaxLength(25)]
            public string Woods { get; set; } = nameof(Woods);
            [FSMaxLength(25)]
            public string Dauda { get; set; } = nameof(Dauda);
            [FSMaxLength(25)]
            public string Hackett { get; set; } = nameof(Hackett);
            [FSMaxLength(25)]
            public string Bishop { get; set; } = nameof(Bishop);
        }

#if IL2CPP
        public override void OnEnable()
        {
            SetAllBotNames();
        }

        public override void OnDisable()
        {
            if (IsApplicationQuitting)
                return;

            SetAllBotNames(setToDefault: true);
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            SetAllBotNames();
        }

        public static void SetAllBotNames(bool setToDefault = false)
        {
            if (!SNet.IsInLobby)
                return;

            foreach(var slot in SNet.Slots.PlayerSlots)
            {
                var player = slot.player;

                if (!player.HasPlayerAgent || !player.SafeIsBot())
                    continue;

                var agent = player.PlayerAgent.TryCastTo<PlayerAgent>();

                SetBotName(agent, setToDefault);
            }
        }

        public static void SetBotName(PlayerAgent agent, bool setToDefault = false)
        {
            if (!SNet.IsMaster)
                return;

            if (!agent.Owner.SafeIsBot())
                return;

            string name;
            switch (agent.CharacterID)
            {
                default:
                case 0:
                    name = setToDefault ? nameof(NamedBotsSettings.Woods) : Settings.Woods;
                    break;
                case 1:
                    name = setToDefault ? nameof(NamedBotsSettings.Dauda) : Settings.Dauda;
                    break;
                case 2:
                    name = setToDefault ? nameof(NamedBotsSettings.Hackett) : Settings.Hackett;
                    break;
                case 3:
                    name = setToDefault ? nameof(NamedBotsSettings.Bishop) : Settings.Bishop;
                    break;
            }

            if(agent.Owner.NickName != name)
                agent.Owner.NickName = name;
        }

        [ArchivePatch(nameof(PlayerAIBot.Setup))]
        internal static class PlayerAIBot_Setup_Patch
        {
            public static Type Type() => typeof(PlayerAIBot);

            public static void Postfix(PlayerAIBot __instance, PlayerAgent agent)
            {
                //FeatureLogger.Debug($"Bot agent {agent.Owner.NickName} has been created. CharacterID: {agent.CharacterID}");
                SetBotName(agent);
            }
        }
#endif
    }
}
