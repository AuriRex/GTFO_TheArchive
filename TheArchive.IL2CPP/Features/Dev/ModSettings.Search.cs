﻿using System;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Utilities;
using static TheArchive.Features.Dev.ModSettings.SettingsCreationHelper;

namespace TheArchive.Features.Dev
{
    public partial class ModSettings
    {
        public class SearchMainPage : IDisposable
        {
            private SubMenu _searchMainSubmenu;
            private DynamicSubMenu _resultsMenu;

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
                _searchMainSubmenu = new SubMenu("Search");

                Query.TitleContains = Settings.Search.SearchTitles;
                Query.DesciptionContains = Settings.Search.SearchDescriptions;
                Query.SubSettingsTitleContains = Settings.Search.SearchSubSettingsTitles;
                Query.SubSettingsDesciptionContains = Settings.Search.SearchSubSettingsDescription;

                CreateHeader("Search All Features", UnityEngine.Color.magenta, subMenu: null);

                CreateSubMenuControls(_searchMainSubmenu, null, "Search", null, "Search");

                CreateSpacer(null);

                CreateSimpleTextField("Query:", Query.QueryString, OnSearchValueUpdate, placeIntoMenu: _searchMainSubmenu);

                CreateHeader("Search Options:", DISABLED, subMenu: _searchMainSubmenu);
                CreateSimpleToggle("Title", initialState: Query.TitleContains, TitleContainsToggled, _searchMainSubmenu);
                CreateSimpleToggle("Description", initialState: Query.DesciptionContains, DescriptionContainsToggled, _searchMainSubmenu);
                CreateSimpleToggle("SubSettings Title", initialState: Query.SubSettingsTitleContains, SubTitleContainsToggled, _searchMainSubmenu);
                CreateSimpleToggle("SubSettings Description", initialState: Query.SubSettingsDesciptionContains, SubDescriptionContainsToggled, _searchMainSubmenu);

                CreateSpacer(_searchMainSubmenu);

                _resultsMenu = new DynamicSubMenu("Search Results", BuildSearchResultsMenu);

                CreateSubMenuControls(_resultsMenu, null, menuEntryLabelText: "Start Search", placeIntoMenu: _searchMainSubmenu, headerText: "Search Results", enterButtonText: "> Search! <");

                CreateHeader("Search is WIP, Toggling settings here does not visually update the normal buttons currently!", DISABLED, subMenu: _searchMainSubmenu);

                using(_resultsMenu.GetPersistentContenAdditionToken())
                {
                    CreateHeader("Search is WIP, Toggling settings here does not visually update the normal buttons currently!", DISABLED, subMenu: _resultsMenu);
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
                    numFoundText = "Nothing found! :(";
                }
                else
                {
                    numFoundText = $"Found {count} Feature{(count > 1 ? "s" : string.Empty)}!";
                }

                CreateHeader($"Query: \"<color=orange>{Query.QueryString}</color>\"", WHITE_GRAY, subMenu: menu);
                CreateHeader(numFoundText, count == 0 ? RED : GREEN, subMenu: menu);
                CreateSpacer();

                foreach(var feature in features)
                {
                    CM_PageSettings_Setup_Patch.SetupEntriesForFeature(feature, menu);
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

                if(stripTMPTags)
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
}
