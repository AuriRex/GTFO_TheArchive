using Player;
using TheArchive.Core;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableAccidentalThrowablesThrowProtection), "QoL")]
    public class ThrowingWeaponPatches
    {
        public static void DontDownThrowingWeapon(FirstPersonItemHolder holder)
        {
            if (holder.WieldedItem is ThrowingWeapon)
            {
                holder.ItemDownTrigger = false;
            }
        }

        [ArchivePatch(typeof(PLOC_Run), nameof(PLOC_Run.Enter))]
        internal static class PLOC_Run_EnterPatch
        {
            public static void Postfix(PlayerAgent ___m_owner) => DontDownThrowingWeapon(___m_owner.FPItemHolder);
        }

        [ArchivePatch(typeof(PLOC_Jump), nameof(PLOC_Jump.Enter))]
        internal static class PLOC_Jump_EnterPatch
        {
            public static void Postfix(PlayerAgent ___m_owner) => DontDownThrowingWeapon(___m_owner.FPItemHolder);
        }

        [ArchivePatch(typeof(PLOC_Fall), nameof(PLOC_Fall.Enter))]
        internal static class PLOC_Fall_EnterPatch
        {
            public static void Postfix(PlayerAgent ___m_owner) => DontDownThrowingWeapon(___m_owner.FPItemHolder);
        }

    }
}
