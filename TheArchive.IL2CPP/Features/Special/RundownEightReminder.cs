using GameData;
using System.Collections;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Features.Dev;
using TheArchive.Features.QoL;
using TheArchive.Loader;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Special
{
    [EnableFeatureByDefault]
    [RundownConstraint(RundownFlags.RundownAltSix, RundownFlags.Latest)]
    internal class RundownEightReminder : Feature
    {
        public override string Name => "Rundown 8 Reminder";

        public override string Group => FeatureGroups.Special;

        public override string Description => "Reminds you to turn off \"Remove Story Dialog\" for whenever Rundown 8 drops!";

        public override bool SkipInitialOnEnable => true;

        [FeatureConfig]
        public static RundownEightReminderSettings Settings { get; set; }

        public class RundownEightReminderSettings
        {
            [FSHide]
            public bool HasShownTheMessage { get; set; } = false;
        }

        public override void OnEnable()
        {
            if (!GameDataInited)
                return;

            TryShowReminderPopup(false);
        }

        private static bool _onlyOnce = true;
        public void OnGameStateChanged(eGameStateName state)
        {
            if (_onlyOnce && state == eGameStateName.NoLobby)
            {
                _onlyOnce = false;

                if (Settings.HasShownTheMessage)
                    return;

                TryShowReminderPopup();
            }
        }

        private static void TryShowReminderPopup(bool delayed = true)
        {
            var setupDB = GameSetupDataBlock.GetBlock(1);
            if (setupDB.RundownIdsToLoad.Count > 7)
            {
                if(delayed)
                {
                    LoaderWrapper.StartCoroutine(DelayedShowReminderPopup());
                    return;
                }

                ShowReminderPopup();
            }
        }

        private static IEnumerator DelayedShowReminderPopup()
        {
            yield return new WaitForSeconds(4f);
            ShowReminderPopup();
        }

        public static void ShowReminderPopup()
        {
            var noStoryDialog = FeatureManager.GetByType<NoStoryDialog>();

            if (!noStoryDialog.Enabled)
                return;

            if (!Settings.HasShownTheMessage)
            {
                Settings.HasShownTheMessage = true;
                MarkSettingsAsDirty(Settings);
            }

            var updateText = $"Hi, quick reminder that you have <color=orange>{noStoryDialog.Name}</color> <#0f0>enabled</color> and that <color=orange><b>might impact your Rundown 8 experience</b></color>!\n\nConsider turning it <#F00>off</color> in mod settings <color=orange>if you care about the Story</color>!\n\nYou can find it under:\n<color=orange>[Mod Settings] > [{noStoryDialog.Group}] > [{noStoryDialog.Name}]</color>";

            PageRundownPopupManager.ShowPopup(new PopupMessage()
            {
                BlinkInContent = true,
                BlinkTimeInterval = 0.2f,
                Header = "<#F00>Rundown 8 Reminder</color>",
                UpperText = updateText,
                PopupType = PopupType.BoosterImplantMissed,
                OnCloseCallback = new System.Action(() => { })
            });
        }
    }
}
