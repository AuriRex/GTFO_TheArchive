using Gear;
using Player;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.QoL
{
    public class L4DStylePacks : Feature
    {
        public override string Name => "L4D Style Resource Packs";

        public override string Group => FeatureGroups.QualityOfLife;

        public override string Description => "Use left and right mouse buttons to apply resource packs instead of E.\n\nLeft mouse = yourself\nRight mouse = other players";

        public static new IArchiveLogger FeatureLogger { get; set; }

        public static bool IsR6OrLater { get; private set; } = false;

        public override void Init()
        {
            IsR6OrLater = BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownSix.ToLatest());
        }

        public static bool NeedsResource(iResourcePackReceiver receiver, eResourceContainerSpawnType packType)
        {
            if (receiver == null) return false;

            switch (packType)
            {
                case eResourceContainerSpawnType.Health:
                    return receiver.NeedHealth();
                case eResourceContainerSpawnType.AmmoWeapon:
                    return receiver.NeedWeaponAmmo();
                case eResourceContainerSpawnType.AmmoTool:
                    return receiver.NeedToolAmmo();
                case eResourceContainerSpawnType.Disinfection:
                    return receiver.NeedDisinfection();
                default:
                    return false;
            }
        }

        [ArchivePatch(typeof(ResourcePackFirstPerson), "UpdateInteraction")]
        internal static class ResourcePackFirstPerson_UpdateInteraction_Patch
        {
            private static MethodAccessor<ResourcePackFirstPerson> A_UpdateInteractionActionName;

            public static void Init()
            {
                A_UpdateInteractionActionName = MethodAccessor<ResourcePackFirstPerson>.GetAccessor("UpdateInteractionActionName");
            }

            public static bool Prefix(ResourcePackFirstPerson __instance)
            {
                var packReceiver = __instance.m_actionReceiver;
                var lastPackReceiver = __instance.m_lastActionReceiver;

                var packType = __instance.m_packType;
                var timer = __instance.m_interactApplyResource;

                bool anyButtonDown = false;
                InputAction nextInputAction = InputAction.None;

                if (InputMapper.GetButtonDown.Invoke(InputAction.Fire, __instance.Owner.InputFilter))
                {
                    // Give Pack to self (Fire / Left Mouse Button)

                    anyButtonDown = true;

                    if (!timer.TimerIsActive)
                    {
                        packReceiver = __instance.Owner.CastTo<iResourcePackReceiver>();
                        __instance.m_actionReceiver = packReceiver;
                    }

                    nextInputAction = InputAction.Fire;
                }

                if (InputMapper.GetButtonDown.Invoke(InputAction.Aim, __instance.Owner.InputFilter))
                {
                    // Give Pack to others (Aim / Right Mouse Button)

                    anyButtonDown = true;

                    if (!timer.TimerIsActive)
                    {
                        if (Physics.Raycast(__instance.Owner.FPSCamera.Position, __instance.Owner.FPSCamera.Forward, out RaycastHit raycastHit, 2.4f, LayerManager.MASK_GIVE_RESOURCE_PACK))
                        {
                            iResourcePackReceiver componentInParent = raycastHit.collider.GetComponentInParent<iResourcePackReceiver>();
                            if (componentInParent != null)
                            {
                                packReceiver = componentInParent;
                                __instance.m_actionReceiver = packReceiver;
                            }
                        }
                    }

                    if(packReceiver.IsLocallyOwned)
                    {
                        nextInputAction = InputAction.Fire;
                    }
                    else
                    {
                        nextInputAction = InputAction.Aim;
                    }
                }

                if(packReceiver == null || (!timer.TimerIsActive && !anyButtonDown))
                {
                    packReceiver = __instance.Owner.CastTo<iResourcePackReceiver>();
                    __instance.m_actionReceiver = packReceiver;
                }

                if (!timer.TimerIsActive)
                {
                    if (packReceiver != lastPackReceiver)
                    {
                        if (packReceiver.IsLocallyOwned)
                        {
                            A_UpdateInteractionActionName.Invoke(__instance, "YOURSELF");
                            timer.m_input = nextInputAction;
                        }
                        else
                        {
                            A_UpdateInteractionActionName.Invoke(__instance, packReceiver.InteractionName);
                            timer.m_input = nextInputAction;
                        }
                        __instance.m_lastActionReceiver = packReceiver;
                    }
                }

                bool needsResources = NeedsResource(packReceiver, packType);

                bool timerActiveBefore = timer.TimerIsActive;
                timer.ManualUpdateWithCondition(needsResources, __instance.Owner, needsResources && !packReceiver.IsLocallyOwned);
                bool timerActiveAfter = timer.TimerIsActive;

                if (!timerActiveBefore && timerActiveAfter)
                {
                    if(IsR6OrLater)
                    {
                        SendGenericInteractR6Plus(__instance, packReceiver);
                    }
                }

                if(!needsResources && anyButtonDown && __instance.m_lastButtonDown != anyButtonDown)
                {
                    SharedUtils.SafePost(__instance.Sound, AK.EVENTS.BUTTONGENERICBLIPDENIED);
                    ShowDoesNotNeedResourcePrompt(packReceiver, packType);
                }

                __instance.m_lastButtonDown = anyButtonDown;

                return ArchivePatch.SKIP_OG;
            }

            private static void SendGenericInteractR6Plus(ResourcePackFirstPerson __instance, iResourcePackReceiver packReceiver)
            {
                pGenericInteractAnimation.TypeEnum type = packReceiver.IsLocallyOwned ? pGenericInteractAnimation.TypeEnum.ConsumeResource : pGenericInteractAnimation.TypeEnum.GiveResource;
                __instance.Owner.Sync.SendGenericInteract(type, false);
            }

            private static void ShowDoesNotNeedResourcePrompt(iResourcePackReceiver receiver, eResourceContainerSpawnType packType)
            {
                string text = receiver.IsLocallyOwned ? "YOU DO" : (receiver.InteractionName + " DOES");
                switch (packType)
                {
                    case eResourceContainerSpawnType.AmmoWeapon:
                        text += " NOT NEED WEAPON AMMUNITION";
                        break;
                    case eResourceContainerSpawnType.AmmoTool:
                        text += " NOT NEED TOOL AMMUNITION";
                        break;
                    case eResourceContainerSpawnType.Health:
                        text += " NOT NEED MEDICAL RESOURCES";
                        break;
                    case eResourceContainerSpawnType.Disinfection:
                        text += " NOT NEED DISINFECTION";
                        break;
                }
                GuiManager.InteractionLayer.SetTimedInteractionPrompt(text, 1.4f);
            }
        }
    }
}
