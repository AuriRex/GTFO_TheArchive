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
    [EnableFeatureByDefault]
    public class ModSettings : Feature
    {
        public override string Name => "Mod Settings (this)";

#if MONO
        private static readonly FieldAccessor<CM_PageSettings, eSettingsSubMenuId> A_CM_PageSettings_m_currentSubMenuId = FieldAccessor<CM_PageSettings, eSettingsSubMenuId>.GetAccessor("m_currentSubMenuId");
        private static readonly MethodAccessor<CM_PageSettings> A_CM_PageSettings_ResetAllValueHolders = MethodAccessor<CM_PageSettings>.GetAccessor("ResetAllValueHolders");
        private static readonly MethodAccessor<CM_PageSettings> A_CM_PageSettings_ShowSettingsWindow = MethodAccessor<CM_PageSettings>.GetAccessor("ShowSettingsWindow");
#endif
        private static MethodAccessor<TMPro.TextMeshPro> A_TextMeshPro_ForceMeshUpdate;

        public static Color RED = new Color(0.8f, 0.1f, 0.1f, 0.8f);
        public static Color GREEN = new Color(0.1f, 0.8f, 0.1f, 0.8f);
        public static Color DISABLED = new Color(0.3f, 0.3f, 0.3f, 0.8f);


        public class JankTextMeshProUpdaterOnce : MonoBehaviour
        {
#if IL2CPP
            public JankTextMeshProUpdaterOnce(IntPtr ptr) : base(ptr)
            {

            }
#endif

            public void Awake()
            {
                TMPro.TextMeshPro textMesh = this.GetComponent<TMPro.TextMeshPro>();

                A_TextMeshPro_ForceMeshUpdate.Invoke(textMesh);

                Destroy(this);
            }
        }


        public override void Init()
        {
#if IL2CPP
            UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<JankTextMeshProUpdaterOnce>();
#endif

            A_TextMeshPro_ForceMeshUpdate = MethodAccessor<TMPro.TextMeshPro>.GetAccessor("ForceMeshUpdate", new Type[0]);
        }

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

                    CM_Item mainModSettingsButton = guiLayer.AddRectComp(m_subMenuButtonPrefab, GuiAnchor.TopLeft, new Vector2(70f, m_subMenuItemOffset), m_movingContentHolder).TryCastTo<CM_Item>();

                    mainModSettingsButton.SetScaleFactor(0.85f);
                    mainModSettingsButton.UpdateColliderOffset();
#if IL2CPP
                    __instance.m_subMenuItemOffset -= 80f;
#else
                    ___m_subMenuItemOffset -= 80f;
#endif
                    mainModSettingsButton.SetText(title);

                    

                    SharedUtils.ChangeColorCMItem(mainModSettingsButton, Color.magenta);

                    CM_ScrollWindow mainModSettingsScrollWindow = guiLayer.AddRectComp(m_scrollwindowPrefab, GuiAnchor.TopLeft, new Vector2(420f, -200f), m_movingContentHolder).TryCastTo<CM_ScrollWindow>();

                    mainModSettingsScrollWindow.Setup();
                    mainModSettingsScrollWindow.SetSize(new Vector2(1020f, 900f));
                    mainModSettingsScrollWindow.SetVisible(visible: false);
                    mainModSettingsScrollWindow.SetHeader(title);

#if IL2CPP
                    Il2CppSystem.Collections.Generic.List<iScrollWindowContent> allSWCs = new Il2CppSystem.Collections.Generic.List<iScrollWindowContent>();
#else
                    List<iScrollWindowContent> allSWCs = new List<iScrollWindowContent>();
#endif

                    List<TMPro.TextMeshPro> textMeshProToUpdate = new List<TMPro.TextMeshPro>();

                    foreach(var feature in FeatureManager.Instance.RegisteredFeatures)
                    {
                        try
                        {
                            var settingsItemGameObject = GameObject.Instantiate(__instance.m_settingsItemPrefab, mainModSettingsScrollWindow.gameObject.transform);

                            allSWCs.Add(settingsItemGameObject.GetComponentInChildren<iScrollWindowContent>());

                            var cm_settingsItem = settingsItemGameObject.GetComponentInChildren<CM_SettingsItem>();

                            var titleTextTMP = cm_settingsItem.transform.GetChildWithExactName("Title").GetChildWithExactName("TitleText").gameObject.GetComponent<TMPro.TextMeshPro>();



#if IL2CPP
                            titleTextTMP.m_text = feature.Name;
                            titleTextTMP.SetText(feature.Name);
#else
                            titleTextTMP.SetText(feature.Name);
#endif

                            if (BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownFour | Utils.RundownFlags.RundownFive))
                            {
                                titleTextTMP.gameObject.AddComponent<JankTextMeshProUpdaterOnce>();
                            }


                            CM_SettingsToggleButton cm_SettingsToggleButton = GOUtil.SpawnChildAndGetComp<CM_SettingsToggleButton>(cm_settingsItem.m_toggleInputPrefab, cm_settingsItem.m_inputAlign);

                            var toggleButton_cm_item = cm_SettingsToggleButton.gameObject.AddComponent<CM_Item>();



                            Component.Destroy(cm_SettingsToggleButton);

                            var toggleButtonText = toggleButton_cm_item.GetComponentInChildren<TMPro.TextMeshPro>();

                            toggleButton_cm_item.SetCMItemEvents(delegate (int id) {
                                FeatureManager.ToggleFeature(feature);

                                SetFeatureItemTextAndColor(feature, toggleButton_cm_item, toggleButtonText);
                            });

                            SetFeatureItemTextAndColor(feature, toggleButton_cm_item, toggleButtonText);

                            var collider = toggleButton_cm_item.GetComponent<BoxCollider2D>();
                            collider.size = new Vector2(550, 50);
                            collider.offset = new Vector2(250, -25);

                            cm_settingsItem.ForcePopupLayer(true);

                        }
                        catch (Exception ex)
                        {
                            ArchiveLogger.Exception(ex);
                        }
                        
                    }

#if IL2CPP
                    __instance.m_allSettingsWindows.Add(mainModSettingsScrollWindow);
#else
                    ___m_allSettingsWindows.Add(mainModSettingsScrollWindow);
#endif
                    mainModSettingsButton.SetCMItemEvents(delegate (int id)
                    {
                        CM_PageSettings.ToggleAudioTestLoop(false);
                        __instance.ResetAllInputFields();
#if IL2CPP
                        __instance.ResetAllValueHolders();
                        __instance.m_currentSubMenuId = eSettingsSubMenuId.None;
                        __instance.ShowSettingsWindow(mainModSettingsScrollWindow);
#else
                        A_CM_PageSettings_ResetAllValueHolders.Invoke(__instance);
                        A_CM_PageSettings_m_currentSubMenuId.Set(__instance, eSettingsSubMenuId.None);
                        A_CM_PageSettings_ShowSettingsWindow.Invoke(__instance, mainModSettingsScrollWindow);
#endif
                    });

                    mainModSettingsScrollWindow.SetContentItems(allSWCs, 5f);

                }
                catch(Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }

            private static void SetFeatureItemTextAndColor(Feature feature, CM_Item buttonItem, TMPro.TextMeshPro text)
            {
                bool enabled = feature.AppliesToThisGameBuild ? feature.Enabled : FeatureManager.IsEnabledInConfig(feature);
                text.SetText(enabled ? "Enabled" : "Disabled");

                SetFeatureItemColor(feature, buttonItem);
            }

            private static void SetFeatureItemColor(Feature feature, CM_Item item)
            {
                if (!feature.AppliesToThisGameBuild)
                {
                    SharedUtils.ChangeColorCMItem(item, DISABLED);
                }
                else if (feature.Enabled)
                {
                    SharedUtils.ChangeColorCMItem(item, GREEN);
                }
                else
                {
                    SharedUtils.ChangeColorCMItem(item, RED);
                }
            }
        }
    }
}
