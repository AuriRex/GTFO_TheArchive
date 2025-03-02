using CellMenu;
using SNetwork;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Hud
{
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    public class DontHideLoadoutUI : Feature
    {
        public override string Name => "Don't Hide Loadout UI";

        public override FeatureGroup Group => FeatureGroups.Hud;

        public override string Description => "Keep loadout visible after readying up / in expedition";

#if IL2CPP
        public override void OnDisable()
        {
            if (IsApplicationQuitting)
                return;

            CollidersEnabled = true;
            foreach (var bar in CM_PageLoadout.Current.m_playerLobbyBars)
            {
                UpdateLobbyBarColliders(bar);
            }
        }

        public static bool CollidersEnabled { get; private set; } = true;

        public static void SetColliderEnabledState(Transform trans)
        {
            var collider = trans?.gameObject?.GetComponent<Collider2D>();

            if (collider == null)
                return;

            collider.enabled = CollidersEnabled;
        }

        public static void UpdateLobbyBarColliders(CM_PlayerLobbyBar playerLobbyBar)
        {
            playerLobbyBar.m_pillarRoot.transform.ForEachChildDo(SetColliderEnabledState);
            playerLobbyBar.m_boosterImplantAlign.ForEachChildDo(SetColliderEnabledState);

            SetColliderEnabledState(playerLobbyBar.m_clothesButton.transform);

            // Don't touch permission button collider unless we want to change it back once we drop into the level again!
            //playerLobbyBar.m_permissionButton.gameObject.SetActive(!playerLobbyBar.m_player?.IsLocal ?? false & SNet.IsMaster & CollidersEnabled);
        }

        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.UpdatePlayer))]
        internal static class CM_PlayerLobbyBar_UpdatePlayer_Patch
        {
            public static void Postfix(CM_PlayerLobbyBar __instance)
            {
                CM_PlayerLobbyBar_HideLoadoutUI_Patch.Prefix(__instance, !CollidersEnabled);
            }
        }

        [ArchivePatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.HideLoadoutUI))]
        internal static class CM_PlayerLobbyBar_HideLoadoutUI_Patch
        {
            public static bool Prefix(CM_PlayerLobbyBar __instance, bool hide)
            {
                if (GameStateManager.IsInExpedition)
                    return ArchivePatch.SKIP_OG;

                CollidersEnabled = !hide;

                if (__instance.m_player != null)
                {
                    if (__instance.m_player.IsLocal || SNet.IsMaster && __instance.m_player.SafeIsBot())
                        UpdateLobbyBarColliders(__instance);
                }

                if (!hide)
                    return ArchivePatch.RUN_OG;

                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.UpdateReadyState))]
        internal static class CM_PageLoadout_UpdateReadyState_Patch
        {
            public static void Postfix(CM_PageLoadout __instance)
            {
                __instance.m_copyLobbyIdRoot.SetActive(!SNet.IsExternalMatchMakingActive);
            }
        }
#endif
    }
}
