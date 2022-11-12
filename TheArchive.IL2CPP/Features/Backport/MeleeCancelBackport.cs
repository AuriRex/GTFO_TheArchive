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
        public override string Name => "Modern Melee Cancel";

        public override string Group => FeatureGroups.Backport;

        public override string Description => "The hammer goes back to neutral instead of shoving whenever it's charged and alt-fire is pressed.";

        private static readonly eMeleeWeaponState _state_idle = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.Idle));
        private static readonly eMeleeWeaponState _state_none = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.None));
        private static readonly eMeleeWeaponState _state_push = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.Push));

        [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
        [ArchivePatch(typeof(MeleeWeaponFirstPerson), nameof(MeleeWeaponFirstPerson.ChangeState))]
        internal static class MeleeWeaponFirstPerson_ChangeStatePatch
        {
            private static MethodAccessor<MeleeWeaponFirstPerson> _A_PlayAnim = MethodAccessor<MeleeWeaponFirstPerson>.GetAccessor(nameof(MeleeWeaponFirstPerson.PlayAnim));
            private static FieldAccessor<MWS_Base, MeleeAttackData> _A_FI_m_data;

            [RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownThree)]
            public static void Init()
            {
                _A_FI_m_data = FieldAccessor<MWS_Base, MeleeAttackData>.GetAccessor("m_data");
            }

#if IL2CPP
            [IsPrefix, RundownConstraint(Utils.RundownFlags.RundownFour, Utils.RundownFlags.RundownFive)]
            public static bool PrefixNew(MeleeWeaponFirstPerson __instance, eMeleeWeaponState newState)
            {
                if (newState == _state_push)
                {
                    if (__instance.CurrentStateName == _state_idle
                        || __instance.CurrentStateName == _state_none)
                    {
                        return ArchivePatch.RUN_OG;
                    }

                    __instance.ChangeState(_state_idle);
                    _A_PlayAnim.Invoke(__instance, __instance.CurrentState.m_data.m_animHash, 0f, 0.5f);
                    return ArchivePatch.SKIP_OG;
                }

                return ArchivePatch.RUN_OG;
            }
#endif

#if MONO
            private static eMeleeWeaponState _lastState = eMeleeWeaponState.None;

            // kinda janky but it works
            [IsPrefix, RundownConstraint(Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownThree)]
            public static bool PrefixOld(MeleeWeaponFirstPerson __instance, eMeleeWeaponState newState)
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
                    var m_data = _A_FI_m_data.Get(__instance.CurrentState);
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
