using Gear;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Utilities;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
    public class MeleeCancelBackport : Feature
    {
        public override string Name => "Modern Melee Charge Cancel";

        public override string Group => FeatureGroups.Backport;

        public override string Description => "Returns the hammer back to neutral instead of shoving whenever you're charging and alt-fire is pressed.";

        private static readonly eMeleeWeaponState _state_idle = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.Idle));
        private static readonly eMeleeWeaponState _state_none = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.None));
        private static readonly eMeleeWeaponState _state_push = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.Push));
        private static readonly eMeleeWeaponState _state_attackChargeUpLeft = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.AttackChargeUpLeft));
        private static readonly eMeleeWeaponState _state_attackChargeUpRight = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.AttackChargeUpRight));

        [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
        [ArchivePatch(typeof(MeleeWeaponFirstPerson), nameof(MeleeWeaponFirstPerson.ChangeState))]
        internal static class MeleeWeaponFirstPerson_ChangeState_Patch
        {
            private static MethodAccessor<MeleeWeaponFirstPerson> _A_PlayAnim;
            private static IValueAccessor<MWS_Base, MeleeAttackData> _A_m_data;

            public static void Init()
            {
                _A_PlayAnim = MethodAccessor<MeleeWeaponFirstPerson>.GetAccessor(nameof(MeleeWeaponFirstPerson.PlayAnim));
                _A_m_data = AccessorBase.GetValueAccessor<MWS_Base, MeleeAttackData>("m_data");
            }

            [IsPrefix, RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.RundownFive)]
            public static bool PrefixR4_5(MeleeWeaponFirstPerson __instance, eMeleeWeaponState newState)
            {
                if (newState == _state_push)
                {
                    var currentStateName = __instance.CurrentStateName;

                    if (currentStateName != _state_attackChargeUpLeft
                        && currentStateName != _state_attackChargeUpRight)
                    {
                        return ArchivePatch.RUN_OG;
                    }

                    __instance.ChangeState(_state_idle);
                    var m_data = _A_m_data.Get(__instance.CurrentState);
                    _A_PlayAnim.Invoke(__instance, m_data.m_animHash, 0f, 0.5f);
                    return ArchivePatch.SKIP_OG;
                }

                return ArchivePatch.RUN_OG;
            }

#if MONO
            private static eMeleeWeaponState _lastState = eMeleeWeaponState.None;

            // kinda janky but it works
            // MWS_ChargeUp changes state to idle first, resulting in two state changes (in R3 and below)
            [IsPrefix, RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownThree)]
            public static bool PrefixMonoR3(MeleeWeaponFirstPerson __instance, eMeleeWeaponState newState)
            {
                if (newState == _state_push)
                {
                    if (_lastState == _state_idle
                        || _lastState == _state_none
                        || _lastState == _state_push)
                    {
                        _lastState = __instance.CurrentStateName;
                        return ArchivePatch.RUN_OG;
                    }

                    __instance.ChangeState(_state_idle);
                    var m_data = _A_m_data.Get(__instance.CurrentState);
                    _A_PlayAnim.Invoke(__instance, m_data.m_animHash, 0f, 0.5f);
                    _lastState = _state_idle;
                    return ArchivePatch.SKIP_OG;
                }

                _lastState = __instance.CurrentStateName;
                return ArchivePatch.RUN_OG;
            }
#endif
        }
    }
}
