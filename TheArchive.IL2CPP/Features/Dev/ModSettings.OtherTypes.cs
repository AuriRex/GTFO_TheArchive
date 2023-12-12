using CellMenu;
using System;
using System.Collections.Generic;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Features.Dev.ModSettings.PageSettingsData;
using static TheArchive.Features.Dev.ModSettings.SettingsCreationHelper;
using static TheArchive.Utilities.SColorExtensions;
using TheArchive.Core.FeaturesAPI;
#if Unhollower
using UnhollowerBaseLib.Attributes;
#endif
#if Il2CppInterop
using Il2CppInterop.Runtime.Attributes;
#endif

namespace TheArchive.Features.Dev
{
    public partial class ModSettings
    {
        public class KeyListener : MonoBehaviour
        {
            public static readonly KeyCode[] allKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));

#if IL2CPP
            public KeyListener(IntPtr ptr) : base(ptr) { }
#endif

#if IL2CPP
            [HideFromIl2Cpp]
#endif
            public KeySetting ActiveKeySetting { get; private set; } = null;

#if IL2CPP
            [HideFromIl2Cpp]
#endif
            public void StartListening(KeySetting setting)
            {
                FeatureLogger.Debug($"[{nameof(KeyListener)}] Starting listener, disabling all input!");
                setting.UpdateKeyText("<#F00><b>Press any Key!</b></color>");
                ActiveKeySetting = setting;
                FeatureManager.EnableAutomatedFeature(typeof(InputDisabler));
                enabled = true;
            }

            public void StopListening()
            {
                KeyCode currentOrNone = KeyCode.None;

                if (ActiveKeySetting != null)
                {
                    currentOrNone = ActiveKeySetting.GetCurrent();
                }

                OnKeyFound(currentOrNone);
            }

            public void OnKeyFound(KeyCode key)
            {
                if (key == KeyCode.Escape)
                {
                    key = KeyCode.None;
                }

                if(ActiveKeySetting != null)
                {
                    FeatureLogger.Debug($"[{nameof(KeyListener)}] Key found (\"{key}\"), re-enabling all input!");
                    ActiveKeySetting.SetValue(key);
                }

                ActiveKeySetting = null;
                
                FeatureManager.DisableAutomatedFeature(typeof(InputDisabler));
                enabled = false;
            }

            public void Update()
            {
                if (ActiveKeySetting == null)
                {
                    OnKeyFound(KeyCode.None);
                    return;
                }

                foreach (var key in allKeys)
                {
                    if (Input.GetKeyDown(key))
                    {
                        OnKeyFound(key);
                        break;
                    }
                }
            }

            public void OnDisable()
            {
                StopListening();
            }

            public void OnDestroy()
            {
                if (IsApplicationQuitting)
                    return;

                StopListening();
            }

            public static void ActivateKeyListener(KeySetting setting)
            {
                var settingsGO = PageSettingsData.SettingsPageInstance.gameObject;

                var listener = settingsGO.GetComponent<KeyListener>() ?? settingsGO.AddComponent<KeyListener>();

                listener.StartListening(setting);
            }
        }

        public class OnDisabledListener : MonoBehaviour
        {
#if IL2CPP
            public OnDisabledListener(IntPtr ptr) : base(ptr) { }
#endif

            public Action<GameObject> OnDisabledSelf;

            public void OnDisable()
            {
                OnDisabledSelf?.Invoke(gameObject);
            }
        }

        public class OnEnabledListener : MonoBehaviour
        {
#if IL2CPP
            public OnEnabledListener(IntPtr ptr) : base(ptr) { }
#endif

            public Action<GameObject> OnEnabledSelf;

            public void OnEnable()
            {
                OnEnabledSelf?.Invoke(gameObject);
            }
        }

        public class JankTextMeshProUpdaterOnce : MonoBehaviour
        {
#if IL2CPP
            public JankTextMeshProUpdaterOnce(IntPtr ptr) : base(ptr) { }
#endif
            public void Awake()
            {
                UpdateMesh(this.GetComponent<TMPro.TextMeshPro>());
                Destroy(this);
            }

            public static void UpdateMesh(TMPro.TextMeshPro textMesh)
            {
                if (Is.R4 || Is.R5)
                {
                    ForceUpdateMesh(textMesh);
                }
            }

            public static void ForceUpdateMesh(TMPro.TextMeshPro textMesh)
            {
                if (textMesh == null)
                    return;

                if (Is.R6OrLater)
                {
                    A_TextMeshPro_ForceMeshUpdate.Invoke(textMesh, true, false);
                }
                else
                {
                    A_TextMeshPro_ForceMeshUpdate.Invoke(textMesh);
                }
            }

#if IL2CPP
            [HideFromIl2Cpp]
#endif
            public static JankTextMeshProUpdaterOnce Apply(TMPro.TextMeshPro tmp)
            {
                if (Is.R4 || Is.R5)
                {
                    return tmp.gameObject.AddComponent<JankTextMeshProUpdaterOnce>();
                }
                return null;
            }
        }

        public class ColorPicker : IDisposable
        {
            public bool IsActive => _backgroundPanel?.gameObject?.activeInHierarchy ?? false;

            private readonly CM_ScrollWindow _backgroundPanel;
            private readonly TMPro.TextMeshPro _headerText;

            private SpriteRenderer _previewRenderer;
            private ColorSetting _currentSetting;
            private SpriteRenderer _currentSetting_previewRenderer;

            private CM_SettingsInputField _hexField;

            private float _hue = 0.5f;
            public float Hue
            {
                get => _hue;
                private set => _hue = value;
            }

            private float _saturation = 0.5f;
            public float Saturation
            {
                get => _saturation;
                private set => _saturation = value;
            }

            private float _value = 0.5f;
            public float Value
            {
                get => _value;
                private set => _value = value;
            }

            public Color CurrentColor => Color.HSVToRGB(Hue, Saturation, Value);

            private readonly CM_SettingScrollReceiver _sr_hue;
            private readonly CM_SettingScrollReceiver _sr_saturation;
            private readonly CM_SettingScrollReceiver _sr_value;

            public ColorPicker()
            {
                _backgroundPanel = CreateScrollWindow("Color Picker");
                _backgroundPanel.transform.localPosition = _backgroundPanel.transform.localPosition + new Vector3(1050, 0, 0);


                var headerItemGO = GameObject.Instantiate(SettingsItemPrefab, _backgroundPanel.transform);

                _headerText = headerItemGO.GetComponentInChildren<CM_SettingsItem>().transform.GetChildWithExactName("Title").GetChildWithExactName("TitleText").gameObject.GetComponent<TMPro.TextMeshPro>();

                _headerText.color = ORANGE;

                _headerText.SetText("Header Text");

                var rectTrans = _headerText.gameObject.GetComponent<RectTransform>();

                rectTrans.sizeDelta = new Vector2(rectTrans.sizeDelta.x * 2, rectTrans.sizeDelta.y);


                CreateHeader(string.Empty, out var settingsItemPreview, clickAction: (_) => { }, hoverAction: (_, _) => {}, placeInNoMenu: true);

                settingsItemPreview.ForcePopupLayer(true);

                settingsItemPreview.SafeDestroy();

                var bg = settingsItemPreview.transform.GetChildWithExactName("Background");
                _previewRenderer = bg.GetComponent<SpriteRenderer>();

                #region COLOR_PICKER_HUE_SLIDER
                var setValueHue = new Action<float>((val) =>
                {
                    //FeatureLogger.Debug($"Setting Hue to: {val}");
                    Hue = val;
                    UpdatePreviewColor();
                });

                var getValueHue = new Func<float, float>((oldValOrZero) =>
                {
                    return Hue;
                });

                CreateSimpleNumberField("Hue", .5f, setValueHue, out var settingsItemHue, out _, out _sr_hue, getValueHue, new FSSlider(0, 1), placeInNoMenu: true);
                #endregion COLOR_PICKER_HUE_SLIDER

                #region COLOR_PICKER_SAT_SLIDER
                var setValueSaturation = new Action<float>((val) =>
                {
                    //FeatureLogger.Debug($"Setting Saturation to: {val}");
                    Saturation = val;
                    UpdatePreviewColor();
                });

                var getValueSaturation = new Func<float, float>((oldValOrZero) =>
                {
                    return Saturation;
                });

                CreateSimpleNumberField("Saturation", .5f, setValueSaturation, out var settingsItemSaturation, out _, out _sr_saturation, getValueSaturation, new FSSlider(0, 1), placeInNoMenu: true);
                #endregion COLOR_PICKER_SAT_SLIDER

                #region COLOR_PICKER_VAL_SLIDER
                var setValueValue = new Action<float>((val) =>
                {
                    //FeatureLogger.Debug($"Setting Value to: {val}");
                    Value = val;
                    UpdatePreviewColor();
                });

                var getValueValue = new Func<float, float>((oldValOrZero) =>
                {
                    return Value;
                });

                CreateSimpleNumberField("Value", .5f, setValueValue, out var settingsItemValue, out _, out _sr_value, getValueValue, new FSSlider(0, 1), placeInNoMenu: true);
                #endregion COLOR_PICKER_VAL_SLIDER



                CreateSimpleButton("Apply Color", $"<{GREEN.ToHexString()}>Apply</color>", OnApplyPress, out var settingsItemApply, placeInNoMenu: true);

                CreateSpacer(out var settingsItemSpacer, placeInNoMenu: true);

                CreateSimpleButton("Close Color Picker", "Cancel", OnCancelPress, out var settingsItemCancel, placeInNoMenu: true);

                CreateSpacer(out var settingsItemSpacer2, placeInNoMenu: true);
                CreateSpacer(out var settingsItemSpacer3, placeInNoMenu: true);

                var setValueHexCode = new Action<string>((val) =>
                {
                    FeatureLogger.Debug($"Setting Value to: {val}");

                    if (!TryGetColorFromHexString(val, out var color))
                        return;

                    Color.RGBToHSV(color, out _hue, out _saturation, out _value);

                    ResetAllScrollReceivers();

                    UpdatePreviewColor();
                });

                var getValueHexCode = new Func<string, string>((oldValOrZero) =>
                {
                    return CurrentColor.ToHexString();
                });

                CreateSimpleTextField("Hex Code", "#COLORS", onValueUpdated: setValueHexCode, out var settingsItemHexField, out _hexField, getValue: getValueHexCode, maxLength: 7, null, null, placeInNoMenu: true);

                CreateSimpleButton("Copy Color Hex Code", "Copy", OnCopyColor, out var settingsItemCopyColor, placeInNoMenu: true);
                CreateSimpleButton("Paste Color Hex Code", "Paste", OnPasteColor, out var settingsItemPasteColor, placeInNoMenu: true);

                _backgroundPanel.SetContentItems(new List<iScrollWindowContent>()
                {
                    headerItemGO.GetComponentInChildren<iScrollWindowContent>(),

                    settingsItemPreview.GetComponentInChildren<iScrollWindowContent>(),

                    settingsItemHue.GetComponentInChildren<iScrollWindowContent>(),
                    settingsItemSaturation.GetComponentInChildren<iScrollWindowContent>(),
                    settingsItemValue.GetComponentInChildren<iScrollWindowContent>(),

                    settingsItemApply.GetComponentInChildren<iScrollWindowContent>(),

                    settingsItemSpacer.GetComponentInChildren<iScrollWindowContent>(),

                    settingsItemCancel.GetComponentInChildren<iScrollWindowContent>(),

                    settingsItemSpacer2.GetComponentInChildren<iScrollWindowContent>(),
                    settingsItemSpacer3.GetComponentInChildren<iScrollWindowContent>(),

                    settingsItemHexField.GetComponentInChildren<iScrollWindowContent>(),

                    settingsItemCopyColor.GetComponentInChildren<iScrollWindowContent>(),
                    settingsItemPasteColor.GetComponentInChildren<iScrollWindowContent>(),

                    //contentItemGO.GetComponentInChildren<iScrollWindowContent>()
                }.ToIL2CPPListIfNecessary(), 5);

                AddToAllSettingsWindows(_backgroundPanel);
            }

            private static bool TryGetColorFromHexString(string colorString, out Color color)
            {
                color = Color.white;
                colorString = colorString?.Trim();

                if (string.IsNullOrWhiteSpace(colorString))
                    return false;

                if (colorString.Length > 7)
                    return false;

                colorString = EnsureLeadingHash(colorString);

                if (ColorUtility.TryParseHtmlString(colorString, out color))
                    return true;

                return false;
            }

            private void OnPasteColor()
            {
                var colorString = GUIUtility.systemCopyBuffer;

                if (!TryGetColorFromHexString(colorString, out var color))
                    return;

                FeatureLogger.Info($"Pasted in color from clipboard: {colorString}");

                Color.RGBToHSV(color, out _hue, out _saturation, out _value);

                ResetAllScrollReceivers();
            }

            private void OnCopyColor()
            {
                var color = Color.HSVToRGB(Hue, Saturation, Value);

                var colorString = color.ToHexString();

                GUIUtility.systemCopyBuffer = colorString;

                FeatureLogger.Info($"Copied color to clipboard: {colorString}");
            }

            private void OnApplyPress()
            {
                if (_currentSetting == null)
                    return;

                var color = CurrentColor;

                _currentSetting.SetValue(color.ToSColor());
                _currentSetting_previewRenderer.color = color;

                Hide();
            }

            private void OnCancelPress()
            {
                Hide();
            }

            private void ResetAllScrollReceivers()
            {
                ResetScrollReceiver(_sr_hue, Hue);
                ResetScrollReceiver(_sr_saturation, Saturation);
                ResetScrollReceiver(_sr_value, Value);
            }

            private void ResetScrollReceiver(CM_SettingScrollReceiver receiver, float value)
            {
                CM_SettingScrollReceiver_GetFloatDisplayText_Patch.OverrideDisplayValue = true;
                CM_SettingScrollReceiver_GetFloatDisplayText_Patch.Value = value;

                receiver?.ResetValue();
            }

            private void UpdatePreviewColor()
            {
                _previewRenderer.color = CurrentColor;

                _hexField.ResetValue();
            }

            public void Show(ColorSetting setting)
            {
                _backgroundPanel.gameObject.SetActive(true);

                var colorPreviewGO = CustomExtensions.FindChildRecursive(((CM_SettingsItem)setting.CM_SettingsItem).transform, COLOR_PREVIEW_NAME);

                _currentSetting = setting;
                _currentSetting_previewRenderer = colorPreviewGO.GetComponent<SpriteRenderer>();

                _headerText.SetText(GetNameForSetting(setting));
                JankTextMeshProUpdaterOnce.Apply(_headerText);

                var color = ((SColor)setting.GetValue()).ToUnityColor();

                Color.RGBToHSV(color, out _hue, out _saturation, out _value);

                ResetAllScrollReceivers();
                UpdatePreviewColor();
            }

            public void Hide()
            {
                _backgroundPanel.gameObject.SetActive(false);
            }

            public void Dispose()
            {
                RemoveFromAllSettingsWindows(_backgroundPanel);
                _backgroundPanel.SafeDestroyGO();
            }
        }

        public class DescriptionPanel : IDisposable
        {
            public class DescriptionPanelData
            {
                public string Title;
                public string Description;
                public string CriticalInfo;
                public string FeatureOrigin;

                public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
            }

            private CM_ScrollWindow _backgroundPanel;
            private TMPro.TextMeshPro _headerText;
            private TMPro.TextMeshPro _contentText;
            private TMPro.TextMeshPro _infoL1Text;
            private TMPro.TextMeshPro _infoL2Text;


            public DescriptionPanel()
            {
                _backgroundPanel = CreateScrollWindow("Description");
                _backgroundPanel.transform.localPosition = _backgroundPanel.transform.localPosition + new Vector3(1050, 0, 0);

                CreateItem("Header Text", ORANGE, _backgroundPanel.transform, out var headerSWC, out var rectTransHeader, out _headerText);
                rectTransHeader.sizeDelta = new Vector2(rectTransHeader.sizeDelta.x * 2, rectTransHeader.sizeDelta.y);

                CreateItem("Content Text", WHITE_GRAY, _backgroundPanel.transform, out var contentSWC, out var rectTransContent, out _contentText);
                rectTransContent.sizeDelta = new Vector2(rectTransContent.sizeDelta.x * 2, rectTransContent.sizeDelta.y * 10);

                CreateItem("Info One", RED, _backgroundPanel.transform, out var infoOneSWC, out var rectTransInfo, out _infoL1Text);
                rectTransInfo.sizeDelta = new Vector2(rectTransInfo.sizeDelta.x * 2, rectTransInfo.sizeDelta.y);
                rectTransInfo.localPosition = rectTransInfo.localPosition + new Vector3(0, -660, 0);

                CreateItem("Info Two", ORANGE, _backgroundPanel.transform, out var infoTwoSWC, out var rectTransInfoTwo, out _infoL2Text);
                rectTransInfoTwo.sizeDelta = new Vector2(rectTransInfoTwo.sizeDelta.x * 2, rectTransInfoTwo.sizeDelta.y);
                rectTransInfoTwo.localPosition = rectTransInfoTwo.localPosition + new Vector3(0, -640, 0);

                _backgroundPanel.SetContentItems(new List<iScrollWindowContent>()
                    {
                        headerSWC,
                        contentSWC,
                        infoOneSWC,
                        infoTwoSWC,
                    }.ToIL2CPPListIfNecessary(), 5);

                AddToAllSettingsWindows(_backgroundPanel);
            }

            private static void CreateItem(string text, Color col, Transform parent, out iScrollWindowContent scrollWindowContent, out RectTransform rectTrans, out TMPro.TextMeshPro tmp)
            {
                var settingsItemGO = GameObject.Instantiate(SettingsItemPrefab, parent);

                tmp = settingsItemGO.GetComponentInChildren<CM_SettingsItem>().transform.GetChildWithExactName("Title").GetChildWithExactName("TitleText").gameObject.GetComponent<TMPro.TextMeshPro>();

                tmp.color = col;

                tmp.SetText(text);

                rectTrans = tmp.gameObject.GetComponent<RectTransform>();

                scrollWindowContent = settingsItemGO.GetComponentInChildren<iScrollWindowContent>();
            }

            public void Dispose()
            {
                RemoveFromAllSettingsWindows(_backgroundPanel);
                _backgroundPanel.SafeDestroyGO();
            }

            public void Show(DescriptionPanelData data)
            {
                if (TheColorPicker.IsActive)
                    return;

                _backgroundPanel.gameObject.SetActive(true);

                _headerText.SetText(data.Title);
                JankTextMeshProUpdaterOnce.ForceUpdateMesh(_headerText);

                _contentText.SetText(data.Description);
                JankTextMeshProUpdaterOnce.ForceUpdateMesh(_contentText);

                _infoL1Text.SetText(data.CriticalInfo ?? string.Empty);
                JankTextMeshProUpdaterOnce.ForceUpdateMesh(_infoL1Text);

                _infoL2Text.SetText(data.FeatureOrigin ?? string.Empty);
                JankTextMeshProUpdaterOnce.ForceUpdateMesh(_infoL2Text);
            }

            public void Hide()
            {
                _backgroundPanel.gameObject.SetActive(false);
            }

        }

        public static class PageSettingsData
        {
            internal static GameObject SettingsItemPrefab { get; set; }
            internal static CM_ScrollWindow MainModSettingsScrollWindow { get; set; }
            internal static List<iScrollWindowContent> ScrollWindowContentElements { get; set; } = new List<iScrollWindowContent>();
            internal static List<CM_ScrollWindow> AllSubmenuScrollWindows { get; set; } = new List<CM_ScrollWindow>();
            internal static Transform MainScrollWindowTransform { get; set; }
            internal static CM_ScrollWindow PopupWindow { get; set; }
            internal static CM_PageSettings SettingsPageInstance { get; set; }
            internal static DescriptionPanel TheDescriptionPanel { get; set; }
            internal static ColorPicker TheColorPicker { get; set; }
            internal static SearchMainPage TheSearchMenu { get; set; }

            internal static CM_Item MainModSettingsButton { get; set; }
            internal static GameObject SubMenuButtonPrefab { get; set; }

            public static HashSet<CM_SettingsInputField> TheStaticSettingsInputFieldJankRemoverHashSet2000 { get; private set; } = new HashSet<CM_SettingsInputField>();

            public static MainMenuGuiLayer MMGuiLayer { get; internal set; }
            public static GameObject ScrollWindowPrefab { get; internal set; }
            public static RectTransform MovingContentHolder { get; internal set; }
        }

        public static class UIHelper
        {
            internal static void Setup()
            {
                var settingsItemGameObject = GameObject.Instantiate(ModSettings.PageSettingsData.SettingsItemPrefab);

                var settingsItem = settingsItemGameObject.GetComponentInChildren<CM_SettingsItem>();

                var enumDropdown = GOUtil.SpawnChildAndGetComp<CM_SettingsEnumDropdownButton>(settingsItem.m_enumDropdownInputPrefab, settingsItemGameObject.transform);

                PopupItemPrefab = GameObject.Instantiate(enumDropdown.m_popupItemPrefab);
                GameObject.DontDestroyOnLoad(PopupItemPrefab);
                PopupItemPrefab.hideFlags = HideFlags.HideAndDontSave;
                PopupItemPrefab.transform.position = new Vector3(-3000, -3000, 0);

                GameObject.Destroy(settingsItemGameObject);
            }

            public static GameObject PopupItemPrefab { get; private set; } //iScrollWindowContent
        }

    }
}
