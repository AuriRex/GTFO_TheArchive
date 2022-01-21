using HarmonyLib;
using System.Linq;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    public class StatePatches
    {
        public static bool LocalPlayerIsInTerminal { get; private set; } = false;

        private static void ResetStates()
        {
            foreach(var prop in typeof(StatePatches).GetProperties(AccessTools.all).Where(p => p.PropertyType == typeof(bool) && p.SetMethod.IsStatic))
            {
                prop.SetValue(null, false);
            }
        }

        [ArchivePatch(typeof(FocusStateManager), nameof(FocusStateManager.ChangeState))]
        internal static class FocusStateManager_ChangeStatePatch
        {
            public static void Postfix(eFocusState state)
            {
                ResetStates();

                switch(state)
                {
                    case eFocusState.ComputerTerminal:
                        LocalPlayerIsInTerminal = true;
                        break;
                }
            }
        }

    }
}
