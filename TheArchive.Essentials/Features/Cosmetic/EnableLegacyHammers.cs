using GameData;
using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Core.Managers;
using TheArchive.Interfaces;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Cosmetic;

[RundownConstraint(RundownFlags.RundownSix, RundownFlags.RundownAltSix)]
public class EnableLegacyHammers : Feature
{
    public override string Name => "Enable old Hammers";

    public override GroupBase Group => GroupManager.Cosmetic;

    public override string Description => "Re-enable the pre-R6 Hammers:\n<color=orange>Maul</color>, <color=orange>Gavel</color>, <color=orange>Sledge</color> and <color=orange>Mallet</color>";

    public override bool RequiresRestart => true;

    public new static IArchiveLogger FeatureLogger { get; set; }

#if IL2CPP
    public override void OnEnable()
    {
        DataBlockManager.RegisterTransformationFor<PlayerOfflineGearDataBlock>(Transform, -10);
    }

    private static readonly Dictionary<string, string> _oldHammers = new Dictionary<string, string>
    {
        { "Maul", "{\"Ver\":1,\"Name\":\"Maul\",\"Packet\":{\"Comps\":{\"Length\":8,\"a\":{\"c\":2,\"v\":14},\"b\":{\"c\":3,\"v\":100},\"c\":{\"c\":4,\"v\":13},\"d\":{\"c\":44,\"v\":3},\"e\":{\"c\":46,\"v\":5},\"f\":{\"c\":48,\"v\":2},\"g\":{\"c\":50,\"v\":5}},\"MatTrans\":{\"tDecalA\":{\"scale\":0.1},\"tDecalB\":{\"scale\":0.1},\"tPattern\":{\"scale\":0.1}},\"publicName\":{\"data\":\"Maul\"}}}" },
        { "Mallet", "{\"Ver\":1,\"Name\":\"Mallet\",\"Packet\":{\"Comps\":{\"Length\":8,\"a\":{\"c\":2,\"v\":14},\"b\":{\"c\":3,\"v\":100},\"c\":{\"c\":4,\"v\":13},\"d\":{\"c\":44,\"v\":6},\"e\":{\"c\":46,\"v\":9},\"f\":{\"c\":48,\"v\":10},\"g\":{\"c\":50,\"v\":8}},\"MatTrans\":{\"tDecalA\":{\"scale\":0.1},\"tDecalB\":{\"scale\":0.1},\"tPattern\":{\"scale\":0.1}},\"publicName\":{\"data\":\"Mallet\"}}}" },
        { "Gavel", "{\"Ver\":1,\"Name\":\"Gavel\",\"Packet\":{\"Comps\":{\"Length\":8,\"a\":{\"c\":2,\"v\":14},\"b\":{\"c\":3,\"v\":100},\"c\":{\"c\":4,\"v\":13},\"d\":{\"c\":44,\"v\":5},\"e\":{\"c\":46,\"v\":3},\"f\":{\"c\":48,\"v\":5},\"g\":{\"c\":50,\"v\":2}},\"MatTrans\":{\"tDecalA\":{\"scale\":0.1},\"tDecalB\":{\"scale\":0.1},\"tPattern\":{\"scale\":0.1}},\"publicName\":{\"data\":\"Gavel\"}}}" },
        { "Sledgehammer", "{\"Ver\":1,\"Name\":\"Sledgehammer\",\"Packet\":{\"Comps\":{\"Length\":8,\"a\":{\"c\":2,\"v\":14},\"b\":{\"c\":3,\"v\":100},\"c\":{\"c\":4,\"v\":13},\"d\":{\"c\":44,\"v\":11},\"e\":{\"c\":46,\"v\":12},\"f\":{\"c\":48,\"v\":6},\"g\":{\"c\":50,\"v\":4}},\"MatTrans\":{\"tDecalA\":{\"scale\":0.1},\"tDecalB\":{\"scale\":0.1},\"tPattern\":{\"scale\":0.1}},\"publicName\":{\"data\":\"Sledgehammer\"}}}" },
    };

    public static void Transform(List<PlayerOfflineGearDataBlock> allPlayerOfflineGearDataBlocks)
    {
        if (Is.A2OrLater)
        {
            // The OG hammers have been removed from the DataBlocks.
            var allBlocksPre = PlayerOfflineGearDataBlock.GetAllBlocks().AsEnumerable();
            foreach (var kvp in _oldHammers)
            {
                if(allBlocksPre.Any(block => block.name == kvp.Key))
                {
                    FeatureLogger.Debug($"> Skipping block '{kvp.Key}' because one with the same name already exists!");
                    continue;
                }
                FeatureLogger.Info($"> Adding old hammer '{kvp.Key}' as new block ...");
                PlayerOfflineGearDataBlock.AddBlock(new PlayerOfflineGearDataBlock
                {
                    Type = eOfflineGearType.StandardInventory,
                    name = kvp.Key,
                    GearJSON = kvp.Value,
                    internalEnabled = true,
                });
            }

            return;
        }

        var blocksToEnable = _oldHammers.Keys.ToList();

        foreach (var block in allPlayerOfflineGearDataBlocks)
        {
            if (blocksToEnable.Contains(block.name))
            {
                FeatureLogger.Success($"> Enabling block: ID: {block.persistentID}, Name: '{block.name}'");
                block.internalEnabled = true;
            }
        }
    }
#endif
}