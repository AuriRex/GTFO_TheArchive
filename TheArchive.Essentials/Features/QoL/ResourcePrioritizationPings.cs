using Player;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.SharedUtils;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.QoL;

[EnableFeatureByDefault]
[RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownAltFive)]
public class ResourcePrioritizationPings : Feature
{
    public override string Name => "Prioritize Resource Pings";

    public override GroupBase Group => GroupManager.QualityOfLife;

    public override string Description => "Resource Packs will be prioritized and show up with special icons and trigger voice lines when pinged by Middle-Mouse-Pings.\n(Yes, disinfect is ammo apparently)";

    public new static IArchiveLogger FeatureLogger { get; set; }

#if IL2CPP
    public static uint Sound_DisinfectHere_ID { get; private set; }
    public static uint Sound_ToolHere_ID { get; private set; }


    public override void OnDatablocksReady()
    {
        Sound_DisinfectHere_ID = SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_DISINFECTIONHERE));
        Sound_ToolHere_ID = SoundEventCache.Resolve(nameof(AK.EVENTS.PLAY_CL_TOOLREFILLHERE));
    }

    // R6+ only!
    private static bool OverrideSoundsAndPings(Vector3 worldPos, GameObject gameObject)
    {
        bool shouldSkipOG = false;
        uint soundToTrigger = 0;
        eNavMarkerStyle navMarkerStyle = eNavMarkerStyle.PlayerInCompass;

        Gear.ResourcePackPickup resourcePack = gameObject.GetComponentInChildren<Gear.ResourcePackPickup>();

        if (resourcePack != null)
        {
            gameObject = resourcePack.gameObject;

            switch (resourcePack.m_packType)
            {
                case eResourceContainerSpawnType.Disinfection:
                    soundToTrigger = Sound_DisinfectHere_ID;
                    navMarkerStyle = eNavMarkerStyle.PlayerPingDisinfection;
                    shouldSkipOG = true;
                    break;
                case eResourceContainerSpawnType.AmmoTool:
                    soundToTrigger = Sound_ToolHere_ID;
                    navMarkerStyle = eNavMarkerStyle.PlayerPingAmmo;
                    shouldSkipOG = true;
                    break;
            }
        }

        var localPlayer = PlayerManager.GetLocalPlayerAgent();

        if (soundToTrigger != 0)
        {
            PlayerVoiceManager.WantToSay(localPlayer.CharacterID, soundToTrigger);
        }

        if(navMarkerStyle != eNavMarkerStyle.PlayerInCompass)
        {
            GuiManager.AttemptSetPlayerPingStatus(localPlayer, true, worldPos, navMarkerStyle);

            localPlayer.Sync.SendGenericInteract(pGenericInteractAnimation.TypeEnum.PointForward, false);

            PlayerManager.Current.OnObjectHighlighted(localPlayer.CourseNode, gameObject);
        }

        return shouldSkipOG;
    }
#endif

    // prioritize resources in ping raycasts
    [RundownConstraint(RundownFlags.RundownFour, RundownFlags.RundownFive)]
    [ArchivePatch(typeof(PlayerAgent), "TriggerMarkerPing")]
    internal static class PlayerAgent_TriggerMarkerPing_R4_R5_Patch
    {
        private static PropertyAccessor<PlayerAgent, Vector3> A_PlayerAgent_CamPos;
        private static PropertyAccessor<PlayerAgent, iPlayerPingTarget> A_PlayerAgent_m_pingTargets;
        private static PropertyAccessor<PlayerAgent, Vector3> A_PlayerAgent_m_pingPos;

        public static void Init()
        {
            A_PlayerAgent_CamPos = PropertyAccessor<PlayerAgent, Vector3>.GetAccessor("CamPos");
            A_PlayerAgent_m_pingTargets = PropertyAccessor<PlayerAgent, iPlayerPingTarget>.GetAccessor("m_pingTarget");
            A_PlayerAgent_m_pingPos = PropertyAccessor<PlayerAgent, Vector3>.GetAccessor("m_pingPos");
        }

        public static void Prefix(PlayerAgent __instance, ref iPlayerPingTarget target, ref Vector3 worldPos)
        {
            try
            {
                if (__instance == null || !__instance.IsLocallyOwned || target == null)
                {
                    return;
                }

                var rayCastHits = Physics.RaycastAll(A_PlayerAgent_CamPos.Get(__instance), __instance.FPSCamera.Forward, 40f, LayerManager.MASK_PING_TARGET, QueryTriggerInteraction.Ignore);

                if (rayCastHits == null || rayCastHits.Length == 0)
                {
                    return;
                }

                float distanceToLockerOrBox = -1;
                foreach (var hit in rayCastHits)
                {
                    var hitTarget = hit.collider.GetComponentInChildren<iPlayerPingTarget>();
                    if (hitTarget == null) continue;
                    //ArchiveLogger.Notice($" -> Hit this: {hit.collider?.gameObject?.name} - {hitTarget?.PingTargetStyle}, distance: {hit.distance}");
                    if (hitTarget?.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceBox || hitTarget?.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceLocker)
                    {
                        distanceToLockerOrBox = hit.distance;
                        break;
                    }
                }

                if (distanceToLockerOrBox < 0)
                {
                    return;
                }

                foreach (var hit in rayCastHits)
                {
                    var hitTarget = hit.collider.GetComponentInChildren<iPlayerPingTarget>();
                    if (hitTarget == null) continue;
                    if (hitTarget.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceBox || hitTarget.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceLocker)
                    {
                        continue;
                    }
                    //ArchiveLogger.Warning($" -> Also Hit this: {hit.collider.gameObject.name} - {hitTarget.PingTargetStyle}, distance: {hit.distance}");
                    if (hit.distance < distanceToLockerOrBox + 1f)
                    {
                        target = hitTarget;
                        worldPos = hit.point;
                        A_PlayerAgent_m_pingTargets.Set(__instance, hitTarget);
                        A_PlayerAgent_m_pingPos.Set(__instance, hit.point);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                FeatureLogger.Exception(ex);
            }
        }
    }

#if IL2CPP
    [RundownConstraint(RundownFlags.RundownSix, RundownFlags.Latest)]
    [ArchivePatch(typeof(PlayerAgent), nameof(PlayerAgent.TriggerMarkerPing))]
    internal static class PlayerAgent_TriggerMarkerPing_Patch
    {
        public static bool Prefix(PlayerAgent __instance, ref iPlayerPingTarget target, ref Vector3 worldPos)
        {
            try
            {
                if (__instance == null || !__instance.IsLocallyOwned || target == null)
                {
                    return ArchivePatch.RUN_OG;
                }

                var localPlayerAgent = __instance.TryCastTo<LocalPlayerAgent>();

                var rayCastHits = Physics.RaycastAll(localPlayerAgent.CamPos, __instance.FPSCamera.Forward, 40f, LayerManager.MASK_PING_TARGET, QueryTriggerInteraction.Ignore);

                if (rayCastHits == null || rayCastHits.Length == 0)
                {
                    return ArchivePatch.RUN_OG;
                }

                float distanceToLockerOrBox = -1;
                foreach (var hit in rayCastHits)
                {
                    var hitTarget = hit.collider.GetComponentInChildren<iPlayerPingTarget>();
                    if (hitTarget == null) continue;
                    //ArchiveLogger.Notice($" -> Hit this: {hit.collider?.gameObject?.name} - {hitTarget?.PingTargetStyle}, distance: {hit.distance}");
                    if (hitTarget?.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceBox || hitTarget?.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceLocker)
                    {
                        distanceToLockerOrBox = hit.distance;
                        break;
                    }
                }

                if (distanceToLockerOrBox < 0)
                {
                    return ArchivePatch.RUN_OG;
                }

                foreach (var hit in rayCastHits)
                {
                    var hitTarget = hit.collider.GetComponentInChildren<iPlayerPingTarget>();
                    if (hitTarget == null) continue;
                    if (hitTarget.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceBox || hitTarget.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceLocker)
                    {
                        continue;
                    }
                    //ArchiveLogger.Warning($" -> Also Hit this: {hit.collider.gameObject.name} - {hitTarget.PingTargetStyle}, distance: {hit.distance}");
                    if (hit.distance < distanceToLockerOrBox + 1f)
                    {
                        target = hitTarget;
                        worldPos = hit.point;
                        localPlayerAgent.m_pingTarget = hitTarget;
                        localPlayerAgent.m_pingPos = hit.point;
                        if (OverrideSoundsAndPings(worldPos, hit.collider.gameObject))
                            return ArchivePatch.SKIP_OG;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                FeatureLogger.Exception(ex);
            }
            return ArchivePatch.RUN_OG;
        }
    }
#else
        [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownThree)]
        [ArchivePatch(typeof(CrosshairGuiLayer), "ShowPingIndicator")]
        internal static class CrosshairGuiLayer_ShowPingIndicatorPatch
        {
            internal static bool ShouldRun { get; set; } = false;
            internal static bool WouldHaveRun { get; set; } = false;
            public static bool Prefix()
            {
                if (!ShouldRun)
                    WouldHaveRun = true;
                return ShouldRun;
            }
        }

        [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownThree)]
        [ArchivePatch(typeof(PlayerAgent), "LateUpdate")]
        internal static class PlayerAgent_LateUpdatePatch
        {

            private static void PlayPingSfxAndStuff(iPlayerPingTarget ___m_pingTarget)
            {
                CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun = true;
                GuiManager.CrosshairLayer.ShowPingIndicator(___m_pingTarget != null);
                CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun = false;
                CrosshairGuiLayer_ShowPingIndicatorPatch.WouldHaveRun = false;
            }

            public static void Postfix(PlayerAgent __instance, ref iPlayerPingTarget ___m_pingTarget, ref Vector3 ___m_pingPos)
            {
                try
                {
                    if (__instance == null || !__instance.IsLocallyOwned || ___m_pingTarget == null)
                    {
                        return;
                    }

                    if (CrosshairGuiLayer_ShowPingIndicatorPatch.WouldHaveRun && !CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun)
                    {
                        var rayCastHits = Physics.RaycastAll(__instance.CamPos, __instance.FPSCamera.Forward, 40f, LayerManager.MASK_PING_TARGET, QueryTriggerInteraction.Ignore);

                        if (rayCastHits == null || rayCastHits.Length == 0)
                        {
                            PlayPingSfxAndStuff(___m_pingTarget);
                            return;
                        }

                        float distanceToLockerOrBox = -1;
                        foreach (var hit in rayCastHits)
                        {
                            var hitTarget = hit.collider.GetComponentInChildren<iPlayerPingTarget>();
                            if (hitTarget == null) continue;
                            //ArchiveLogger.Notice($" -> Hit this: {hit.collider?.gameObject?.name} - {hitTarget?.PingTargetStyle}, distance: {hit.distance}");
                            if (hitTarget?.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceBox || hitTarget?.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceLocker)
                            {
                                distanceToLockerOrBox = hit.distance;
                                break;
                            }
                        }

                        if (distanceToLockerOrBox < 0)
                        {
                            PlayPingSfxAndStuff(___m_pingTarget);
                            return;
                        }

                        foreach (var hit in rayCastHits)
                        {
                            var hitTarget = hit.collider.GetComponentInChildren<iPlayerPingTarget>();
                            if (hitTarget == null) continue;
                            if (hitTarget.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceBox || hitTarget.PingTargetStyle == eNavMarkerStyle.PlayerPingResourceLocker)
                            {
                                continue;
                            }
                            //ArchiveLogger.Warning($" -> Also Hit this: {hit.collider.gameObject.name} - {hitTarget.PingTargetStyle}, distance: {hit.distance}");
                            if (hit.distance < distanceToLockerOrBox + 1f)
                            {
                                //ArchiveLogger.Msg(ConsoleColor.DarkRed, $"Set this as new ping target!");
                                ___m_pingTarget = hitTarget;
                                ___m_pingPos = hit.point;
                                break;
                            }
                        }

                        CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun = true;
                        GuiManager.CrosshairLayer.ShowPingIndicator(___m_pingTarget != null);
                        CrosshairGuiLayer_ShowPingIndicatorPatch.ShouldRun = false;
                    }
                    CrosshairGuiLayer_ShowPingIndicatorPatch.WouldHaveRun = false;
                }
                catch (Exception ex)
                {
                    FeatureLogger.Exception(ex);
                }
            }
        }
#endif
}