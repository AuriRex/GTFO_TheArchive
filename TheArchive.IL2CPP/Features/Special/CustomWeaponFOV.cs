using GameData;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
#if IL2CPP
using IL2ColGen = Il2CppSystem.Collections.Generic;
#else
using IL2ColGen = System.Collections.Generic;
#endif

namespace TheArchive.Features.Special
{
    internal class CustomWeaponFOV : Feature
    {
        public override string Name => "Weapon FOV Adjustments";

        public override bool PlaceSettingsInSubMenu => true;

        public new static IArchiveLogger FeatureLogger { get; set; }

        private static Dictionary<string, CustomItemFPSSettingsEntry> _defaultItemFPSSettings;
        private static ItemFPSSettingsDataBlock[] _allFPSSettingsBlocks;

        [FeatureConfig]
        public static WeaponFOVSettings Settings { get; set; }

        public override void OnEnable()
        {
            
            if (DataBlocksReady)
            {
                SetupDefaultData();
                SetCustomValues();
            } 
        }

        public override void OnDisable()
        {
            SetDefaultValues();
        }

        public override void OnDatablocksReady()
        {
            SetupDefaultData();
            SetCustomValues();
        }

        public override void OnFeatureSettingChanged(FeatureSetting setting)
        {
            SetCustomValues();
        }

        [ArchivePatch(typeof(ItemEquippable), nameof(ItemEquippable.GetItemFovZoom))]
        internal static class ItemEquipable_GetItemFovZoom_Patch
        {
            public static bool Prefix(ItemEquippable __instance, ref float __result)
            {
                if (!Settings.IgnoreSightSettings)
                    return ArchivePatch.RUN_OG;

                __result = __instance.ItemFPSData.ItemCameraFOVZoom;

                return ArchivePatch.SKIP_OG;
            }
        }

        [ArchivePatch(typeof(ItemEquippable), nameof(ItemEquippable.GetWorldCameraZoomFov))]
        internal static class ItemEquipable_GetWorldCameraZoomFov_Patch
        {
            public static bool Prefix(ItemEquippable __instance, ref float __result)
            {
                if (!Settings.IgnoreSightSettings)
                    return ArchivePatch.RUN_OG;

                __result = __instance.ItemFPSData.LookCameraFOVZoom;

                return ArchivePatch.SKIP_OG;
            }
        }

        public static string GetID(ItemFPSSettingsDataBlock data)
        {
            if (data == null)
            {
                FeatureLogger.Warning($"Received null {nameof(ItemFPSSettingsDataBlock)}.");
                return string.Empty;
            }
            return $"{data.persistentID}_{data.name}";
        }

        public void SetCustomValues()
        {
            if (_allFPSSettingsBlocks == null) return;
            foreach (var block in _allFPSSettingsBlocks)
            {
                var id = GetID(block);
                if (Settings.ItemFPSSettings.TryGetValue(id, out var value))
                {
                    value.SetValuesOnBlock(block);
                }
            }
        }

        public void SetDefaultValues()
        {
            if (_allFPSSettingsBlocks == null) return;
            foreach (var block in _allFPSSettingsBlocks)
            {
                var id = GetID(block);
                if (_defaultItemFPSSettings.TryGetValue(id, out var value))
                {
                    value.SetValuesOnBlock(block);
                }
            }
        }

        private void SetupDefaultData()
        {
            _allFPSSettingsBlocks = GameDataBlockBase<ItemFPSSettingsDataBlock>.GetAllBlocks();
            if (_defaultItemFPSSettings == null)
            {
                _defaultItemFPSSettings = new Dictionary<string, CustomItemFPSSettingsEntry>();
                foreach (var block in _allFPSSettingsBlocks)
                {
                    var id = GetID(block);
                    _defaultItemFPSSettings.Add(id, new CustomItemFPSSettingsEntry()
                    {
                        ItemCameraFOVDefault = block.ItemCameraFOVDefault,
                        ItemCameraFOVZoom = block.ItemCameraFOVZoom,
                        LookCameraFOVZoom = block.LookCameraFOVZoom,
                    });
                }
            }

            foreach(var defaultSettings in _defaultItemFPSSettings)
            {
                if (!Settings.ItemFPSSettings.ContainsKey(defaultSettings.Key))
                {
                    Settings.ItemFPSSettings.Add(defaultSettings.Key, defaultSettings.Value);
                }
            }
        }

        public class WeaponFOVSettings
        {
            [FSDisplayName("Override Sights Settings")]
            public bool IgnoreSightSettings { get; set; } = true;
            
            [FSDisplayName("Item FPS Settings"), FSReadOnly(recursive: false)]
            public Dictionary<string, CustomItemFPSSettingsEntry> ItemFPSSettings { get; set; } = new Dictionary<string, CustomItemFPSSettingsEntry>();

            [FSHeader("Add to all"), FSHide, FSReadOnly]
            public string IGNORE_EMPTY { get; set; } = string.Empty;

            public CustomItemFPSSettingsEntry GlobalAddition { get; set; } = new CustomItemFPSSettingsEntry();
        }

        public class CustomItemFPSSettingsEntry
        {
            [FSDisplayName("Item FOV Default")]
            public int ItemCameraFOVDefault { get; set; }
            [FSDisplayName("Item FOV ADS")]
            public int ItemCameraFOVZoom { get; set; }
            [FSDisplayName("Camera FOV ADS")]
            public int LookCameraFOVZoom { get; set; }

            public void SetValuesOnBlock(ItemFPSSettingsDataBlock block)
            {
                block.ItemCameraFOVDefault = ItemCameraFOVDefault + Settings.GlobalAddition.ItemCameraFOVDefault;
                block.ItemCameraFOVZoom = ItemCameraFOVZoom + Settings.GlobalAddition.ItemCameraFOVZoom;
                block.LookCameraFOVZoom = LookCameraFOVZoom + Settings.GlobalAddition.LookCameraFOVZoom;
            }
        }
    }
}
