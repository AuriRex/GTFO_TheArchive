using Enemies;
using System;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;

namespace TheArchive.Features.Hud;

internal class BioPingColor : Feature
{
    public override string Name => "Bio Ping Colors";

    public override FeatureGroup Group => FeatureGroups.Hud;

    public override string Description => "Customize the color of Bio Tracker Pings as well as the blobs on its display.\n\nSingle color, does not differentiate between enemies.";


    public new static IArchiveLogger FeatureLogger { get; set; }

    public static IValueAccessor<EnemyAgent, NavMarker> A_EnemyAgent_m_tagMarker;
    public static IValueAccessor<ES_Hibernate, EnemyAgent> A_ES_Hibernate_m_enemyAgent;
    public static IValueAccessor<ES_PathMove, EnemyAgent> A_ES_PathMove_m_enemyAgent;

    public override void Init()
    {
        A_EnemyAgent_m_tagMarker = AccessorBase.GetValueAccessor<EnemyAgent, NavMarker>("m_tagMarker");
        A_ES_Hibernate_m_enemyAgent = AccessorBase.GetValueAccessor<ES_Hibernate, EnemyAgent>("m_enemyAgent");
        A_ES_PathMove_m_enemyAgent = AccessorBase.GetValueAccessor<ES_PathMove, EnemyAgent>("m_enemyAgent");
    }

    public override void OnEnable()
    {
        RefreshColorsIfNeeded();
    }

    public override void OnFeatureSettingChanged(FeatureSetting setting)
    {
        if(setting.Type == typeof(SColor))
        {
            _pingColor = null;
            _displayColor = null;

            RefreshColorsIfNeeded();
        }
    }

    [FeatureConfig]
    public static BioPingColorSettings Settings { get; set; }

    public class BioPingColorSettings
    {
        [FSDisplayName("Bio Ping Color")]
        [FSDescription("Set the color of bio tracker pings: <#f00><u>/!\\</u></color>")]
        public SColor PingColor { get; set; } = SColor.RED;

        [FSDisplayName("Separate Blob Color")]
        [FSDescription("Should blobs on the Bio Tracker Screen be a different color?")]
        public bool UseSeparateColorForTrackerDisplay { get; set; } = false;

        [FSDisplayName("Bio Display Blob Color")]
        [FSDescription("The color of the blobs on the Bio Tracker Screen")]
        public SColor DisplayColor { get; set; } = SColor.RED;
    }

    private static Color? _pingColor = null;
    private static Color? _displayColor = null;

    private static void RefreshColorsIfNeeded()
    {
        if (!_pingColor.HasValue)
        {
            _pingColor = Settings.PingColor.ToUnityColor();
        }

        if (!_displayColor.HasValue)
        {
            _displayColor = Settings.DisplayColor.ToUnityColor();
        }
    }

    public static void SetScannerColor(EnemyAgent enemy)
    {
        if (enemy == null)
            return;
        
        var pingColor = _pingColor!.Value;
        var displayColor = _displayColor!.Value;

        enemy.ScannerColor = Settings.UseSeparateColorForTrackerDisplay ? displayColor : pingColor;
        /*enemy.m_scannerColor = Settings.UseSeparateColorForTrackerDisplay ? displayColor : pingColor;
        enemy.m_hasDirtyScannerColor = true;*/
    }

    //public void SyncPlaceNavMarkerTag()
    [ArchivePatch(typeof(EnemyAgent), nameof(EnemyAgent.SyncPlaceNavMarkerTag))]
    internal static class EnemyAgent_SyncPlaceNavMarkerTag_Patch
    {
        public static void Postfix(EnemyAgent __instance)
        {
            A_EnemyAgent_m_tagMarker.Get(__instance)?.SetColor(_pingColor!.Value);

            SetScannerColor(__instance);
        }
    }

    [ArchivePatch(typeof(ES_Hibernate), "CommonExit")]
    internal static class ES_Hibernate_CommonExit_Patch
    {
        public static void Postfix(ES_Hibernate __instance)
        {
            SetScannerColor(A_ES_Hibernate_m_enemyAgent.Get(__instance));
        }
    }

    [ArchivePatch(typeof(ES_PathMove), "CommonEnter")]
    internal static class ES_PathMove_CommonEnter_Patch
    {
        public static void Postfix(ES_PathMove __instance)
        {
            SetScannerColor(A_ES_PathMove_m_enemyAgent.Get(__instance));
        }
    }

#if IL2CPP
    [RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
    [ArchivePatch(nameof(ES_PathMoveFlyer.CommonEnter))]
    internal static class ES_PathMoveFlyer_CommonEnter_Patch
    {
        public static Type Type() => typeof(ES_PathMoveFlyer);

        public static void Postfix(ES_PathMoveFlyer __instance)
        {
            SetScannerColor(__instance.m_enemyAgent);
        }
    }
#endif
}