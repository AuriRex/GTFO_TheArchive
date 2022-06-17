using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Backport
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownFive)]
    public class ThrowablesRunFix : Feature
    {
        public override string Name => "Throwables Run / Fall Fix";

        public override string Description => "Prevents you from accidentally throwing your C-Foam nade / Glowsticks whenever you start running / jumping";

        public static void DontDownThrowingWeapon(PLOC_Base __instance)
        {
            FirstPersonItemHolder holder = __instance.m_owner.FPItemHolder;

#if IL2CPP
            if (holder.WieldedItem.TryCast<ThrowingWeapon>() != null)
#else
            if (holder.WieldedItem is ThrowingWeapon)
#endif
            {
                holder.ItemDownTrigger = false;
            }
        }

        [ArchivePatch(typeof(PLOC_Run), nameof(PLOC_Run.Enter))]
        internal static class PLOC_Run_EnterPatch
        {
            public static void Postfix(PLOC_Run __instance) => DontDownThrowingWeapon(__instance);
        }

        [ArchivePatch(typeof(PLOC_Jump), nameof(PLOC_Jump.Enter))]
        internal static class PLOC_Jump_EnterPatch
        {
            public static void Postfix(PLOC_Jump __instance) => DontDownThrowingWeapon(__instance);
        }

        [ArchivePatch(typeof(PLOC_Fall), nameof(PLOC_Fall.Enter))]
        internal static class PLOC_Fall_EnterPatch
        {
            public static void Postfix(PLOC_Fall __instance) => DontDownThrowingWeapon(__instance);
        }
    }
}
