using CellMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Dev
{
    [EnableFeatureByDefault, HideInModSettings]
    public class ModSettings : Feature
    {
        public override string Name => "Mod Settings (this)";

        public override bool RequiresRestart => true;

        public override string Group => FeatureGroups.Dev;

#if MONO
        private static readonly FieldAccessor<CM_PageSettings, eSettingsSubMenuId> A_CM_PageSettings_m_currentSubMenuId = FieldAccessor<CM_PageSettings, eSettingsSubMenuId>.GetAccessor("m_currentSubMenuId");
        private static readonly MethodAccessor<CM_PageSettings> A_CM_PageSettings_ResetAllValueHolders = MethodAccessor<CM_PageSettings>.GetAccessor("ResetAllValueHolders");
        private static readonly MethodAccessor<CM_PageSettings> A_CM_PageSettings_ShowSettingsWindow = MethodAccessor<CM_PageSettings>.GetAccessor("ShowSettingsWindow");
#endif
        private static MethodAccessor<TMPro.TextMeshPro> A_TextMeshPro_ForceMeshUpdate;

        private static bool _restartRequested = false;

        public static Color ORANGE = new Color(1f, 0.5f, 0.05f, 0.8f);
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
            private static GameObject _settingsItemPrefab;
            private static List<iScrollWindowContent> _scrollWindowContentElements;
            private static Transform _mainScrollWindowTransform;

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
                    _settingsItemPrefab = __instance.m_settingsItemPrefab;
                    if(_scrollWindowContentElements == null)
                    {
                        _scrollWindowContentElements = new List<iScrollWindowContent>();
                    }
                    _scrollWindowContentElements.Clear();

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

                    _mainScrollWindowTransform = mainModSettingsScrollWindow.transform;

                    mainModSettingsScrollWindow.Setup();
                    mainModSettingsScrollWindow.SetSize(new Vector2(1020f, 900f));
                    mainModSettingsScrollWindow.SetVisible(visible: false);
                    mainModSettingsScrollWindow.SetHeader(title);

                    TMPro.TextMeshPro scrollWindowHeaderTextTMP = mainModSettingsScrollWindow.GetComponentInChildren<TMPro.TextMeshPro>();

                    var infoText = GameObject.Instantiate(scrollWindowHeaderTextTMP, scrollWindowHeaderTextTMP.transform.parent);

                    infoText.name = "ModSettings_InfoText";
                    infoText.transform.localPosition += new Vector3(300, 0, 0);
                    infoText.SetText("");
                    infoText.enableWordWrapping = false;
                    infoText.fontSize = 16;
                    infoText.fontSizeMin = 16;


                    var odereredGroups = FeatureManager.Instance.GroupedFeatures.OrderBy(kvp => kvp.Key);

                    foreach(var kvp in odereredGroups)
                    {
                        var groupName = kvp.Key;
                        var featureSet = kvp.Value.OrderBy(fs => fs.Name);

                        if (groupName == FeatureGroups.Dev && !Feature.DevMode)
                            continue;

                        if(!Feature.DevMode && featureSet.All(f => f.IsHidden))
                            continue;

                        // Title
                        CreateHeader(groupName);

                        foreach(var feature in featureSet)
                        {
                            SetupEntriesForFeature(feature, infoText);
                        }

                        // Spacer
                        CreateSpacer();
                    }

                    IEnumerable<Feature> features;
                    if(Feature.DevMode)
                    {
                        features = FeatureManager.Instance.RegisteredFeatures;
                    }
                    else
                    {
                        features = FeatureManager.Instance.RegisteredFeatures.Where(f => !f.IsHidden);
                    }

                    features = features.Where(f => !f.BelongsToGroup);

                    features = features.OrderBy(f => f.GetType().Assembly.GetName().Name + f.Name);

                    Assembly currentAsm = null;
                    bool createSpacer = false;
                    foreach (var feature in features)
                    {
                        var featureAsm = feature.GetType().Assembly;
                        if (featureAsm != currentAsm)
                        {
                            if(createSpacer)
                            {
                                CreateSpacer();
                            }
                            createSpacer = true;
                            var headerTitle = featureAsm.GetCustomAttribute<ModDefaultFeatureGroupName>()?.DefaultGroupName ?? featureAsm.GetName().Name;
                            CreateHeader(headerTitle);
                            currentAsm = featureAsm;
                        }
                        SetupEntriesForFeature(feature, infoText);
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

#if IL2CPP
                    Il2CppSystem.Collections.Generic.List<iScrollWindowContent> allSWCsIL2CPP = new Il2CppSystem.Collections.Generic.List<iScrollWindowContent>();
                    foreach(var swc in _scrollWindowContentElements)
                    {
                        allSWCsIL2CPP.Add(swc);
                    }
                    mainModSettingsScrollWindow.SetContentItems(allSWCsIL2CPP, 5f);
#else
                    mainModSettingsScrollWindow.SetContentItems(_scrollWindowContentElements, 5f);
#endif
                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }

            private static CM_ScrollWindow CreateScrollWindow()
            {
                // TODO
                return null;
            }

            private static void CreateSpacer()
            {
                CreateHeader(string.Empty);
            }

            private static void CreateHeader(string title)
            {
                var settingsItemGameObject = GameObject.Instantiate(_settingsItemPrefab, _mainScrollWindowTransform);

                _scrollWindowContentElements.Add(settingsItemGameObject.GetComponentInChildren<iScrollWindowContent>());

                var cm_settingsItem = settingsItemGameObject.GetComponentInChildren<CM_SettingsItem>();

                var titleTextTMP = cm_settingsItem.transform.GetChildWithExactName("Title").GetChildWithExactName("TitleText").gameObject.GetComponent<TMPro.TextMeshPro>();

                if(!string.IsNullOrWhiteSpace(title))
                {
                    titleTextTMP.SetText($"<b>{title}</b>");
                    titleTextTMP.color = ORANGE;
                    cm_settingsItem.ForcePopupLayer(true);

                    if (BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownFour | Utils.RundownFlags.RundownFive))
                    {
                        titleTextTMP.gameObject.AddComponent<JankTextMeshProUpdaterOnce>();
                    }
                }
                else
                {
                    titleTextTMP.SetText(string.Empty);
                }
            }

            private static void SetupEntriesForFeature(Feature feature, TMPro.TextMeshPro infoText)
            {
                if (!Feature.DevMode && feature.IsHidden) return;

                try
                {
                    var settingsItemGameObject = GameObject.Instantiate(_settingsItemPrefab, _mainScrollWindowTransform);

                    _scrollWindowContentElements.Add(settingsItemGameObject.GetComponentInChildren<iScrollWindowContent>());

                    var cm_settingsItem = settingsItemGameObject.GetComponentInChildren<CM_SettingsItem>();

                    var titleTextTMP = cm_settingsItem.transform.GetChildWithExactName("Title").GetChildWithExactName("TitleText").gameObject.GetComponent<TMPro.TextMeshPro>();

                    string featureName;
                    if (feature.IsHidden)
                    {
                        featureName = $"[H] {feature.Name}";
                        titleTextTMP.color = DISABLED;
                    }
                    else
                    {
                        featureName = feature.Name;
                    }

                    if (feature.RequiresRestart)
                    {
                        featureName = $"<u>{featureName}</u>";
                    }

#if IL2CPP
                    titleTextTMP.m_text = featureName;
                    titleTextTMP.SetText(featureName);
#else
                    titleTextTMP.SetText(featureName);
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

                        if (feature.RequiresRestart && !_restartRequested)
                        {
                            _restartRequested = true;
                            infoText.SetText($"<color=red><b>Restart required for some settings to apply!</b></color>");
                        }
                    });

                    SetFeatureItemTextAndColor(feature, toggleButton_cm_item, toggleButtonText);

                    var collider = toggleButton_cm_item.GetComponent<BoxCollider2D>();
                    collider.size = new Vector2(550, 50);
                    collider.offset = new Vector2(250, -25);

                    cm_settingsItem.ForcePopupLayer(true);

                    if(feature.HasAdditionalSettings)
                    {
                        // Create SubMenu for additional settings
                    }

                }
                catch (Exception ex)
                {
                    ArchiveLogger.Exception(ex);
                }
            }

            private static void SetFeatureItemTextAndColor(Feature feature, CM_Item buttonItem, TMPro.TextMeshPro text)
            {
                bool enabled = (feature.AppliesToThisGameBuild && !feature.RequiresRestart) ? feature.Enabled : FeatureManager.IsEnabledInConfig(feature);
                text.SetText(enabled ? "Enabled" : "Disabled");

                SetFeatureItemColor(feature, buttonItem);
            }

            private static void SetFeatureItemColor(Feature feature, CM_Item item)
            {
                if (!feature.AppliesToThisGameBuild)
                {
                    SharedUtils.ChangeColorCMItem(item, DISABLED);
                    return;
                }

                Color col;

                if (feature.RequiresRestart)
                {
                    col = FeatureManager.IsEnabledInConfig(feature) ? GREEN : RED;
                    SharedUtils.ChangeColorCMItem(item, col);
                    return;
                }

                col = feature.Enabled ? GREEN : RED;
                SharedUtils.ChangeColorCMItem(item, col);
            }
        }
    }
}
