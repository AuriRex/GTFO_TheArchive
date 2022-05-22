using Gear;
using System.Reflection;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class MeleeCancelBackportPatches
    {
        [ArchivePatch(typeof(MeleeWeaponFirstPerson), nameof(MeleeWeaponFirstPerson.ChangeState), Utilities.Utils.RundownFlags.RundownOne, Utilities.Utils.RundownFlags.RundownFive)]
        internal static class MeleeWeaponFirstPerson_ChangeStatePatch
        {
            private static eMeleeWeaponState _state_idle = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.Idle));
            private static eMeleeWeaponState _state_none = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.None));
            private static eMeleeWeaponState _state_push = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.Push));
            private static eMeleeWeaponState _state_hitreact = Utils.GetEnumFromName<eMeleeWeaponState>(nameof(eMeleeWeaponState.Hitreact));

            private static MethodInfo _MI_PlayAnim = typeof(MeleeWeaponFirstPerson).GetMethod(nameof(MeleeWeaponFirstPerson.PlayAnim), AnyBindingFlags);

            public static bool Prefix(MeleeWeaponFirstPerson __instance, eMeleeWeaponState newState)
            {
                if(newState == _state_push)
                {
                    if (__instance.CurrentStateName == _state_idle
                        || __instance.CurrentStateName == _state_none)
                    {
                        return true;
                    }

                    __instance.ChangeState(_state_idle);
                    _MI_PlayAnim.Invoke(__instance, new object[] { __instance.CurrentState.m_data.m_animHash, 0f, 0.5f });
                    return false;
                }

                return true;
            }
        }
    }
}
