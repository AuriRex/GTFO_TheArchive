using Gear;
using Player;
using System.Runtime.CompilerServices;
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

        public override FeatureGroup Group => FeatureGroups.QualityOfLife;

        public override string Description => "Use left and right mouse buttons to apply resource packs instead of E.\n\nLeft mouse = yourself\nRight mouse = other players\n\n<color=orange>[R4+]</color> You're able to hold down M2 and it will start applying to a receiver under your croshair if in range automatically\n\n<#f00><u>/!\\</u> Make sure to <color=orange><u>disable</u></color> the vanilla game setting <color=orange>Gameplay > Separate Use Keybinds</color> for this Feature to work!</color>";

        public static new IArchiveLogger FeatureLogger { get; set; }

        private static MethodAccessor<InteractionGuiLayer> A_SetTimedMessage;

        public override void Init()
        {
            if (!Is.R2OrLater)
                A_SetTimedMessage = MethodAccessor<InteractionGuiLayer>.GetAccessor("SetTimedMessage");
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
                    if (Is.R2OrLater)
                        return NeedsDisinfect(receiver);
                    return false;
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool NeedsDisinfect(iResourcePackReceiver receiver) => receiver.NeedDisinfection();

        public static void ShowDoesNotNeedResourcePrompt(iResourcePackReceiver receiver, eResourceContainerSpawnType packType)
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
            SetTimedInteractionPrompt(text, 1.4f);
        }

        private static void SetTimedInteractionPrompt(string text, float time)
        {
            if (Is.R2OrLater)
                TimedInteractionPromptR2Plus(text, time);
            else
                A_SetTimedMessage.Invoke(GuiManager.InteractionLayer, text, time, ePUIMessageStyle.Message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TimedInteractionPromptR2Plus(string text, float time)
        {
            GuiManager.InteractionLayer.SetTimedInteractionPrompt(text, time);
        }

#if MONO
        [RundownConstraint(Utils.RundownFlags.RundownOne)]
        [ArchivePatch(typeof(ResourcePackFirstPerson), "Update")]
        internal static class ResourcePackFirstPerson_Update_Patch
        {
            public static bool Prefix(ResourcePackFirstPerson __instance,
                ref bool ___m_lastSetCanInteract,
                ref float ___m_interactTimer,
                ref iResourcePackReceiver ___m_actionReceiver,
                ref iResourcePackReceiver ___m_lastActionReceiver,
                ref bool ___m_applyPress,
                ref Interact_ManualTimedWithCallback ___m_interactApplyResource)
            {
                bool flag = false;
                if (!__instance.Owner.Interaction.HasWorldInteraction && !__instance.Owner.FPItemHolder.ItemHiddenTrigger && Clock.Time > ___m_interactTimer)
                {
                    ResourcePackFirstPerson_UpdateApplyActionInput_Patch.Prefix(__instance, ref flag, ref ___m_actionReceiver, ref ___m_lastActionReceiver, ref ___m_applyPress, ref ___m_interactApplyResource);
                }
                if (!flag && ___m_lastSetCanInteract)
                {
                    __instance.m_interactApplyResource.SetCanInteract(false);
                    __instance.m_interactApplyResource.SetSelected(false, __instance.Owner);
                }
                else if (flag && !___m_lastSetCanInteract)
                {
                    __instance.m_interactApplyResource.SetCanInteract(true);
                    __instance.m_interactApplyResource.SetSelected(true, __instance.Owner);
                }
                ___m_lastSetCanInteract = flag;

                // base.Update();
                __instance.Sound.UpdatePosition(__instance.transform.position);

                return ArchivePatch.SKIP_OG;
            }
        }

        [RundownConstraint(Utils.RundownFlags.RundownTwo, Utils.RundownFlags.RundownThree)]
        [ArchivePatch(typeof(ResourcePackFirstPerson), "UpdateApplyActionInput")]
        internal static class ResourcePackFirstPerson_UpdateApplyActionInput_Patch
        {
            private static void UpdateInteractionAction(ResourcePackFirstPerson instance, Interact_ManualTimedWithCallback interactApplyResource, string targetName, InputAction input)
            {
                interactApplyResource.SetAction($"Use {instance.PublicName} on <b>{targetName}</b>", input);
            }

            public static bool Prefix(ResourcePackFirstPerson __instance,
                ref bool __result,
                ref iResourcePackReceiver ___m_actionReceiver,
                ref iResourcePackReceiver ___m_lastActionReceiver,
                ref bool ___m_applyPress,
                ref Interact_ManualTimedWithCallback ___m_interactApplyResource)
            {
                ref var packReceiver = ref ___m_actionReceiver;

                var packType = __instance.m_packType;

                InputAction nextInputAction = InputAction.None;

                __result = false;

                bool anyButtonPressed = false;

                if (InputMapper.GetButtonDown.Invoke(InputAction.Fire, __instance.Owner.InputFilter))
                {
                    anyButtonPressed = true;

                    packReceiver = __instance.Owner.CastTo<iResourcePackReceiver>();

                    nextInputAction = InputAction.Fire;
                }

                if (InputMapper.GetButtonDown.Invoke(InputAction.Aim, __instance.Owner.InputFilter))
                {
                    anyButtonPressed = true;

                    if(!___m_applyPress)
                    {
                        if (Physics.Raycast(__instance.Owner.FPSCamera.Position, __instance.Owner.FPSCamera.Forward, out RaycastHit raycastHit, 2.4f, LayerManager.MASK_GIVE_RESOURCE_PACK))
                        {
                            iResourcePackReceiver componentInParent = raycastHit.collider.GetComponentInParent<iResourcePackReceiver>();
                            if (componentInParent != null)
                            {
                                packReceiver = componentInParent;
                            }
                        }
                    }

                    if(packReceiver == null)
                    {
                        packReceiver = __instance.Owner.CastTo<iResourcePackReceiver>();
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

                bool fireHeld = InputMapper.GetButton.Invoke(InputAction.Fire, __instance.Owner.InputFilter);
                bool aimHeld = InputMapper.GetButton.Invoke(InputAction.Aim, __instance.Owner.InputFilter);

                bool needsResource = NeedsResource(packReceiver, packType);

                if(packReceiver != null && anyButtonPressed)
                {
                    if(packReceiver.IsLocallyOwned)
                    {
                        UpdateInteractionAction(__instance, ___m_interactApplyResource, "YOURSELF", nextInputAction);
                    }
                    else
                    {
                        UpdateInteractionAction(__instance, ___m_interactApplyResource, packReceiver.InteractionName, nextInputAction);
                    }
                    ___m_lastActionReceiver = packReceiver;
                }

                if(needsResource)
                {
                    ___m_applyPress = ___m_applyPress || anyButtonPressed;
                    if (___m_applyPress && ((fireHeld && packReceiver.IsLocallyOwned) || (aimHeld && !packReceiver.IsLocallyOwned)))
                    {
                        ___m_interactApplyResource.DoInteract(__instance.Owner);
                        ___m_interactApplyResource.SetCanInteract(true);
                        ___m_interactApplyResource.SetSelected(true, __instance.Owner);
                        __result = true;
                    }
                    else
                    {
                        if(___m_applyPress)
                        {
                            ___m_interactApplyResource.SetActive(false);
                            ___m_interactApplyResource.SetActive(true);
                        }
                        ___m_applyPress = false;
                    }
                }
                else if(anyButtonPressed)
                {
                    SharedUtils.SafePost(__instance.Sound, AK.EVENTS.BUTTONGENERICBLIPDENIED);
                    ShowDoesNotNeedResourcePrompt(packReceiver, packType);
                }

                return ArchivePatch.SKIP_OG;
            }
        }
#else
        [ArchivePatch(typeof(ResourcePackFirstPerson), "UpdateInteraction")]
        internal static class ResourcePackFirstPerson_UpdateInteraction_Patch
        {
            private static MethodAccessor<ResourcePackFirstPerson> A_UpdateInteractionActionName;

            private static IValueAccessor<Interact_Timed, PlayerAgent> A_m_interactionSourceAgent;
            private static IValueAccessor<Interact_Timed, float> A_m_interactionTimer;
            public static void Init()
            {
                A_UpdateInteractionActionName = MethodAccessor<ResourcePackFirstPerson>.GetAccessor("UpdateInteractionActionName");

                if(!Is.R6OrLater)
                {
                    A_m_interactionSourceAgent = AccessorBase.GetValueAccessor<Interact_Timed, PlayerAgent>("m_interactionSourceAgent");
                    A_m_interactionTimer = AccessorBase.GetValueAccessor<Interact_Timed, float>("m_interactionTimer");
                }
            }

            public static bool Prefix(ResourcePackFirstPerson __instance)
            {
                var packReceiver = __instance.m_actionReceiver;
                var lastPackReceiver = __instance.m_lastActionReceiver;

                var packType = __instance.m_packType;
                var timer = __instance.m_interactApplyResource;

                bool anyButtonDown = false;
                bool aimButtonHeld = false;
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

                if (InputMapper.GetButton.Invoke(InputAction.Aim, __instance.Owner.InputFilter))
                {
                    // Give Pack to others (Aim / Right Mouse Button)
                    aimButtonHeld = true;
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
                    if (Is.R6OrLater)
                        OnInteractorStateChangedR6Plus(timer, __instance.Owner, false);
                    timer.PlayerSetSelected(false, __instance.Owner);
                }

                bool needsResources = NeedsResource(packReceiver, packType);

                if (!timer.TimerIsActive)
                {
                    if (packReceiver != lastPackReceiver)
                    {
                        if (packReceiver.IsLocallyOwned)
                        {
                            A_UpdateInteractionActionName.Invoke(__instance, "YOURSELF", true);
                            timer.m_input = nextInputAction;
                        }
                        else
                        {
                            A_UpdateInteractionActionName.Invoke(__instance, packReceiver.InteractionName, false);
                            timer.m_input = nextInputAction;
                            if (needsResources)
                            {
                                if(Is.R6OrLater)
                                {
                                    OnInteractorStateChangedR6Plus(timer, __instance.Owner, true);
                                }
                                else
                                {
                                    ActivateTimerR5AndBelow(timer, __instance.Owner);
                                }
                            }
                        }
                        __instance.m_lastActionReceiver = packReceiver;
                    }
                }

                bool timerActiveBefore = timer.TimerIsActive;
                timer.ManualUpdateWithCondition(needsResources, __instance.Owner, needsResources && !packReceiver.IsLocallyOwned);
                bool timerActiveAfter = timer.TimerIsActive;

                if (!timerActiveBefore && timerActiveAfter)
                {
                    if(Is.R6OrLater)
                    {
                        SendGenericInteractR6Plus(__instance, packReceiver);
                    }
                }

                if(timerActiveBefore && !timerActiveAfter)
                {
                    __instance.m_actionReceiver = __instance.Owner?.CastTo<iResourcePackReceiver>();
                }

                if(!needsResources
                    && anyButtonDown
                    && __instance.m_lastButtonDown != anyButtonDown
                    && !(aimButtonHeld && packReceiver.IsLocallyOwned))
                {
                    SharedUtils.SafePost(__instance.Sound, AK.EVENTS.BUTTONGENERICBLIPDENIED);
                    ShowDoesNotNeedResourcePrompt(packReceiver, packType);
                }

                __instance.m_lastButtonDown = anyButtonDown;

                return ArchivePatch.SKIP_OG;
            }

            private static void ActivateTimerR5AndBelow(Interact_ManualTimedWithCallback timer, PlayerAgent agent)
            {
                A_m_interactionSourceAgent.Set(timer, agent);
                timer.SetTimerActive(true);
                A_m_interactionTimer.Set(timer, Clock.Delta);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void SendGenericInteractR6Plus(ResourcePackFirstPerson __instance, iResourcePackReceiver packReceiver)
            {
                pGenericInteractAnimation.TypeEnum type = packReceiver.IsLocallyOwned ? pGenericInteractAnimation.TypeEnum.ConsumeResource : pGenericInteractAnimation.TypeEnum.GiveResource;
                __instance.Owner.Sync.SendGenericInteract(type, false);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void OnInteractorStateChangedR6Plus(Interact_ManualTimedWithCallback timer, PlayerAgent sourceAgent, bool state)
            {
                timer.OnInteractorStateChanged(sourceAgent, state);
            }
        }
#endif
    }
}
