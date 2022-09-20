using GameData;
using System.Collections.Generic;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
#if IL2CPP
using IL2ColGen = Il2CppSystem.Collections.Generic;
#else
using IL2ColGen = System.Collections.Generic;
#endif

namespace TheArchive.Features.Special
{
    [EnableFeatureByDefault]
    internal class CustomWeaponFOV : Feature
    {
        public override string Name => nameof(CustomWeaponFOV);

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
            public Dictionary<string, CustomItemFPSSettingsEntry> ItemFPSSettings { get; set; } = new Dictionary<string, CustomItemFPSSettingsEntry>();

        }

        public class CustomItemFPSSettingsEntry
        {
            public int ItemCameraFOVDefault { get; set; }

            public int ItemCameraFOVZoom { get; set; }

            public int LookCameraFOVZoom { get; set; }

            public void SetValuesOnBlock(ItemFPSSettingsDataBlock block)
            {
                block.ItemCameraFOVDefault = ItemCameraFOVDefault;
                block.ItemCameraFOVZoom = ItemCameraFOVZoom;
                block.LookCameraFOVZoom = LookCameraFOVZoom;
            }
        }
    }
}
