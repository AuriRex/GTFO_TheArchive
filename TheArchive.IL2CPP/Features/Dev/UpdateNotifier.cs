using System;
using System.Collections;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Loader;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownFive, RundownFlags.Latest)]
    internal class UpdateNotifier : Feature
    {
        public override string Name => "Update Notifier";

        public override string Group => FeatureGroups.ArchiveCore;

        public override string Description => "Shows a popup whenever a new version is available.";

        public const string CMD_ARG_DISABLE_AUTO_UPDATE_CHECKER = "--ar.noupdater";

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static UpdateNotifierSettings Settings { get; set; }

        public class UpdateNotifierSettings
        {
            [FSDisplayName("Check Updates on start")]
            [FSDescription("Checks for updates every time the game starts!")]
            public bool EnableAutoUpdateChecker { get; set; } = true;

            [FSHeader("://Popup Settings")]
            [FSDisplayName("Show only once")]
            [FSDescription("Shows the update available popup only once per <b>new</b> version whenever you launch the game.\n\n<size=80%>(If this is on and the new version is equal to the one in the text below it won't show it)</size>")]
            public bool ShowOnlyOncePerVersion { get; set; } = false;

            [FSReadOnly]
            [FSDisplayName("Last Shown Version")]
            public string LastShownTag { get; set; } = string.Empty;

            [FSSpacer]
            [FSDisplayName("Check for Updates")]
            [FSDescription("Manually check for updates.")]
            public FButton ShowUpdatesButton { get; set; } = new FButton("Check!", nameof(ShowUpdatesButton));

            [FSHide]
            public bool FirstEverPopup { get; set; } = true;
        }

        public override bool ShouldInit()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Any(arg => arg == CMD_ARG_DISABLE_AUTO_UPDATE_CHECKER))
            {
                FeatureLogger.Info("Disabling update checker via command line argument.");
                return false;
            }

            return true;
        }

        public override void OnGameDataInitialized()
        {
            if (Settings.EnableAutoUpdateChecker)
            {
                UpdateChecker.CheckForUpdate(null!);
            }
        }

        public override void OnButtonPressed(ButtonSetting setting)
        {
            if (setting.ButtonID == nameof(UpdateNotifierSettings.ShowUpdatesButton))
            {
                UpdateChecker.CheckForUpdate((releaseInfo) =>
                {
                    ShowUpdatesPopup(showUpToDate: true);
                });
            }
        }

        private static bool _firstTime = true;
        public void OnGameStateChanged(eGameStateName state)
        {
            if(state == eGameStateName.NoLobby && _firstTime)
            {
                _firstTime = false;

                LoaderWrapper.StartCoroutine(DelayedUpdatesPopup());
            }
        }

        private static IEnumerator DelayedUpdatesPopup()
        {
            yield return new WaitForSeconds(5);
            ShowUpdatesPopup();
        }

        public static void ShowUpdatesPopup(bool showUpToDate = false)
        {
            string updateText;

            if(UpdateChecker.HasReleaseInfo && !UpdateChecker.IsOnLatestRelease)
            {
                var latestTag = UpdateChecker.LatestReleaseInfo.Value.Tag;

                if (Settings.ShowOnlyOncePerVersion && Settings.LastShownTag == latestTag)
                {
                    FeatureLogger.Notice($"Not showing popup for version {latestTag} again!");
                    return;
                }

                if (Settings.LastShownTag != latestTag)
                {
                    Settings.LastShownTag = latestTag;
                    MarkSettingsAsDirty(Settings);
                }

                updateText = $"<size=150%>A new version of <color=orange>TheArchive</color> is available!</size>\n\nVersion <color=orange>{latestTag}</color> is available on Github!\n\nCurrently installed version: <color=orange>{ArchiveMod.GIT_BASE_TAG}</color>";
            }
            else
            {
                if (!showUpToDate)
                    return;

                updateText = $"<size=150%>No new version found!</size>\n\nYou seem to be up to date!\n\nCurrently installed version: <color=orange>{ArchiveMod.GIT_BASE_TAG}</color>";
            }

            if (Settings.FirstEverPopup)
            {
                Settings.FirstEverPopup = false;
                MarkSettingsAsDirty(Settings);
                updateText = $"{updateText}\n\n<size=80%><color=orange>(This can be turned off in mod settings!)</color>\n[Mod Settings] > [{FeatureGroups.ArchiveCore.Name}] > [{nameof(UpdateNotifier)}]</size>";
            }

            GlobalPopupMessageManager.ShowPopup(new PopupMessage()
            {
                BlinkInContent = true,
                BlinkTimeInterval = 0.2f,
                Header = "<#440144>TheArchive Update Checker</color>",
                UpperText = updateText,
                PopupType = PopupType.BoosterImplantMissed,
            });
        }
    }
}
