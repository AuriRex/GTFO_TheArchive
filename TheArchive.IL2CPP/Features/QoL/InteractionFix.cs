using Gear;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.QoL
{
    [EnableFeatureByDefault]
    public class InteractionFix : Feature
    {
        public override string Name => "Interaction Fix";

        public override string Group => FeatureGroups.QualityOfLife;

        public override string Description => "Prevents resource packs from getting interrupted from other interacts.\nMost notably: running past lockers etc\n(Text might sometimes disappear)";

#if MONO
        private static IValueAccessor<Interact_Timed, float> A_Interact_Timed_m_triggerDuration;
#endif
        private static MethodAccessor<PlayerInteraction> A_UnSelectCurrentBestInteraction;

        public override void Init()
        {
#if MONO
            A_Interact_Timed_m_triggerDuration = AccessorBase.GetValueAccessor<Interact_Timed, float>("m_triggerDuration");
#endif
            A_UnSelectCurrentBestInteraction = MethodAccessor<PlayerInteraction>.GetAccessor("UnSelectCurrentBestInteraction");
        }

        public override void OnDisable()
        {
            PlayerInteraction.CameraRayInteractionEnabled = true;
        }

        [ArchivePatch(typeof(ResourcePackFirstPerson), nameof(ResourcePackFirstPerson.OnUnWield))]
        internal static class ResourcePackFirstPerson_OnUnWield_Patch
        {
            public static void Postfix(ResourcePackFirstPerson __instance)
            {
                PlayerInteraction.CameraRayInteractionEnabled = true;
            }
        }

        [ArchivePatch(typeof(ResourcePackFirstPerson), nameof(UnityMessages.Update))]
        internal static class ResourcePackFirstPerson_Update_Patch
        {
            public static void Prefix(ResourcePackFirstPerson __instance)
            {
                var interaction = __instance.Owner.Interaction;

                // possible TODO: With L4DS RPs disabled, make holding M1/M2 + pressing E use pack instead of doing nothing.
                if (
#if IL2CPP
                    __instance.m_interactApplyResource.TimerIsActive
#else
                    A_Interact_Timed_m_triggerDuration.Get(__instance.m_interactApplyResource) > 0
#endif
                    || __instance.FireButton
                    || __instance.AimButtonHeld)
                {
                    if (interaction.HasWorldInteraction)
                    {
                        A_UnSelectCurrentBestInteraction.Invoke(interaction);
                    }

                    PlayerInteraction.CameraRayInteractionEnabled = false;
                }
                else
                {
                    PlayerInteraction.CameraRayInteractionEnabled = true;
                }
            }
        }
    }
}
