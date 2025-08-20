using System;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.QoL;

// TODO: Fix for R5
[RundownConstraint(Utils.RundownFlags.RundownSix, Utils.RundownFlags.Latest)]
public class SortBoosters : Feature
{
    public override string Name => "Sort Boosters";

    public override FeatureGroup Group => FeatureGroups.QualityOfLife;

    public override string Description => "Sorts your booster inventory by type and alphabetically";

    public new static IArchiveLogger FeatureLogger { get; set; }

    public override bool SkipInitialOnEnable => true;

    public override void OnGameDataInitialized()
    {
        SubscribeToInventoryChangedEvent(true);
    }

    private static void SubscribeToInventoryChangedEvent(bool subscribe)
    {
        if (subscribe)
        {
            if (_eventRegistered)
                return;

            PersistentInventoryManager.Current.OnBoosterImplantInventoryChanged += _onBoosterImplantInventoryChangedAction;
            _eventRegistered = true;
            return;
        }

        if (!_eventRegistered)
            return;

        PersistentInventoryManager.Current.OnBoosterImplantInventoryChanged -= _onBoosterImplantInventoryChangedAction;
        _eventRegistered = false;
    }


    private static bool _eventRegistered = false;
    private static readonly Action _onBoosterImplantInventoryChangedAction = new Action(OnBoosterImplantInventoryChanged);
    private static void OnBoosterImplantInventoryChanged()
    {
        var inventory = PersistentInventoryManager.Current.m_boosterImplantInventory;

        foreach(var category in inventory.Categories)
        {
            // TODO: Add some customization perhaps?
            category.Inventory = category.Inventory.ToArray().OrderBy(b => b.Implant.Template.MainEffectType + b.Implant.GetCompositPublicName() + b.Implant.Effects[0].Value).ToList().ToIL2CPPListIfNecessary();
        }

        FeatureLogger.Notice("Sorted Boosters!");
    }

    public override void OnEnable()
    {
        SubscribeToInventoryChangedEvent(true);
    }

    public override void OnDisable()
    {
        if (IsApplicationQuitting)
            return;

        SubscribeToInventoryChangedEvent(false);
    }
}