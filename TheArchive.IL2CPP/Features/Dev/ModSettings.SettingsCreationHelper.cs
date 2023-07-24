using CellMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Core.Attributes.Feature.Settings.FSSlider;
using static TheArchive.Features.Dev.ModSettings.PageSettingsData;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev
{
    public partial class ModSettings
    {
        public static class SettingsCreationHelper
        {
            public static void SetupItemsForSettingsHelper(FeatureSettingsHelper settingsHelper, SubMenu placeIntoMenu = null)
            {
                foreach (var setting in settingsHelper.Settings)
                {
                    if (setting.HeaderAbove != null)
                        CreateHeader(setting.HeaderAbove.Title, setting.HeaderAbove.Color.ToUnityColor(), setting.HeaderAbove.Bold, placeIntoMenu);
                    else if (setting.SeparatorAbove)
                        CreateSeparator(subMenu: placeIntoMenu);
                    else if (setting.SpacerAbove)
                        CreateSpacer(subMenu: placeIntoMenu);

                    if (setting.HideInModSettings && !Feature.DevMode)
                    {
                        continue;
                    }

                    switch (setting)
                    {
                        case EnumListSetting els:
                            CreateEnumListSetting(els, placeIntoMenu);
                            break;
                        case ColorSetting cs:
                            CreateColorSetting(cs, placeIntoMenu);
                            break;
                        case StringSetting ss:
                            CreateStringSetting(ss, placeIntoMenu);
                            break;
                        case BoolSetting bs:
                            CreateBoolSetting(bs, placeIntoMenu);
                            break;
                        case EnumSetting es:
                            CreateEnumSetting(es, placeIntoMenu);
                            break;
                        case GenericListSetting gls:
                            CreateGenericListSetting(gls, placeIntoMenu);
                            break;
                        case GenericDictionarySetting gds:
                            CreateGenericDictionarySetting(gds, placeIntoMenu);
                            break;
                        case NumberSetting ns:
                            CreateNumberSetting(ns, placeIntoMenu);
                            break;
                        case ButtonSetting bs:
                            CreateButton(bs, placeIntoMenu);
                            break;
                        case SubmenuSetting ss:
                            var subMenu = new SubMenu(ss.DisplayName);
                            var data = new DescriptionPanel.DescriptionPanelData() {
                                Title = ss.DisplayName,
                                Description = ss.Description,
                            };
                            CreateSubMenuControls(subMenu, menuEntryLabelText: ss.DisplayName, placeIntoMenu: placeIntoMenu, descriptionPanelData: data);
                            SetupItemsForSettingsHelper(ss.SettingsHelper, subMenu);
                            subMenu.Build();
                            break;
                        default:
                            CreateHeader(setting.DEBUG_Path, subMenu: placeIntoMenu);
                            break;
                    }
                }
            }

            public static CM_ScrollWindow CreateScrollWindow(string title)
            {
                CM_ScrollWindow scrollWindow = MMGuiLayer.AddRectComp(ScrollWindowPrefab, GuiAnchor.TopLeft, new Vector2(420f, -200f), MovingContentHolder).TryCastTo<CM_ScrollWindow>();

                scrollWindow.SetupCMItem();
                scrollWindow.SetSize(new Vector2(1020f, 900f));
                scrollWindow.SetVisible(visible: false);
                scrollWindow.SetHeader(title);

                return scrollWindow;
            }

            public static void CreateSimpleTextField(string labelText, string initialValue, Action<string> onValueUpdated, Func<string, string> getValue = null, int maxLength = 32, SubMenu placeIntoMenu = null, DescriptionPanel.DescriptionPanelData descriptionPanelData = null)
            {
                CreateSettingsItem(labelText, out var cm_settingsItem, subMenu: placeIntoMenu);

                CM_SettingsInputField cm_settingsInputField = GOUtil.SpawnChildAndGetComp<CM_SettingsInputField>(cm_settingsItem.m_textInputPrefab, cm_settingsItem.m_inputAlign);

                StringInputSetMaxLength(cm_settingsInputField, maxLength);

                CreateFSHoverAndSetButtonAction(descriptionPanelData, cm_settingsItem, null);

                cm_settingsInputField.m_text.SetText(initialValue ?? string.Empty);

                JankTextMeshProUpdaterOnce.Apply(cm_settingsInputField.m_text);

                var receiver = new CustomStringReceiver(new Func<string>(
                    () => {
                        var currentText = cm_settingsInputField.m_text.text;
                        var val = getValue?.Invoke(currentText);
                        return val ?? currentText;
                    }),
                    (val) => {
                        onValueUpdated?.Invoke(val);
                    });

#if IL2CPP
                cm_settingsInputField.m_stringReceiver = new iStringInputReceiver(receiver.Pointer);
#else
                A_CM_SettingsInputField_m_stringReceiver.Set(cm_settingsInputField, receiver);
#endif

                cm_settingsItem.ForcePopupLayer(true);
            }

            public static void CreateSimpleToggle(string labelText, bool initialState, Action<bool> onPress, SubMenu placeIntoMenu = null, string stateTrue = "<#0F0>[ On ]</color>", string stateFalse = "<#F00>[ Off ]</color>")
            {
                CreateSettingsItem(labelText, out var cmItem, subMenu: placeIntoMenu);
                cmItem.ForcePopupLayer(true);
                SetupToggleButton(cmItem, out var buttonCMItem, out var buttonTmp);
                buttonTmp.SetText(initialState ? stateTrue : stateFalse);
                JankTextMeshProUpdaterOnce.Apply(buttonTmp);
                buttonCMItem.SetCMItemEvents((_) => {
                    var stateString = buttonTmp.text;

                    var state = stateString == stateTrue;

                    state = !state;

                    buttonTmp.SetText(state ? stateTrue : stateFalse);
                    JankTextMeshProUpdaterOnce.Apply(buttonTmp);

                    onPress?.Invoke(state);
                });
            }

            public static void CreateSimpleButton(string labelText, string buttonText, Action onPress, out CM_SettingsItem cmItem, SubMenu placeIntoMenu = null)
            {
                CreateSettingsItem(labelText, out cmItem, subMenu: placeIntoMenu);
                cmItem.ForcePopupLayer(true);
                SetupToggleButton(cmItem, out var buttonCMItem, out var buttonTmp);
                buttonTmp.SetText($"[ {buttonText} ]");
                JankTextMeshProUpdaterOnce.Apply(buttonTmp);
                buttonCMItem.SetCMItemEvents((_) => {
                    onPress?.Invoke();
                });
            }

            public static void CreateSimpleButton(string labelText, string buttonText, Action onPress, SubMenu placeIntoMenu = null)
            {
                CreateSimpleButton(labelText, buttonText, onPress, out _, placeIntoMenu);
            }

            public static void CreateSpacer(SubMenu subMenu = null)
            {
                CreateHeader(string.Empty, subMenu: subMenu);
            }

            public static void CreateSeparator(Color? col = null, SubMenu subMenu = null)
            {
                if (!col.HasValue)
                    col = DISABLED;
                CreateHeader("------------------------------", col, false, subMenu);
            }

            public static void CreateHeader(string title, Color? color = null, bool bold = true, SubMenu subMenu = null, Action<int> clickAction = null, Action<int, bool> hoverAction = null)
            {
                if (!color.HasValue)
                    color = ORANGE;

                if (string.IsNullOrWhiteSpace(title))
                {
                    title = string.Empty;
                }
                else
                {
                    if (bold)
                        title = $"<b>{title}</b>";
                }

                CreateSettingsItem(title, out var cm_settingsItem, color.Value, subMenu);

                var rectTrans = cm_settingsItem.transform.GetChildWithExactName("Title").GetChildWithExactName("TitleText").gameObject.GetComponent<RectTransform>();

                rectTrans.sizeDelta = new Vector2(rectTrans.sizeDelta.x * 2, rectTrans.sizeDelta.y);

                if (!string.IsNullOrWhiteSpace(title))
                {
                    cm_settingsItem.ForcePopupLayer(true);
                }

                if (clickAction != null || hoverAction != null)
                {
                    cm_settingsItem.SetCMItemEvents(clickAction, hoverAction);
                }
            }

            public static void CreateSettingsItem(string titleText, out CM_SettingsItem cm_settingsItem, Color? titleColor = null, SubMenu subMenu = null)
            {
                var settingsItemGameObject = GameObject.Instantiate(SettingsItemPrefab, subMenu?.WindowTransform ?? MainScrollWindowTransform);

                if (subMenu != null)
                {
                    subMenu.AppendContent(settingsItemGameObject);
                }
                else
                {
                    ScrollWindowContentElements.Add(settingsItemGameObject.GetComponentInChildren<iScrollWindowContent>());
                }

                cm_settingsItem = settingsItemGameObject.GetComponentInChildren<CM_SettingsItem>();

                var titleTextTMP = cm_settingsItem.transform.GetChildWithExactName("Title").GetChildWithExactName("TitleText").gameObject.GetComponent<TMPro.TextMeshPro>();

                if (titleColor.HasValue)
                    titleTextTMP.color = titleColor.Value;

#if IL2CPP
                titleTextTMP.m_text = titleText;
                titleTextTMP.SetText(titleText);
#else
                titleTextTMP.SetText(titleText);
#endif

                JankTextMeshProUpdaterOnce.Apply(titleTextTMP);
            }

            public static void SetupToggleButton(CM_SettingsItem cm_settingsItem, out CM_Item toggleButton_cm_item, out TMPro.TextMeshPro toggleButtonText)
            {
                CM_SettingsToggleButton cm_SettingsToggleButton = GOUtil.SpawnChildAndGetComp<CM_SettingsToggleButton>(cm_settingsItem.m_toggleInputPrefab, cm_settingsItem.m_inputAlign);

                toggleButton_cm_item = cm_SettingsToggleButton.gameObject.AddComponent<CM_Item>();

                Component.Destroy(cm_SettingsToggleButton);

                toggleButtonText = toggleButton_cm_item.GetComponentInChildren<TMPro.TextMeshPro>();

                var collider = toggleButton_cm_item.GetComponent<BoxCollider2D>();
                collider.size = new Vector2(550, 50);
                collider.offset = new Vector2(250, -25);
            }

            public static void CreateSubMenuControls(SubMenu subMenu, Color? entryItemColor = null, string menuEntryLabelText = "> Settings", SubMenu placeIntoMenu = null, string headerText = null, DescriptionPanel.DescriptionPanelData descriptionPanelData = null, string backButtonText = "<<< Back <<<", string enterButtonText = "> ENTER <")
            {
                if (subMenu == null) return;

                using var _ = subMenu.GetPersistentContenAdditionToken();

                CreateSettingsItem(backButtonText, out var outof_sub_cm_settingsItem, RED, subMenu);
                CreateSpacer(subMenu);

                if (!string.IsNullOrWhiteSpace(headerText))
                {
                    CreateHeader(headerText, subMenu: subMenu);
                }

                CreateSettingsItem(menuEntryLabelText, out var into_sub_cm_settingsItem, entryItemColor, placeIntoMenu);

                CreateFSHoverAndSetButtonAction(descriptionPanelData, into_sub_cm_settingsItem, null);

                #region back-button
                outof_sub_cm_settingsItem.ForcePopupLayer(true);

                outof_sub_cm_settingsItem.SetCMItemEvents((_) => subMenu.Close());
                #endregion back-button

                SetupToggleButton(into_sub_cm_settingsItem, out var into_sub_toggleButton_cm_item, out var into_sub_toggleButtonText);

                SharedUtils.ChangeColorCMItem(into_sub_toggleButton_cm_item, ORANGE);

                into_sub_toggleButton_cm_item.SetCMItemEvents(delegate (int id) {
                    subMenu.Show();
                });

                into_sub_cm_settingsItem.ForcePopupLayer(true);

                into_sub_toggleButtonText.SetText(enterButtonText);
                JankTextMeshProUpdaterOnce.Apply(into_sub_toggleButtonText);
            }

            public static void CreateColorSetting(ColorSetting setting, SubMenu subMenu = null)
            {
                //setting.Helper.Feature.TryRetrieve<SubMenu>("SubMenu", out var subMenu);

                CreateSettingsItem(GetNameForSetting(setting), out var cm_settingsItem, subMenu: subMenu);

                CreateRundownInfoTextForItem(cm_settingsItem, setting.RundownHint);

                CM_SettingsInputField cm_settingsInputField = GOUtil.SpawnChildAndGetComp<CM_SettingsInputField>(cm_settingsItem.m_textInputPrefab, cm_settingsItem.m_inputAlign);

                StringInputSetMaxLength(cm_settingsInputField, 7);

                CreateFSHoverAndSetButtonAction(setting, cm_settingsItem, null);

                var bg_rt = cm_settingsInputField.m_background.GetComponent<RectTransform>();
                bg_rt.anchoredPosition = new Vector2(-175, 0);
                bg_rt.localScale = new Vector3(0.19f, 1, 1);

                if (setting.Readonly)
                {
                    cm_settingsInputField.GetComponent<BoxCollider2D>().enabled = false;
                }

                var colorPreviewGO = GameObject.Instantiate(cm_settingsInputField.m_background.gameObject, cm_settingsInputField.transform, true);
                colorPreviewGO.transform.SetParent(cm_settingsItem.m_inputAlign);
                colorPreviewGO.transform.localScale = new Vector3(0.07f, 1, 1);
                colorPreviewGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(-225, -25);
                var renderer = colorPreviewGO.GetComponent<SpriteRenderer>();

                var scol = (SColor)setting.GetValue();

                renderer.color = scol.ToUnityColor();

                cm_settingsInputField.m_text.SetText(scol.ToHexString());

                JankTextMeshProUpdaterOnce.Apply(cm_settingsInputField.m_text);

                var receiver = new CustomStringReceiver(new Func<string>(
                    () => {
                        FeatureLogger.Debug($"[{nameof(CustomStringReceiver)}({nameof(ColorSetting)})] Gotten value of \"{setting.DEBUG_Path}\"!");
                        SColor color = (SColor)setting.GetValue();
                        renderer.color = color.ToUnityColor();
                        return color.ToHexString();
                    }),
                    (val) => {
                        FeatureLogger.Debug($"[{nameof(CustomStringReceiver)}({nameof(ColorSetting)})] Set value of \"{setting.DEBUG_Path}\" to \"{val}\"");
                        SColor color = SColorExtensions.FromHexString(val);
                        setting.SetValue(color);
                    });

#if IL2CPP
                cm_settingsInputField.m_stringReceiver = new iStringInputReceiver(receiver.Pointer);
#else
                A_CM_SettingsInputField_m_stringReceiver.Set(cm_settingsInputField, receiver);
#endif


                cm_settingsItem.ForcePopupLayer(true);
            }

            public static void CreateStringSetting(StringSetting setting, SubMenu subMenu = null)
            {
                CreateSettingsItem(GetNameForSetting(setting), out var cm_settingsItem, subMenu: subMenu);

                CreateRundownInfoTextForItem(cm_settingsItem, setting.RundownHint);

                CM_SettingsInputField cm_settingsInputField = GOUtil.SpawnChildAndGetComp<CM_SettingsInputField>(cm_settingsItem.m_textInputPrefab, cm_settingsItem.m_inputAlign);

                StringInputSetMaxLength(cm_settingsInputField, setting.MaxInputLength);

                CreateFSHoverAndSetButtonAction(setting, cm_settingsItem, null);

                cm_settingsInputField.m_text.SetText(setting.GetValue()?.ToString() ?? string.Empty);

                if (setting.Readonly)
                {
                    cm_settingsInputField.GetComponent<BoxCollider2D>().enabled = false;
                    cm_settingsInputField.m_background.gameObject.SetActive(false);
                }

                JankTextMeshProUpdaterOnce.Apply(cm_settingsInputField.m_text);

                var receiver = new CustomStringReceiver(new Func<string>(
                    () => {
                        FeatureLogger.Debug($"[{nameof(CustomStringReceiver)}] Gotten value of \"{setting.DEBUG_Path}\"!");
                        return setting.GetValue()?.ToString() ?? string.Empty;
                    }),
                    (val) => {
                        FeatureLogger.Debug($"[{nameof(CustomStringReceiver)}] Set value of \"{setting.DEBUG_Path}\" to \"{val}\"");
                        setting.SetValue(val);
                    });

#if IL2CPP
                cm_settingsInputField.m_stringReceiver = new iStringInputReceiver(receiver.Pointer);
#else
                A_CM_SettingsInputField_m_stringReceiver.Set(cm_settingsInputField, receiver);
#endif

                cm_settingsItem.ForcePopupLayer(true);
            }

            public static void StringInputSetMaxLength(CM_SettingsInputField sif, int maxLength)
            {
#if MONO
                A_CM_SettingsInputField_m_maxLen.Set(sif, maxLength);
#else
                sif.m_maxLen = maxLength;
#endif
            }

            public static void CreateBoolSetting(BoolSetting setting, SubMenu subMenu = null)
            {
                CreateSettingsItem(GetNameForSetting(setting), out var cm_settingsItem, subMenu: subMenu);

                CreateRundownInfoTextForItem(cm_settingsItem, setting.RundownHint);

                CM_SettingsToggleButton cm_SettingsToggleButton = GOUtil.SpawnChildAndGetComp<CM_SettingsToggleButton>(cm_settingsItem.m_toggleInputPrefab, cm_settingsItem.m_inputAlign);

                var toggleButton_cm_item = cm_SettingsToggleButton.gameObject.AddComponent<CM_Item>();

                if (setting.Readonly)
                {
                    toggleButton_cm_item.GetComponent<BoxCollider2D>().enabled = false;
                }

                Component.Destroy(cm_SettingsToggleButton);

                var toggleButtonText = toggleButton_cm_item.GetComponentInChildren<TMPro.TextMeshPro>();

                CreateFSHoverAndSetButtonAction(setting, cm_settingsItem, toggleButton_cm_item, delegate (int id) {
                    var val = !(bool)setting.GetValue();
                    setting.SetValue(val);

                    toggleButtonText.SetText(val ? "On" : "Off");
                    var color = val ? GREEN : RED;
                    SharedUtils.ChangeColorCMItem(toggleButton_cm_item, color);
                });

                var currentValue = (bool)setting.GetValue();
                toggleButtonText.SetText(currentValue ? "On" : "Off");
                var col = currentValue ? GREEN : RED;
                SharedUtils.ChangeColorCMItem(toggleButton_cm_item, col);

                cm_settingsItem.ForcePopupLayer(true);
            }

            public static void CreateEnumListSetting(EnumListSetting setting, SubMenu subMenu = null)
            {
                CreateSettingsItem(GetNameForSetting(setting), out var cm_settingsItem, subMenu: subMenu);

                CreateRundownInfoTextForItem(cm_settingsItem, setting.RundownHint);

                CM_SettingsEnumDropdownButton cm_settingsEnumDropdownButton = GOUtil.SpawnChildAndGetComp<CM_SettingsEnumDropdownButton>(cm_settingsItem.m_enumDropdownInputPrefab, cm_settingsItem.m_inputAlign);

                var enumButton_cm_item = cm_settingsEnumDropdownButton.gameObject.AddComponent<CM_Item>();

                enumButton_cm_item.Setup();

                CreateFSHoverAndSetButtonAction(setting, cm_settingsItem, enumButton_cm_item, delegate (int _) {
                    CreateAndShowEnumListPopup(setting, enumButton_cm_item, cm_settingsEnumDropdownButton);
                });

                if (setting.Readonly)
                {
                    enumButton_cm_item.GetComponent<BoxCollider2D>().enabled = false;
                }

                SharedUtils.ChangeColorCMItem(enumButton_cm_item, ORANGE);
                var bg = enumButton_cm_item.gameObject.transform.GetChildWithExactName("Background");
                if (bg != null)
                {
                    UnityEngine.Object.Destroy(bg.gameObject);
                }
                enumButton_cm_item.SetText(GetEnumListItemName(setting));

                var collider = enumButton_cm_item.GetComponent<BoxCollider2D>();
                collider.size = new Vector2(550, 50);
                collider.offset = new Vector2(250, -25);

                cm_settingsItem.ForcePopupLayer(true);

                cm_settingsEnumDropdownButton.enabled = false;
                UnityEngine.Object.Destroy(cm_settingsEnumDropdownButton);

#if MONO
                A_CM_SettingsEnumDropdownButton_m_popupWindow.Set(cm_settingsEnumDropdownButton, PopupWindow);
#else
                cm_settingsEnumDropdownButton.m_popupWindow = PopupWindow;
#endif

            }

            public static void CreateAndShowEnumListPopup(EnumListSetting setting, CM_Item enumButton_cm_item, CM_SettingsEnumDropdownButton cm_settingsEnumDropdownButton)
            {
                var list = SharedUtils.NewListForGame<iScrollWindowContent>();

                var currentValues = setting.CurrentSelectedValues();

                var allCMItems = new List<CM_Item>();

                foreach (var kvp in setting.Map)
                {
                    iScrollWindowContent iScrollWindowContent = GOUtil.SpawnChildAndGetComp<iScrollWindowContent>(cm_settingsEnumDropdownButton.m_popupItemPrefab, enumButton_cm_item.transform);
                    list.Add(iScrollWindowContent);
                    string enumKey = kvp.Key;
                    object enumValue = kvp.Value;

                    CM_Item cm_Item = iScrollWindowContent.TryCastTo<CM_Item>();

                    if (cm_Item != null)
                    {
                        cm_Item.Setup();
                        cm_Item.SetText(enumKey);
                        //cm_Item.SetAnchor(GuiAnchor.TopLeft, true);
                        cm_Item.SetScaleFactor(1f);

                        cm_Item.name = enumKey;

                        SharedUtils.ChangeColorCMItem(cm_Item, currentValues.Contains(enumValue) ? ORANGE : DISABLED);

                        cm_Item.ForcePopupLayer(true);

                        allCMItems.Add(cm_Item);
                    }
                }

                foreach (var cm_Item in allCMItems)
                {
                    cm_Item.SetCMItemEvents((_) =>
                    {
                        var enumKey = cm_Item.name;
                        var value = setting.GetEnumValueFor(enumKey);
                        var enabled = setting.ToggleInList(value);

                        enumButton_cm_item.SetText(GetEnumListItemName(setting));
                        SharedUtils.ChangeColorCMItem(cm_Item, enabled ? ORANGE : DISABLED);
                    });
                }

                PopupWindow.SetupFromButton(cm_settingsEnumDropdownButton.TryCastTo<iCellMenuPopupController>(), SettingsPageInstance);

                PopupWindow.transform.position = cm_settingsEnumDropdownButton.m_popupWindowAlign.position;
                PopupWindow.SetContentItems(list, 5f);
                PopupWindow.SetHeader(setting.DisplayName);
                PopupWindow.SetVisible(true);
            }

            public static string GetEnumListItemName(EnumListSetting setting)
            {
                var str = string.Join(", ", setting.CurrentSelectedValues());

                if (string.IsNullOrWhiteSpace(str))
                {
                    return "[None]";
                }

                if (str.Length > 36)
                {
                    return str.Substring(0, 36) + " ...";
                }

                return str;
            }

            public static void CreateEnumSetting(EnumSetting setting, SubMenu subMenu = null)
            {
                CreateSettingsItem(GetNameForSetting(setting), out var cm_settingsItem, subMenu: subMenu);

                CreateRundownInfoTextForItem(cm_settingsItem, setting.RundownHint);

                CM_SettingsEnumDropdownButton cm_settingsEnumDropdownButton = GOUtil.SpawnChildAndGetComp<CM_SettingsEnumDropdownButton>(cm_settingsItem.m_enumDropdownInputPrefab, cm_settingsItem.m_inputAlign);

                var enumButton_cm_item = cm_settingsEnumDropdownButton.gameObject.AddComponent<CM_Item>();

                enumButton_cm_item.Setup();

                CreateFSHoverAndSetButtonAction(setting, cm_settingsItem, enumButton_cm_item, delegate (int _)
                {
                    CreateAndShowEnumPopup(setting, enumButton_cm_item, cm_settingsEnumDropdownButton);
                });

                if (setting.Readonly)
                {
                    enumButton_cm_item.GetComponent<BoxCollider2D>().enabled = false;
                }

                SharedUtils.ChangeColorCMItem(enumButton_cm_item, ORANGE);
                var bg = enumButton_cm_item.gameObject.transform.GetChildWithExactName("Background");
                if (bg != null)
                {
                    UnityEngine.Object.Destroy(bg.gameObject);
                }
                enumButton_cm_item.SetText(setting.GetCurrentEnumKey());

                var collider = enumButton_cm_item.GetComponent<BoxCollider2D>();
                collider.size = new Vector2(550, 50);
                collider.offset = new Vector2(250, -25);

                cm_settingsItem.ForcePopupLayer(true);

                cm_settingsEnumDropdownButton.enabled = false;
                UnityEngine.Object.Destroy(cm_settingsEnumDropdownButton);

#if MONO
                A_CM_SettingsEnumDropdownButton_m_popupWindow.Set(cm_settingsEnumDropdownButton, PopupWindow);
#else
                cm_settingsEnumDropdownButton.m_popupWindow = PopupWindow;
#endif

            }

            public static void CreateAndShowEnumPopup(EnumSetting setting, CM_Item enumButton_cm_item, CM_SettingsEnumDropdownButton cm_settingsEnumDropdownButton)
            {
                var list = SharedUtils.NewListForGame<iScrollWindowContent>();

                var currentKey = setting.GetCurrentEnumKey();

                foreach (var kvp in setting.Map)
                {
                    iScrollWindowContent iScrollWindowContent = GOUtil.SpawnChildAndGetComp<iScrollWindowContent>(cm_settingsEnumDropdownButton.m_popupItemPrefab, enumButton_cm_item.transform);
                    list.Add(iScrollWindowContent);
                    string enumKey = kvp.Key;

                    CM_Item cm_Item = iScrollWindowContent.TryCastTo<CM_Item>();

                    if (cm_Item != null)
                    {
                        cm_Item.Setup();
                        cm_Item.SetText(enumKey);

                        cm_Item.SetScaleFactor(1f);

                        SharedUtils.ChangeColorCMItem(cm_Item, currentKey == enumKey ? ORANGE : DISABLED);

                        cm_Item.ForcePopupLayer(true);

                        cm_Item.SetCMItemEvents((_) => {
                            setting.SetValue(kvp.Value);

                            enumButton_cm_item.SetText(enumKey);

                            SetPopupVisible(false);
                        });
                    }
                }

                PopupWindow.SetupFromButton(cm_settingsEnumDropdownButton.CastTo<iCellMenuPopupController>(), SettingsPageInstance);

                PopupWindow.transform.position = cm_settingsEnumDropdownButton.m_popupWindowAlign.position;
                PopupWindow.SetContentItems(list, 5f);
                PopupWindow.SetHeader(setting.DisplayName);
                PopupWindow.SetVisible(true);
            }

            public static void CreateGenericListSetting(GenericListSetting setting, SubMenu subMenu = null)
            {
                var dynamicSubMenu = new DynamicSubMenu(setting.DisplayName, (dynamicMenu) => {
                    foreach (var entry in setting.GetEntries())
                    {
                        SetupItemsForSettingsHelper(entry.Helper, dynamicMenu);
                    }
                });

                CreateSubMenuControls(dynamicSubMenu, menuEntryLabelText: setting.DisplayName, headerText: setting.DisplayName, placeIntoMenu: subMenu);
            }

            public static void CreateGenericDictionarySetting(GenericDictionarySetting setting, SubMenu subMenu = null)
            {
                var dynamicSubMenu = new DynamicSubMenu(setting.DisplayName, (dynamicMenu) => {
                    foreach (var entry in setting.GetEntries())
                    {
                        if (setting.TopLevelReadonly || setting.Readonly)
                        {
                            CreateHeader(entry.Key.ToString(), subMenu: dynamicMenu);
                        }
                        else
                        {
#warning TODO: Editable keys in dict setting?
                            CreateHeader("EDITABLE=TODO: " + entry.Key.ToString(), subMenu: dynamicMenu);
                        }
                        SetupItemsForSettingsHelper(entry.Helper, dynamicMenu);
                    }
                });

                CreateSubMenuControls(dynamicSubMenu, menuEntryLabelText: setting.DisplayName, headerText: setting.DisplayName, placeIntoMenu: subMenu);
            }

            #region NumberSetting_FloatValueDisplay
            private static class OnlyTouchOnR5AndLater
            {
#if IL2CPP
                internal static Dictionary<SliderStyle, eDisplayFloatValueAs> _styleDisplayMap = new Dictionary<SliderStyle, eDisplayFloatValueAs>()
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    { SliderStyle.IntMinMax, GetEnumFromName<eDisplayFloatValueAs>(nameof(eDisplayFloatValueAs.Percent)) },
#pragma warning restore CS0618 // Type or member is obsolete
                    { SliderStyle.FloatPercent, GetEnumFromName<eDisplayFloatValueAs>(nameof(eDisplayFloatValueAs.Percent)) },
                    { SliderStyle.FloatNoDecimal, GetEnumFromName<eDisplayFloatValueAs>(nameof(eDisplayFloatValueAs.Decimal0)) },
                    { SliderStyle.FloatOneDecimal, GetEnumFromName<eDisplayFloatValueAs>(nameof(eDisplayFloatValueAs.Decimal1)) },
                    { SliderStyle.FloatTwoDecimal, GetEnumFromName<eDisplayFloatValueAs>(nameof(eDisplayFloatValueAs.Decimal2)) },
                };
#endif
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            /// <summary>
            /// Only touch on R5 and later!!
            /// </summary>
            private static bool GetSliderFloatDisplayValue(SliderStyle style, out int val)
            {
#if IL2CPP
                var ret = OnlyTouchOnR5AndLater._styleDisplayMap.TryGetValue(style, out var value);
                val = (int)value;
                return ret;
#else
                val = 0;
                return false;
#endif
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            /// <summary>
            /// Only touch on R5 and later!!
            /// </summary>
            private static void SetSliderFloatDisplayStyle(CM_SettingScrollReceiver cm_settingScrollReceiver, SliderStyle style)
            {
#if IL2CPP
                if(GetSliderFloatDisplayValue(style, out var value))
                {
                    cm_settingScrollReceiver.m_displayAs = (eDisplayFloatValueAs)value;
                }
#endif
            }
            #endregion NumberSetting_FloatValueDisplay

            private static IValueAccessor<CM_SettingScrollReceiver, iFloatInputReceiver> _A_m_floatReceiver = AccessorBase.GetValueAccessor<CM_SettingScrollReceiver, iFloatInputReceiver>("m_floatReceiver");
            private static IValueAccessor<CM_SettingScrollReceiver, iIntInputReceiver> _A_m_intReceiver = AccessorBase.GetValueAccessor<CM_SettingScrollReceiver, iIntInputReceiver>("m_intReceiver");
            private static IValueAccessor<CM_SettingScrollReceiver, eSettingInputType> _A_m_inputType = AccessorBase.GetValueAccessor<CM_SettingScrollReceiver, eSettingInputType>("m_inputType");
            private static IValueAccessor<CM_SettingScrollReceiver, float> _A_m_scrollRange = AccessorBase.GetValueAccessor<CM_SettingScrollReceiver, float>("m_scrollRange");

            //m_handleLocalXPosMinMax // m_scrollRange
            public static void CreateNumberSetting(NumberSetting setting, SubMenu subMenu)
            {
                CreateSettingsItem(GetNameForSetting(setting), out var cm_settingsItem, subMenu: subMenu);

                CreateRundownInfoTextForItem(cm_settingsItem, setting.RundownHint);

                CM_SettingsInputField cm_settingsInputField = GOUtil.SpawnChildAndGetComp<CM_SettingsInputField>(cm_settingsItem.m_textInputPrefab, cm_settingsItem.m_inputAlign);

                StringInputSetMaxLength(cm_settingsInputField, 30);

                CreateFSHoverAndSetButtonAction(setting, cm_settingsItem, null);

                cm_settingsInputField.m_text.SetText(setting.GetValue()?.ToString() ?? string.Empty);


                CM_SettingScrollReceiver cm_settingScrollReceiver = null;

                if (setting.Readonly)
                {
                    cm_settingsInputField.GetComponent<BoxCollider2D>().enabled = false;
                    cm_settingsInputField.m_background.gameObject.SetActive(false);
                }
                else if (setting.HasSlider)
                {
                    var slider = setting.Slider;

                    cm_settingScrollReceiver = GOUtil.SpawnChildAndGetComp<CM_SettingScrollReceiver>(cm_settingsItem.m_sliderInputPrefab, cm_settingsItem.m_inputAlign);

#pragma warning disable CS0618 // Type or member is obsolete
                    var inputType = slider.Style == SliderStyle.IntMinMax ? eSettingInputType.IntMinMaxSlider : eSettingInputType.FloatSlider;
#pragma warning restore CS0618 // Type or member is obsolete

                    _A_m_inputType.Set(cm_settingScrollReceiver, inputType);
#if IL2CPP
                    if(Is.R5OrLater)
                    {
                        // Proper value display on sliders is R5 and up only for now!
                        SetSliderFloatDisplayStyle(cm_settingScrollReceiver, slider.Style);
                    }   
#endif

                    // TODO: Properly implement int sliders!
                    var intReceiver = new CustomIntReceiver(new Func<int>(() =>
                    {
                        return (int)setting.GetValue();
                    }),
                    (val) => {
                        if(((int)setting.GetValue()) != val)
                        {
                            setting.SetValue(val);
                        }
                    });

                    var floatReceiver = new CustomFloatReceiver(new Func<float>(() =>
                    {
                        var val = (float)setting.GetValue();
                        var delta = (val - slider.Min) / (slider.Max - slider.Min);
                        return delta;
                    }),
                    (delta) => {
                        var val = (slider.Max - slider.Min) * delta + slider.Min;
                        switch (slider.Rounding)
                        {
                            case RoundTo.NoRounding:
                                break;
                            default:
                                val = (float) Math.Round(val, (int)slider.Rounding);
                                break;
                        }
                        val = Math.Min(slider.Max, Math.Max(val, slider.Min));
                        if(((float)setting.GetValue()) != val)
                        {
                            FeatureLogger.Debug($"{setting.DEBUG_Path}: setting to {val} (delta={delta})");
                            setting.SetValue(val);
                        }

                        CM_SettingScrollReceiver_GetFloatDisplayText_Patch.OverrideDisplayValue = true;
                        CM_SettingScrollReceiver_GetFloatDisplayText_Patch.Value = val;
                    });

#if IL2CPP
                    cm_settingScrollReceiver.m_intReceiver = new iIntInputReceiver(intReceiver.Pointer);
                    cm_settingScrollReceiver.m_floatReceiver = new iFloatInputReceiver(floatReceiver.Pointer);
#else
                    _A_m_intReceiver.Set(cm_settingScrollReceiver, intReceiver);
                    _A_m_floatReceiver.Set(cm_settingScrollReceiver, floatReceiver);
#endif

                    _A_m_scrollRange.Set(cm_settingScrollReceiver, cm_settingScrollReceiver.m_handleLocalXPosMinMax.y - cm_settingScrollReceiver.m_handleLocalXPosMinMax.x);

                    CM_SettingScrollReceiver_GetFloatDisplayText_Patch.OverrideDisplayValue = true;

                    if(float.TryParse(setting.GetValue().ToString(), out var fValue))
                        CM_SettingScrollReceiver_GetFloatDisplayText_Patch.Value = fValue;

                    cm_settingScrollReceiver.ResetValue();

                    cm_settingsInputField.gameObject.SetActive(false);
                }

                JankTextMeshProUpdaterOnce.Apply(cm_settingsInputField.m_text);

                var receiver = new CustomStringReceiver(new Func<string>(
                    () => {
                        var val = setting.GetValue()?.ToString() ?? string.Empty;
                        FeatureLogger.Debug($"[{nameof(CustomStringReceiver)}] Gotten value of \"{setting.DEBUG_Path}\"! ({val})");
                        return val;
                    }),
                    (val) => {
                        FeatureLogger.Debug($"[{nameof(CustomStringReceiver)}] Attempting to set value of \"{setting.DEBUG_Path}\" to \"{val}\"");
                        setting.SetValue(val);
                        FeatureLogger.Debug($"[{nameof(CustomStringReceiver)}] Set value of \"{setting.DEBUG_Path}\" to \"{setting.GetValue()}\"");
                    });

#if IL2CPP
                cm_settingsInputField.m_stringReceiver = new iStringInputReceiver(receiver.Pointer);
#else
                A_CM_SettingsInputField_m_stringReceiver.Set(cm_settingsInputField, receiver);
#endif

                cm_settingsItem.ForcePopupLayer(true);
            }

            public static void CreateButton(ButtonSetting setting, SubMenu subMenu)
            {
                CreateSimpleButton(GetNameForSetting(setting), setting.ButtonText, () => {
                    FeatureManager.InvokeButtonPressed(setting.Helper.Feature, setting);
                }, out var cm_settingsItem, subMenu);

                CreateRundownInfoTextForItem(cm_settingsItem, setting.RundownHint);

                CreateFSHoverAndSetButtonAction(setting, cm_settingsItem, null);
            }

            public static void CreateRundownInfoTextForItem(CM_SettingsItem cm_settingsItem, RundownFlags rundowns)
            {
                var rundownInfoButton = GOUtil.SpawnChildAndGetComp<CM_SettingsToggleButton>(cm_settingsItem.m_toggleInputPrefab, cm_settingsItem.m_inputAlign);
                var rundownInfoTMP = rundownInfoButton.GetComponentInChildren<TMPro.TextMeshPro>();
                JankTextMeshProUpdaterOnce.Apply(rundownInfoTMP);
                UnityEngine.Object.Destroy(rundownInfoButton);
                UnityEngine.Object.Destroy(rundownInfoButton.GetComponent<BoxCollider2D>());


                rundownInfoTMP.GetComponent<RectTransform>().sizeDelta = new Vector2(520, 50);

                if (rundowns == RundownFlags.None)
                {
                    rundownInfoTMP.SetText(string.Empty);
                }
                else
                {
                    Enum.TryParse<RundownID>(rundowns.LowestRundownFlag().ToString(), out var lowestId);
                    Enum.TryParse<RundownID>(rundowns.HighestRundownFlag().ToString(), out var highestId);


                    if (lowestId == highestId)
                    {
                        var R = "R";
                        if (lowestId >= RundownID.RundownAltOne)
                        {
                            R = "A";
                            lowestId = lowestId - (int)RundownID.RundownAltOne + 1;
                        }
                        rundownInfoTMP.SetText($"<align=right>{R}{(int)lowestId}</align>");
                    }
                    else
                    {
                        var RL = "R";
                        if (lowestId >= RundownID.RundownAltOne)
                        {
                            RL = "A";
                            lowestId = lowestId - (int)RundownID.RundownAltOne + 1;
                        }
                        var RH = "R";
                        if (highestId >= RundownID.RundownAltOne)
                        {
                            RH = "A";
                            highestId = highestId - (int)RundownID.RundownAltOne + 1;
                        }
                        rundownInfoTMP.SetText($"<align=right>{RL}{(int)lowestId}-{RH}{(int)highestId}</align>");
                    }
                }

                rundownInfoTMP.color = ORANGE;
            }

            public static void CreateFSHoverAndSetButtonAction(FeatureSetting setting, CM_SettingsItem cm_settingsItem, CM_Item toggleButton_cm_item, Action<int> buttonAction = null)
            {
                var data = new DescriptionPanel.DescriptionPanelData() {
                    Title = setting.DisplayName,
                    Description = setting.Description,
                };

                CreateFSHoverAndSetButtonAction(data, cm_settingsItem, toggleButton_cm_item, buttonAction);
            }

            public static void CreateFSHoverAndSetButtonAction(DescriptionPanel.DescriptionPanelData data, CM_SettingsItem cm_settingsItem, CM_Item toggleButton_cm_item, Action<int> buttonAction = null)
            {
                var delHover = delegate (int id, bool hovering)
                {
                    if (hovering)
                    {
                        if (data != null && data.HasDescription)
                            TheDescriptionPanel.Show(data.Title, data.Description);
                    }
                    else
                    {
                        TheDescriptionPanel.Hide();
                    }
                };

                cm_settingsItem?.SetCMItemEvents((_) => { }, delHover);
                toggleButton_cm_item?.SetCMItemEvents(buttonAction ?? ((_) => { }), delHover);
            }

            public static void SetFeatureItemTextAndColor(Feature feature, CM_Item buttonItem, TMPro.TextMeshPro text)
            {
                if (feature.IsAutomated)
                {
                    text.SetText("Automated");
                    SharedUtils.ChangeColorCMItem(buttonItem, DISABLED);
                    return;
                }
                bool enabled = (feature.AppliesToThisGameBuild && !feature.RequiresRestart) ? feature.Enabled : FeatureManager.IsEnabledInConfig(feature);
                text.SetText(enabled ? "Enabled" : "Disabled");

                SetFeatureItemColor(feature, buttonItem);

                JankTextMeshProUpdaterOnce.UpdateMesh(text);
            }

            public static void SetFeatureItemColor(Feature feature, CM_Item item)
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

            public static void ShowPopupWindow(string header, Vector2 pos)
            {
                PopupWindow.SetHeader(header);
                PopupWindow.transform.position = pos;
                PopupWindow.SetVisible(true);
            }

            public static void SetPopupVisible(bool visible)
            {
                PopupWindow.SetVisible(visible);
            }

            public static string GetNameForSetting(FeatureSetting setting)
            {
                if (setting.HideInModSettings)
                    return $"<{DISABLED.ToSColor().ToHexString()}>[H] {setting.DisplayName}</color>";

                return setting.DisplayName;
            }
        }
    }
}
