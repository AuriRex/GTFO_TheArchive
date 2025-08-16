using Gear;
using HarmonyLib;
using Player;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.Fixes;

[EnableFeatureByDefault]
public class InteractionFix : Feature
{
    public override string Name => "Interaction Fix";

    public override FeatureGroup Group => FeatureGroups.Fixes;

    public override string Description => "Prevents resource packs from getting interrupted from other interacts.\nMost notably: running past lockers etc\n\n(Text might sometimes disappear)";

#if MONO
        private static IValueAccessor<Interact_ManualTimedWithCallback, InputAction> A_Interact_ManualTimedWithCallback_m_input;
        private static IValueAccessor<Interact_Timed, float> A_Interact_Timed_m_triggerDuration;
        private static MethodAccessor<Interact_ManualTimedWithCallback> A_Interact_ManualTimedWithCallback_ShowInteractionText;
        private static IValueAccessor<Interact_Timed, float> A_Interact_Timed_m_lastTimeRel;
#endif
    private static MethodAccessor<PlayerInteraction> A_UnSelectCurrentBestInteraction;

    public override void Init()
    {
#if MONO
            //private InputAction m_input;
            A_Interact_ManualTimedWithCallback_m_input = AccessorBase.GetValueAccessor<Interact_ManualTimedWithCallback, InputAction>("m_input");
            A_Interact_Timed_m_triggerDuration = AccessorBase.GetValueAccessor<Interact_Timed, float>("m_triggerDuration");
            A_Interact_ManualTimedWithCallback_ShowInteractionText = MethodAccessor<Interact_ManualTimedWithCallback>.GetAccessor("ShowInteractionText");
            A_Interact_Timed_m_lastTimeRel = AccessorBase.GetValueAccessor<Interact_Timed, float>("m_lastTimeRel");
#endif
        A_UnSelectCurrentBestInteraction = MethodAccessor<PlayerInteraction>.GetAccessor("UnSelectCurrentBestInteraction");
    }

    public override void OnDisable()
    {
        CameraRayInteractionEnabled = true;
    }

    private static bool _cameraRayInteractionEnabled = true;
    public static bool CameraRayInteractionEnabled
    {
        get
        {
            if (Is.R2OrLater)
                return GetR2PCameraRayInteractionEnabled();
            return _cameraRayInteractionEnabled;
        }
        set
        {
            if (Is.R2OrLater)
            {
                SetR2PCameraRayInteractionEnabled(value);
                return;
            }
            _cameraRayInteractionEnabled = value;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SetR2PCameraRayInteractionEnabled(bool enabled)
    {
        PlayerInteraction.CameraRayInteractionEnabled = enabled;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool GetR2PCameraRayInteractionEnabled()
    {
        return PlayerInteraction.CameraRayInteractionEnabled;
    }

    [RundownConstraint(Utils.RundownFlags.RundownOne)]
    [ArchivePatch(typeof(PlayerInteraction), "UpdateWorldInteractions")]
    internal static class PlayerInteraction_UpdateWorldInteractions_Patch
    {
        private static MethodInfo _mi_get_CameraRayInteractionEnabled;
        private static FieldInfo _fi_FPSCamera;

        public static void Init()
        {
            _mi_get_CameraRayInteractionEnabled = typeof(InteractionFix).GetProperty(nameof(CameraRayInteractionEnabled), Utils.AnyBindingFlagss).GetGetMethod();
            _fi_FPSCamera = typeof(PlayerAgent).GetField(nameof(PlayerAgent.FPSCamera), Utils.AnyBindingFlagss);
        }

        // This Transpiler adds the CameraRayInteractionEnabled check into R1 code
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool doThings = true;
            bool loadFPSCamFound = false;
            Label labelToBranchTo;
            foreach (var instruction in instructions)
            {
                if (doThings)
                {
                    // Search for loading of field PlayerAgent.FPSCamera
                    if (instruction.LoadsField(_fi_FPSCamera))
                    {
                        loadFPSCamFound = true;
                    }
                    // Seek to the next brfalse instruction
                    if (loadFPSCamFound)
                    {
                        if (instruction.opcode == OpCodes.Brfalse)
                        {
                            // This is the label that should be jumped to if we want to skip looking for interactables,
                            // so we branch if CameraRayInteractionEnabled returns false
                            labelToBranchTo = (Label)instruction.operand;
                            yield return instruction;
                            yield return new CodeInstruction(OpCodes.Call, _mi_get_CameraRayInteractionEnabled);
                            yield return new CodeInstruction(OpCodes.Brfalse, labelToBranchTo);
                            doThings = false;
                            continue;
                        }
                    }
                }

                yield return instruction;
            }
        }
    }

    [ArchivePatch(typeof(ResourcePackFirstPerson), nameof(ResourcePackFirstPerson.OnUnWield))]
    internal static class ResourcePackFirstPerson_OnUnWield_Patch
    {
        public static void Postfix()
        {
            CameraRayInteractionEnabled = true;
        }
    }

    [ArchivePatch(typeof(ResourcePackFirstPerson), nameof(UnityMessages.Update))]
    internal static class ResourcePackFirstPerson_Update_Patch
    {
        public static void Prefix(ResourcePackFirstPerson __instance)
        {
            var interaction = __instance.Owner.Interaction;
            var timedInteract = __instance.m_interactApplyResource;

            // possible TODO: With L4DS RPs disabled, make holding M1/M2 + pressing E use pack instead of doing nothing.
            if (
#if IL2CPP
                timedInteract.TimerIsActive
#else
                    A_Interact_Timed_m_triggerDuration.Get(timedInteract) > 0
                    && InputMapper.GetButton(A_Interact_ManualTimedWithCallback_m_input.Get(timedInteract), __instance.Owner.InputFilter)
#endif
                || __instance.FireButton
                || __instance.AimButtonHeld)
            {
                if (interaction.HasWorldInteraction)
                {
                    A_UnSelectCurrentBestInteraction.Invoke(interaction);

#if MONO
                        A_Interact_ManualTimedWithCallback_ShowInteractionText.Invoke(timedInteract);
                        GuiManager.InteractionLayer.SetTimer(A_Interact_Timed_m_lastTimeRel.Get(timedInteract));
                        GuiManager.InteractionLayer.InteractPromptVisible = true;
#endif

#if IL2CPP
                    timedInteract.OnSelectedChange(true, __instance.Owner, true);
                    timedInteract.OnTimerUpdate(timedInteract.InteractionTimerRel);
#endif
                }

                CameraRayInteractionEnabled = false;
            }
            else
            {
                CameraRayInteractionEnabled = true;
#if MONO
                    A_Interact_Timed_m_triggerDuration.Set(timedInteract, 0f);
#endif
            }
        }
    }
}