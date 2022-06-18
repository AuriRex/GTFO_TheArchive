using GameEvent;
using System;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using UnityEngine.Analytics;

namespace TheArchive.Features.Dev
{
    [EnableFeatureByDefault, HideInModSettings]
    public class AnalyticsBlocker : Feature
    {
        public override string Name => "Block Game Analytics";
        public override string Group => FeatureGroups.Dev;

        public override void OnEnable()
        {
            Analytics.enabled = false;
        }

        public override void OnDisable()
        {
            Analytics.enabled = true;
        }

        [ArchivePatch(typeof(AnalyticsManager), "OnGameEvent", new Type[] { typeof(GameEventData) })]
        internal static class AnalyticsManager_OnGameEventPatch
        {
            public static bool Prefix() => false;
        }
    }
}
