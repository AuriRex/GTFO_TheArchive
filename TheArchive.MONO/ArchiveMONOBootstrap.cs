using Globals;
using HarmonyLib;

namespace TheArchive
{
    public class ArchiveMONOBootstrap
    {

        [HarmonyPatch(typeof(StartMainGame), "Awake")]
        internal static class StartMainGame_AwakePatch
        {
            public static void Postfix()
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ArchiveMONOModule.instance.Core.InvokeGameDataInitialized(Global.RundownIdToLoad);
                ArchiveMONOModule.instance.Core.InvokeDataBlocksReady();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

    }
}
