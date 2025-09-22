using GameData;
using LevelGeneration;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Core.Models;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Hud;

[RundownConstraint(RundownFlags.RundownTwo, RundownFlags.Latest)]
internal class CarryItemMarker : Feature
{
    public override string Name => "Carry Item Marker";

    public override GroupBase Group => GroupManager.Hud;

    public override string Description => "Adds a marker for whenever someone carries a big pickup like CELLs or FOG_TURBINEs\n\nAdditionally colorizes the marker based on what item it is.";

    public static bool IsEnabled { get; set; }

    public new static IArchiveLogger FeatureLogger { get; set; }

    [FeatureConfig]
    public static CarryItemMarkerSettings Settings { get; set; }

    public class CarryItemMarkerSettings
    {
        [FSDisplayName("Colorize Marker")]
        [FSDescription("Change the marker color.\nturn off for default green color")]
        public bool ColorizeMarker { get; set; } = true;

        [FSDisplayName("Show Is-Carrying Marker")]
        [FSDescription("Adds a \"Carrying\" Marker to players that carry a pickup.")]
        public bool ShowIsCarryingMarker { get; set; } = true;

        [FSDisplayName("Display Item Name")]
        public bool DisplayItemName { get; set; } = true;

        [FSSlider(0.1f, 3f)]
        [FSDisplayName("Item Name Size")]
        [FSDescription("Size of the name text.")]
        public float ItemNameSize { get; set; } = 1f;



        [FSHeader("Colors")]
        [ItemID("CELL")]
        [FSDisplayName("CELL")]
        public SColor PowerCell { get; set; } = new SColor(0.8f, 0.471f, 0);

        [ItemID("FOG_TURBINE")]
        [FSDisplayName("FOG_TURBINE")]
        public SColor FogTurbine { get; set; } = new SColor(0.863f, 1f, 0.98f);

        [ItemID("NEONATE HSU", "NEONATE_HSU")]
        [FSDisplayName("NEONATE_HSU")]
        public SColor Neonate { get; set; } = new SColor(0.153f, 0.666f, 0.478f);

        [ItemID("IMPRINTED NEONATE HSU")]
        [FSDisplayName("IMPRINTED_NEONATE_HSU")]
        public SColor ImprintedNeonate { get; set; } = new SColor(0.6f, 0.215f, 0.062f);

        [ItemID("CRYO")]
        [FSDisplayName("CRYO")]
        public SColor Cryo { get; set; } = new SColor(0f, 0.819f, 1f);

        [ItemID("Carry_CargoCrate_Generic", "CARGO")]
        [FSDisplayName("CARGO")]
        public SColor Cargo { get; set; } = new SColor(0.01f, 0.514f, 0f);

        [ItemID("Carry_CargoCrate_Generic_R2A1")]
        [FSDisplayName("CARGO (R2A1)")]
        public SColor CargoAlternate { get; set; } = new SColor(0.71f, 0.71f, 0.71f);

        [ItemID("HISEC_CARGO")]
        [FSDisplayName("HISEC_CARGO")]
        public SColor CargoHisec { get; set; } = new SColor(0.435f, 0.58f, 0f);

        [ItemID("HISEC_CARGO_OPEN")]
        [FSDisplayName("HISEC_CARGO_OPEN")]
        public SColor OpenCargo { get; set; } = new SColor(0.419f, 0.455f, 0.314f);

        [ItemID("DATA_SPHERE")]
        [FSDisplayName("DATA_SPHERE")]
        public SColor DataSphere { get; set; } = new SColor(0.36f, 0.165f, 0.525f);

        [ItemID("COLLECTION_CASE")]
        [FSDisplayName("COLLECTION_CASE")]
        public SColor CollectionCase { get; set; } = new SColor(0.808f, 0.62f, 0.753f);

        [ItemID("MATTER_WAVE_PROJECTOR")]
        [FSDisplayName("MATTER_WAVE_PROJECTOR")]
        public SColor MatterWaveProjector { get; set; } = new SColor(0.133f, 0.133f, 0.827f);

        [FSDisplayName("Fallback Color")]
        public SColor Fallback { get; set; } = new SColor(0.341f, 0.576f, 0.137f);



        [FSHeader("Static Colors")]
        [FSDisplayName("Use Static Name Color")]
        public bool UseStaticColorForName { get; set; } = false;

        [FSDisplayName("Static Name Color")]
        [FSDescription("Enable setting above to use this color for every pickup name.")]
        public SColor StaticNameColor { get; set; } = new SColor(0.8f, 0.8f, 0.8f);

        [FSDisplayName("Use Static Marker Color")]
        public bool UseStaticColorForMarker { get; set; } = false;

        [FSDisplayName("Static Marker Color")]
        [FSDescription("Enable setting above to use this color for every pickup marker.")]
        public SColor StaticMarkerColor { get; set; } = new SColor(0.8f, 0.8f, 0.8f);

    }

    #region ColorThings
    public static readonly Color MARKER_GREEN = new Color(0.341f, 0.576f, 0.137f);

    private class ItemIDAttribute : Attribute
    {
        public string[] IDs { get; private set; }
        public ItemIDAttribute(params string[] id)
        {
            IDs = id;
        }
    }

    private static readonly Dictionary<string, PropertyInfo> _idToSColorProp = new Dictionary<string, PropertyInfo>();

    public override void Init()
    {
        var properties = typeof(CarryItemMarkerSettings)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(pi => pi.GetCustomAttribute<ItemIDAttribute>() != null);

        foreach (var prop in properties)
        {
            var identifiers = prop.GetCustomAttribute<ItemIDAttribute>();

            foreach (var identifier in identifiers.IDs)
            {
                _idToSColorProp.Add(identifier, prop);
            }
        }
    }

    public static bool GetColor(string id, out SColor col)
    {
        if (!_idToSColorProp.TryGetValue(id, out var prop))
        {
            col = SColor.WHITE;
            return false;
        }

        col = (SColor)prop.GetValue(Settings);
        return true;
    }
    #endregion ColorThings

    public const string MARKER_GO_NAME = $"{nameof(CarryItemMarker)}_MarkerHolder";
    public static bool TryGetOrAddMarkerPlacer(PlayerAgent playerAgent, out PlaceNavMarkerOnGO markerPlacer)
    {
        markerPlacer = GetOrAddMarkerPlacer(playerAgent);
        return markerPlacer != null;
    }

    private static PlaceNavMarkerOnGO GetOrAddMarkerPlacer(PlayerAgent playerAgent)
    {
        var markerHolderTrans = playerAgent.transform.GetChildWithExactName(MARKER_GO_NAME);

        PlaceNavMarkerOnGO markerPlacer;

        if (markerHolderTrans == null)
        {
            markerHolderTrans = new GameObject(MARKER_GO_NAME).transform;
            var bounds = playerAgent.gameObject.GetMaxBounds();
            markerHolderTrans.SetParent(playerAgent.transform);
            markerHolderTrans.SetPositionAndRotation(bounds.center, Quaternion.identity);


            markerPlacer = markerHolderTrans.gameObject.AddComponent<PlaceNavMarkerOnGO>();
            markerPlacer.type = PlaceNavMarkerOnGO.eMarkerType.Waypoint;
            markerPlacer.PlaceMarker(markerHolderTrans.gameObject);
            markerPlacer.SetMarkerVisible(false);
        }
        else
        {
            markerPlacer = markerHolderTrans.GetComponent<PlaceNavMarkerOnGO>();
        }

        return markerPlacer;
    }

    public static void OnSyncStateChanged(LG_PickupItem_Sync sync, ePickupItemStatus status, pPickupPlacement placement, PlayerAgent player, bool isRecall)
    {
        if (!IsEnabled)
            return;

        var pickupCore = sync.GetComponent<CarryItemPickup_Core>();

        if (pickupCore == null)
            return;

        try
        {
            Color markerColor = MARKER_GREEN;

            if (Settings.UseStaticColorForMarker)
            {
                markerColor = Settings.StaticMarkerColor.ToUnityColor();
            }
            else
            {
                if (Settings.ColorizeMarker)
                {
                    markerColor = TryGetColorFor(pickupCore.ItemDataBlock);
                }
            }

            var nameColor = Settings.UseStaticColorForName ? Settings.StaticNameColor.ToUnityColor() : markerColor;

            var itemNameSize = Mathf.Clamp(Mathf.RoundToInt(60f * Settings.ItemNameSize), 6, 300);

            var markerText = Settings.DisplayItemName ? $"<{nameColor.ToHexString()}><size={itemNameSize}%>{pickupCore.PublicName}</size></color>" : string.Empty;

            pickupCore.m_navMarkerPlacer.UpdateName(markerText);
            pickupCore.m_navMarkerPlacer.UpdatePlayerColor(markerColor);

            if (player == null)
                return;

            if (!TryGetOrAddMarkerPlacer(player, out var playerCarryMarkerPlacer))
                return;

            if (!Settings.ShowIsCarryingMarker)
            {
                playerCarryMarkerPlacer.SetMarkerVisible(false);
                return;
            }

            playerCarryMarkerPlacer.UpdateName(Settings.DisplayItemName ? $"<#FFF><size={Mathf.RoundToInt(itemNameSize * 0.833333f)}%>Carrying</size></color>" : string.Empty, markerText);
            playerCarryMarkerPlacer.UpdatePlayerColor(markerColor);

            playerCarryMarkerPlacer.SetMarkerVisible(status == ePickupItemStatus.PickedUp && !player.IsLocallyOwned);
        }
        catch (Exception ex)
        {
            FeatureLogger.Exception(ex);
        }
    }

    private static Color TryGetColorFor(ItemDataBlock itemDataBlock) => TryGetSColorFor(itemDataBlock).ToUnityColor();

    private static SColor TryGetSColorFor(ItemDataBlock itemDataBlock)
    {
        if (GetColor(itemDataBlock.name, out var color))
        {
            return color;
        }

        if (GetColor(itemDataBlock.terminalItemShortName, out color))
        {
            return color;
        }

        return Settings.Fallback;
    }

    public static void SubscribeToOnSyncStateChange(LG_PickupItem_Sync pickupSync)
    {
        // I would have just patched CarryItemPickup_Core.OnSyncStateChange, but that messes with the struct probably? idk
        // it just deleted my powercell even without touching anything (code wise) so that's that I guess
        pickupSync.OnSyncStateChange += new Action<ePickupItemStatus, pPickupPlacement, PlayerAgent, bool>((status, placement, player, recall) =>
        {
            OnSyncStateChanged(pickupSync, status, placement, player, recall);
        });
    }

    [ArchivePatch(typeof(LG_PickupItem_Sync), nameof(LG_PickupItem_Sync.Setup))]
    internal static class LG_PickupItem_Sync_Setup_Patch
    {
        public static void Postfix(LG_PickupItem_Sync __instance)
        {
            SubscribeToOnSyncStateChange(__instance);
        }
    }
}