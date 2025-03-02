using Player;
using System;
using System.Runtime.CompilerServices;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;
using UnityEngine;
using static AirParticleSystem.AirParticleSystem;

namespace TheArchive.Features.Accessibility;

public class DisableAmbientParticles : Feature
{
    public override string Name => "Disable Ambient Particles";

    public override FeatureGroup Group => FeatureGroups.Accessibility;

    public override string Description => "Disable the little floating dust particles in the air.";

    public override void OnEnable()
    {
        if (Is.R6OrLater)
        {
            var particles = GetAmbientParticlesR6Plus();
            if (particles != null)
            {
                particles.enabled = false;
            }
        }
        else
        {
            OnEnableR5Below();
        }
    }

    public override void OnDisable()
    {
        if (IsApplicationQuitting) return;

        if (Is.R6OrLater)
        {
            var particles = GetAmbientParticlesR6Plus();
            if (particles != null)
            {
                particles.enabled = true;
            }
        }
        else
        {
            OnDisableR5Below();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void OnEnableR5Below()
    {
        State = AirParticleState.Disabled;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void OnDisableR5Below()
    {
        eGameStateName currentState = (eGameStateName)CurrentGameState;

        if (currentState == eGameStateName.InLevel)
        {
            State = AirParticleState.InLevel;
        }

        if (currentState == eGameStateName.Generating
            || currentState == eGameStateName.ReadyToStopElevatorRide
            || currentState == eGameStateName.StopElevatorRide)
        {
            State = AirParticleState.InElevator;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static MonoBehaviour GetAmbientParticlesR6Plus()
    {
        var localPlayer = PlayerManager.GetLocalPlayerAgent();

#if IL2CPP
        return localPlayer?.FPSCamera?.gameObject?.GetComponent<AmbientParticles>();
#else
            return null;
#endif
    }

#if IL2CPP
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    [ArchivePatch(UnityMessages.Awake)]
    internal static class AmbientParticles_Awake_Patch
    {
        public static Type Type() => typeof(AmbientParticles);

        public static void Postfix(AmbientParticles __instance)
        {
            __instance.enabled = false;
        }
    }
#endif

    [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
    [ArchivePatch(typeof(AirParticleSystem.AirParticleSystem), nameof(State), patchMethodType: ArchivePatch.PatchMethodType.Setter)]
    internal static class AirParticleSystem_State_Patch
    {
        public static void Prefix(ref AirParticleState value)
        {
            value = AirParticleState.Disabled;
        }
    }
}