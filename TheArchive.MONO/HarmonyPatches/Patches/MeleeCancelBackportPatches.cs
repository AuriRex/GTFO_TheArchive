using Gear;
using System.Reflection;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class MeleeCancelBackportPatches
    {
        [ArchivePatch(typeof(MeleeWeaponFirstPerson), nameof(MeleeWeaponFirstPerson.ChangeState), Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
        internal static class MeleeWeaponFirstPerson_ChangeStatePatch
        {
            private static eMeleeWeaponState _state_idle = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.Idle));
            private static eMeleeWeaponState _state_none = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.None));
            private static eMeleeWeaponState _state_push = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.Push));

            private static MethodInfo _MI_PlayAnim = typeof(MeleeWeaponFirstPerson).GetMethod(nameof(MeleeWeaponFirstPerson.PlayAnim), AnyBindingFlags);
            private static FieldInfo _FI_m_data = typeof(MWS_Base).GetField("m_data", AnyBindingFlags);

            private static eMeleeWeaponState _lastState = eMeleeWeaponState.None;

            // kinda janky but it works
            public static bool Prefix(MeleeWeaponFirstPerson __instance, eMeleeWeaponState newState)
            {
                if (newState == _state_push)
                {
                    if (_lastState == _state_idle
                        || _lastState == _state_none
                        || _lastState == _state_push)
                    {
                        _lastState = __instance.CurrentStateName;
                        return true;
                    }

                    __instance.ChangeState(_state_idle);
                    var m_data = (MeleeAttackData)_FI_m_data.GetValue(__instance.CurrentState);
                    _MI_PlayAnim.Invoke(__instance, new object[] { m_data.m_animHash, 0f, 0.5f });
                    _lastState = _state_idle;
                    return false;
                }

                _lastState = __instance.CurrentStateName;
                return true;
            }
        }
    }
}
