using CellMenu;
using HarmonyLib;
using System;
using System.Linq;
using TheArchive.Utilities;

namespace TheArchive.HarmonyPatches.AutoPatches
{
    public class CM_ItemPatches
    {

        /*public static string[] BannedCellMenuItems = new string[]
        {
            "Button Matchmake All"
            //"CM_TimedExpeditionButton(Clone)" // Matchmake button in popup
        };

        [HarmonyPatch(typeof(CM_Item), nameof(CM_Item.OnBtnPress))]
        public class CM_Item_OnBtnPressPatch
        {
            public static bool Prefix(CM_Item __instance)
            {
                if(__instance?.Name != null && (BannedCellMenuItems.Contains(__instance.Name) || __instance.m_texts.Any( tmp => tmp.text.Contains("Matchmaking", StringComparison.OrdinalIgnoreCase))))
                {
                    //__instance.SetButtonEnabled(false);
                    __instance.SetText("<color=red>Button Disabled!</color>");
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CM_ExpeditionWindow), nameof(CM_ExpeditionWindow.Setup))]
        public class CM_ExpeditionWindow_OnBtnPressPatch
        {
            public static void Postfix(CM_ExpeditionWindow __instance)
            {
                __instance.m_matchButton.OnBtnPressCallback = (Il2CppSystem.Action<int>) (Action<int>) ((i) => { ArchiveLogger.Msg(ConsoleColor.Red, $"Clicked that one button! {i}"); });
                __instance.m_matchButton.m_btnText.text = "Clear Expedition";
                __instance.m_matchButton?.SetPosition(new UnityEngine.Vector2(-200000,0));
            }
        }*/

    }

    // https://stackoverflow.com/questions/444798/case-insensitive-containsstring/444818#444818
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
