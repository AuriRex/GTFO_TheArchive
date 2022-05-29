using CellMenu;
using SNetwork;
using TheArchive.Core;
using TheArchive.Utilities;
using static TheArchive.Core.ArchivePatcher;

namespace TheArchive.HarmonyPatches.Patches
{
    [BindPatchToSetting(nameof(ArchiveSettings.EnableQualityOfLifeImprovements), "QOL")]
    internal class UnreadyButtonPatches
    {
        [ArchivePatch(typeof(CM_PageLoadout), "UpdateReadyButton", Utils.RundownFlags.RundownOne, Utils.RundownFlags.RundownFive)]
        internal static class CM_PageLoadout_UpdateReadyButtonPatch
        {
#if IL2CPP
            public static void Postfix(CM_PageLoadout __instance)
            {
                var readyButton = __instance.m_readyButton;
#else
            public static void Postfix(CM_TimedButton ___m_readyButton)
            {
                var readyButton = ___m_readyButton;
#endif
                if (!SNet.IsInLobby)
                    return;

                if (SNet.IsMaster)
                    return;

                if (GameStateManager.IsReady)
                {
                    readyButton.SetText("UNREADY");
                    readyButton.SetButtonEnabled(true);
                }
            }

        }
    }
}
