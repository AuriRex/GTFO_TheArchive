using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Localization;
using TheArchive.Utilities;
using static TheArchive.Features.Dev.ModSettings.SettingsCreationHelper;

namespace TheArchive.Features.Dev;

public partial class ModSettings
{
    internal class SearchMainPage : IDisposable
    {
        private readonly SubMenu _searchMainSubmenu;
        private readonly DynamicSubMenu _resultsMenu;

        public SearchQuery Query { get; set; } = new SearchQuery();

        public class SearchQuery
        {
            public string QueryString { get; set; } = string.Empty;

            public bool TitleContains { get; set; } = true;
            public bool DesciptionContains { get; set; } = true;

            public bool SubSettingsTitleContains { get; set; } = true;
            public bool SubSettingsDesciptionContains { get; set; } = false;
        }

        internal SearchMainPage()
        {
            _searchMainSubmenu = new SubMenu(ArchiveLocalizationService.GetById(9, "Search"), identifier: "Feature Search Menu");

            Query.TitleContains = Settings.Search.SearchTitles;
            Query.DesciptionContains = Settings.Search.SearchDescriptions;
            Query.SubSettingsTitleContains = Settings.Search.SearchSubSettingsTitles;
            Query.SubSettingsDesciptionContains = Settings.Search.SearchSubSettingsDescription;

            CreateHeader(ArchiveLocalizationService.GetById(11, "Search All Features"), UnityEngine.Color.magenta, subMenu: null);

            CreateSubMenuControls(_searchMainSubmenu, null, ArchiveLocalizationService.GetById(9, "Search"), null, ArchiveLocalizationService.GetById(9, "Search"));

            CreateSpacer(null);

            CreateSimpleTextField($"{ArchiveLocalizationService.GetById(12, "Query")}:", Query.QueryString, OnSearchValueUpdate, placeIntoMenu: _searchMainSubmenu);

            CreateHeader($"{ArchiveLocalizationService.GetById(10, "Search Options")}:", DISABLED, subMenu: _searchMainSubmenu);
            CreateSimpleToggle(ArchiveLocalizationService.GetById(16, "Title"), initialState: Query.TitleContains, TitleContainsToggled, _searchMainSubmenu);
            CreateSimpleToggle(ArchiveLocalizationService.GetById(17, "Description"), initialState: Query.DesciptionContains, DescriptionContainsToggled, _searchMainSubmenu);
            CreateSimpleToggle(ArchiveLocalizationService.GetById(18, "SubSettings Title"), initialState: Query.SubSettingsTitleContains, SubTitleContainsToggled, _searchMainSubmenu);
            CreateSimpleToggle(ArchiveLocalizationService.GetById(19, "SubSettings Description"), initialState: Query.SubSettingsDesciptionContains, SubDescriptionContainsToggled, _searchMainSubmenu);

            CreateSpacer(_searchMainSubmenu);

            _resultsMenu = new DynamicSubMenu(ArchiveLocalizationService.GetById(13, "Search Results"), BuildSearchResultsMenu, identifier: "Search Results Menu");

            CreateSubMenuControls(_resultsMenu, null, menuEntryLabelText: ArchiveLocalizationService.GetById(14, "Start Search"), placeIntoMenu: _searchMainSubmenu, headerText: ArchiveLocalizationService.GetById(13, "Search Results"), enterButtonText: ArchiveLocalizationService.GetById(36));

            CreateHeader(ArchiveLocalizationService.GetById(15, "Search is WIP, Toggling settings here does not visually update the normal buttons currently!"), DISABLED, subMenu: _searchMainSubmenu);

            using (_resultsMenu.GetPersistentContentAdditionToken())
            {
                CreateHeader(ArchiveLocalizationService.GetById(15, "Search is WIP, Toggling settings here does not visually update the normal buttons currently!"), DISABLED, subMenu: _resultsMenu);
            }

            _searchMainSubmenu.Build();
        }

        private void OnSearchValueUpdate(string search)
        {
            Query.QueryString = search;
        }

        public void Dispose()
        {
            _searchMainSubmenu.Dispose();
            _resultsMenu.Dispose();
        }

        #region toggles
        private void TitleContainsToggled(bool state)
        {
            Settings.Search.SearchTitles = state;
            Query.TitleContains = state;
        }

        private void DescriptionContainsToggled(bool state)
        {
            Settings.Search.SearchDescriptions = state;
            Query.DesciptionContains = state;
        }

        private void SubTitleContainsToggled(bool state)
        {
            Settings.Search.SearchSubSettingsTitles = state;
            Query.SubSettingsTitleContains = state;
        }

        private void SubDescriptionContainsToggled(bool state)
        {
            Settings.Search.SearchSubSettingsDescription = state;
            Query.SubSettingsDesciptionContains = state;
        }
        #endregion

        private void BuildSearchResultsMenu(DynamicSubMenu menu)
        {
            var features = SearchForFeatures(Query);

            var count = features.Count();

            string numFoundText;
            if (count == 0)
            {
                numFoundText = ArchiveLocalizationService.GetById(20, "Nothing found! :(");
            }
            else
            {
                numFoundText = ArchiveLocalizationService.Format(21, "Found {0} Feature{1}!", count, count > 1 ? "s" : string.Empty);
            }

            CreateHeader(ArchiveLocalizationService.Format(22, "Query: <color=orange>{0}</color>", Query.QueryString), WHITE_GRAY, subMenu: menu);
            CreateHeader(numFoundText, count == 0 ? RED : GREEN, subMenu: menu);
            CreateSpacer();

            foreach (var feature in features)
            {
                CM_PageSettings__Setup__Patch.SetupEntriesForFeature(feature, menu);
            }
        }

        private bool IncludeHidden(Feature f)
        {
            if (DevMode)
            {
                return true;
            }

            return !f.IsHidden;
        }

        private bool IncludeHidden(FeatureSetting fs)
        {
            if (DevMode)
            {
                return true;
            }

            return !fs.HideInModSettings;
        }

        private bool ContainsIgnoreCase(string text, string value, bool stripTMPTags = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (stripTMPTags)
                text = Utils.StripTMPTagsRegex(text);

            return text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private IEnumerable<Feature> SearchForFeatures(SearchQuery query)
        {
            IEnumerable<Feature> features = FeatureManager.Instance.RegisteredFeatures.AsEnumerable();

            IEnumerable<Feature> result = new HashSet<Feature>();

            if (query.TitleContains)
                result = result.Concat(features.Where(f => IncludeHidden(f) && ContainsIgnoreCase(f.Name, query.QueryString)));

            if (query.DesciptionContains)
                result = result.Concat(features.Where(f => IncludeHidden(f) && ContainsIgnoreCase(f.Description, query.QueryString)));

            if (query.SubSettingsTitleContains)
                result = result.Concat(features.Where(f => f.SettingsHelpers.Any(fsh => fsh.Settings.Any(fs => IncludeHidden(fs) && ContainsIgnoreCase(fs.DisplayName, query.QueryString)))));

            if (query.SubSettingsDesciptionContains)
                result = result.Concat(features.Where(f => f.SettingsHelpers.Any(fsh => fsh.Settings.Any(fs => IncludeHidden(fs) && ContainsIgnoreCase(fs.Description, query.QueryString)))));

            return result.Distinct().OrderBy(f => f.Name);
        }
    }
}