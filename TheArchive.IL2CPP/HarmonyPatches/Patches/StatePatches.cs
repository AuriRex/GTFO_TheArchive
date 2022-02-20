using HarmonyLib;
using System.Linq;
using System.Reflection;
using static TheArchive.Core.ArchivePatcher;
using static TheArchive.Utilities.RundownFlagsExtensions;

namespace TheArchive.HarmonyPatches.Patches
{
    public class StatePatches
    {
        public static bool LocalPlayerIsInTerminal { get; private set; } = false;

        private static PropertyInfo[] _statePatchesProperties = null;

        public static bool InExpedition { get; private set; } = false;
        public static string ExpeditionTier { get; private set; } = string.Empty; // A, B, C, D, E
        public static string ExpeditionNumber { get; private set; } = string.Empty; // 1, 2, 3, 4

        public static int RundownNumber
        {
            get
            {
                return (int) ArchiveMod.CurrentRundownInt;
            }
        }

        private static void ResetStates()
        {
            if(_statePatchesProperties == null)
            {
                _statePatchesProperties = typeof(StatePatches).GetProperties(AccessTools.all).Where(p => p.PropertyType == typeof(bool) && p.SetMethod.IsStatic).ToArray();
            }

            foreach(var prop in _statePatchesProperties)
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
