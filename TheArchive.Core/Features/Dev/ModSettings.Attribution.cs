using System;
using TheArchive.Core.Localization;
using TheArchive.Utilities;
using static TheArchive.Features.Dev.ModSettings.SettingsCreationHelper;

using AttrData = TheArchive.Core.Attribution;

namespace TheArchive.Features.Dev;

public partial class ModSettings
{
    internal class Attribution : IDisposable
    {
        private readonly SubMenu _submenu;

        internal Attribution()
        {
            _submenu = new DynamicSubMenu(ArchiveLocalizationService.GetById(60, "Attribution & Licenses"), BuildSubMenu, identifier: "Attribution and Licenses Menu");
            
            using var _ = _submenu.GetPersistentContentAdditionToken();

            CreateSettingsItem($"<<< {ArchiveLocalizationService.GetById(38, "Back")} <<<", out var outof_sub_cm_settingsItem, titleColor: RED, subMenu: _submenu);
            outof_sub_cm_settingsItem.ForcePopupLayer(true);
            outof_sub_cm_settingsItem.SetCMItemEvents((_) => _submenu.Close());
            
            CreateSpacer(_submenu);
        }

        private void BuildSubMenu(DynamicSubMenu subMenu)
        {
            foreach (var info in AttrData.AttributionInfos)
            {
                CreateHeader(info.Name, out var cm_settingsItem, ORANGE, subMenu: subMenu);
                
                var data = new DescriptionPanelData {
                    Title = info.Name,
                    Description = info.Content,
                    CriticalInfo = $"<#FFF>{info.Comment}</color>",
                    FeatureOrigin = info.Origin,
                };
                
                CreateFSHoverAndSetButtonAction(data, cm_settingsItem, toggleButton_cm_item: null);
            }
        }

        internal void InsertMenuButton()
        {
            CreateSimpleButton(ArchiveLocalizationService.GetById(60, "Attribution & Licenses"), ArchiveLocalizationService.GetById(28, "Open"), () =>
            {
                _submenu.Show();
            });
        }
    
        public void Dispose()
        {
            _submenu?.Dispose();
        }
    }
}