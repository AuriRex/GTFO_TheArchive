using CellMenu;
using System;
using System.Collections.Generic;
using System.Reflection;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features
{
    [EnableFeatureByDefault(true)]
    public class ModSettings : Feature
    {
        public override string Name => "Mod Settings (this)";

#if MONO
        private static readonly FieldAccessor<CM_PageSettings, eSettingsSubMenuId> A_CM_PageSettings_m_currentSubMenuId = FieldAccessor<CM_PageSettings, eSettingsSubMenuId>.GetAccessor("m_currentSubMenuId");
        private static readonly MethodAccessor<CM_PageSettings> A_CM_PageSettings_ResetAllValueHolders = MethodAccessor<CM_PageSettings>.GetAccessor("ResetAllValueHolders");
        private static readonly MethodAccessor<CM_PageSettings> A_CM_PageSettings_ShowSettingsWindow = MethodAccessor<CM_PageSettings>.GetAccessor("ShowSettingsWindow");
#endif

        public static Color RED = new Color(0.8f, 0.1f, 0.1f, 0.8f);
        public static Color GREEN = new Color(0.1f, 0.8f, 0.1f, 0.8f);
        public static Color DISABLED = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        //Setup(MainMenuGuiLayer guiLayer)
        [ArchivePatch(typeof(CM_PageSettings), "Setup")]
        public class CM_PageSettings_SetupPatch
        {
#if IL2CPP
            public static void Postfix(CM_PageSettings __instance, MainMenuGuiLayer guiLayer)
            {
                try
                {
                    var m_subMenuButtonPrefab = __instance.m_subMenuButtonPrefab;
                    var m_subMenuItemOffset = __instance.m_subMenuItemOffset;
                    var m_movingContentHolder = __instance.m_movingContentHolder;
                    var m_scrollwindowPrefab = __instance.m_scrollwindowPrefab;
#else
            public static void Postfix(CM_PageSettings __instance, MainMenuGuiLayer guiLayer, GameObject ___m_subMenuButtonPrefab, ref float ___m_subMenuItemOffset, List<CM_ScrollWindow> ___m_allSettingsWindows)
            {
                var m_subMenuButtonPrefab = ___m_subMenuButtonPrefab;
                var m_subMenuItemOffset = ___m_subMenuItemOffset;
                var m_movingContentHolder = __instance.m_movingContentHolder;
                var m_scrollwindowPrefab = __instance.m_scrollwindowPrefab;
                try
                {
#endif

                    var title = "Mod Settings";

                    CM_Item cM_Item = guiLayer.AddRectComp(m_subMenuButtonPrefab, GuiAnchor.TopLeft, new Vector2(70f, m_subMenuItemOffset), m_movingContentHolder).TryCastTo<CM_Item>();

                    cM_Item.SetScaleFactor(0.85f);
                    cM_Item.UpdateColliderOffset();
#if IL2CPP
                    __instance.m_subMenuItemOffset -= 80f;
#else
                    ___m_subMenuItemOffset -= 80f;
#endif
                    cM_Item.SetText(title);

                    

                    SharedUtils.ChangeColorCMItem(cM_Item, Color.magenta);

                    CM_ScrollWindow window = guiLayer.AddRectComp(m_scrollwindowPrefab, GuiAnchor.TopLeft, new Vector2(420f, -200f), m_movingContentHolder).TryCastTo<CM_ScrollWindow>();

                    window.Setup();
                    window.SetSize(new Vector2(1020f, 900f));
                    window.SetVisible(visible: false);
                    window.SetHeader(title);

#if IL2CPP
                    __instance.m_allSettingsWindows.Add(window);
                    cM_Item.OnBtnPressCallback = (Action<int>)delegate (int id)
                    {
                        CM_PageSettings.ToggleAudioTestLoop(false);
                        __instance.ResetAllInputFields();
                        __instance.ResetAllValueHolders();
                        __instance.m_currentSubMenuId = eSettingsSubMenuId.None;
                        __instance.ShowSettingsWindow(window);
                    };
#else
                    ___m_allSettingsWindows.Add(window);
                    cM_Item.OnBtnPressCallback += delegate (int id)
                    {
                        CM_PageSettings.ToggleAudioTestLoop(false);
                        __instance.ResetAllInputFields();
                        A_CM_PageSettings_ResetAllValueHolders.Invoke(__instance);
                        A_CM_PageSettings_m_currentSubMenuId.Set(__instance, eSettingsSubMenuId.None);
                        A_CM_PageSettings_ShowSettingsWindow.Invoke(__instance, window);
                    };
#endif

#if IL2CPP
                    Il2CppSystem.Collections.Generic.List<iScrollWindowContent> allSWCs = new Il2CppSystem.Collections.Generic.List<iScrollWindowContent>();
#else
                    List<iScrollWindowContent> allSWCs = new List<iScrollWindowContent>();
#endif
                    foreach(var feature in FeatureManager.Instance.RegisteredFeatures)
                    {
                        var go = GameObject.Instantiate(__instance.m_settingsItemPrefab, window.gameObject.transform);

                        //go.transform.localPosition = Vector3.zero;

                        var iScrollWindowContent = go.GetComponentInChildren<iScrollWindowContent>();
                        allSWCs.Add(iScrollWindowContent);

                        go.GetComponentInChildren<TMPro.TextMeshPro>()?.SetText(feature.Name);

                        var settingsItem = go.GetComponentInChildren<CM_SettingsItem>();
                        

                        CM_SettingsToggleButton cm_SettingsToggleButton = GOUtil.SpawnChildAndGetComp<CM_SettingsToggleButton>(settingsItem.m_toggleInputPrefab, settingsItem.m_inputAlign);

                        Component.Destroy(cm_SettingsToggleButton);

                        var buttonItem = cm_SettingsToggleButton.gameObject.AddComponent<CM_Item>();

                        buttonItem.m_onBtnPress = new UnityEngine.Events.UnityEvent();

                        

                        var text = buttonItem.GetComponentInChildren<TMPro.TextMeshPro>();
                        text.SetText(feature.Enabled ? "Enabled" : "Disabled");

                        if(!feature.AppliesToThisGameBuild)
                        {
                            text.color = DISABLED;
                        }
                        else if (feature.Enabled)
                        {
                            text.color = GREEN;
                        }
                        else
                        {
                            text.color = RED;
                        }

                        var collider = buttonItem.GetComponent<BoxCollider2D>();
                        collider.size = new Vector2(1700, 50);
                        collider.offset = new Vector2(-300, -25);

#if IL2CPP
                        buttonItem.OnBtnPressCallback = (Action<int>)delegate (int id)
#else
                        buttonItem.OnBtnPressCallback += delegate (int id)
#endif
                        {
                            FeatureManager.ToggleFeature(feature);
                            text.SetText(feature.Enabled ? "Enabled" : "Disabled");
                            if (!feature.AppliesToThisGameBuild)
                            {
                                text.color = DISABLED;
                            }
                            else if (feature.Enabled)
                            {
                                text.color = GREEN;
                            }
                            else
                            {
                                text.color = RED;
                            }
                        };

                    }

                    window.SetContentItems(allSWCs, 5f);

                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }
        }
    }
}
