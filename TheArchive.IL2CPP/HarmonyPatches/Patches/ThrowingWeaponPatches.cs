using TheArchive.Core;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.Utils;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableAccidentalThrowablesThrowProtection), "QoL")]
    public class ThrowingWeaponPatches
    {
        public static void DontDownThrowingWeapon(PLOC_Base __instance)
        {
            FirstPersonItemHolder holder = __instance.m_owner.FPItemHolder;

            if (holder.WieldedItem.TryCast<ThrowingWeapon>() != null)
            {
                holder.ItemDownTrigger = false;
            }
        }

        [ArchivePatch(typeof(PLOC_Run), nameof(PLOC_Run.Enter), RundownFlags.RundownFour, RundownFlags.RundownFive)]
        internal static class PLOC_Run_EnterPatch
        {
            public static void Postfix(PLOC_Run __instance) => DontDownThrowingWeapon(__instance);
        }

        [ArchivePatch(typeof(PLOC_Jump), nameof(PLOC_Jump.Enter), RundownFlags.RundownFour, RundownFlags.RundownFive)]
        internal static class PLOC_Jump_EnterPatch
        {
            public static void Postfix(PLOC_Jump __instance) => DontDownThrowingWeapon(__instance);
        }

        [ArchivePatch(typeof(PLOC_Fall), nameof(PLOC_Fall.Enter), RundownFlags.RundownFour, RundownFlags.RundownFive)]
        internal static class PLOC_Fall_EnterPatch
        {
            public static void Postfix(PLOC_Fall __instance) => DontDownThrowingWeapon(__instance);
        }

    }
}
