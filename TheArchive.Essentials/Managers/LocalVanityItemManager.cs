using GameData;
using System;
using System.IO;
using TheArchive.Core;
using TheArchive.Interfaces;
using TheArchive.Models.Progression;
using TheArchive.Models.Vanity;
using TheArchive.Utilities;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Managers;

public class LocalVanityItemManager : InitSingletonBase<LocalVanityItemManager>, IInitAfterDataBlocksReady, IInitCondition, IInjectLogger
{
    public IArchiveLogger Logger { get; set; }

    private LocalVanityItemStorage _localVanityItemStorage;
    public LocalVanityItemStorage LocalVanityItemPlayerData
    {
        get
        {
            if (_localVanityItemStorage == null)
            {
                // Load from disk
                _localVanityItemStorage = LoadFromLocalFile();

                if (_localVanityItemStorage.Items.Count == 0)
                {
                    Dropper.DropFirstTimePlayingItems(_localVanityItemStorage);
                }
            }

            return _localVanityItemStorage;
        }
    }

    private LocalVanityAcquiredLayerDrops _acquiredLayerDrops;
    public LocalVanityAcquiredLayerDrops AlreadyAcquiredLayerDrops => _acquiredLayerDrops ??= LoadAcquiredLayerDrops();

    private LocalVanityItemDropper Dropper => LocalVanityItemDropper.Instance;

    public bool InitCondition()
    {
        return ArchiveMod.CurrentBuildInfo.Rundown.IsIncludedIn(RundownFlags.RundownSix.ToLatest());
    }

    public void Init()
    {
        LocalProgressionManager.OnExpeditionCompleted += OnExpeditionCompleted;
    }

    public void OnExpeditionCompleted(ExpeditionCompletionData data)
    {
        if (!data.Success)
            return;

        CheckFirstTimeExpeditionCompletion(data);
        CheckTotalUniqueCompletionsRequirementMet(data);
    }

    public void CheckTotalUniqueCompletionsRequirementMet(ExpeditionCompletionData data)
    {
        try
        {
            if (data.RundownId == 0)
            {
                Logger.Error($"[{nameof(CheckTotalUniqueCompletionsRequirementMet)}] {nameof(ExpeditionCompletionData)}.{nameof(ExpeditionCompletionData.RundownId)} returned 0!");
                return;
            }

            var vanityItemLayerDropDataBlockPersistentID = RundownDataBlock.GetBlock(data.RundownId)?.VanityItemLayerDropDataBlock;

            if (!vanityItemLayerDropDataBlockPersistentID.HasValue)
            {
                Logger.Error($"[{nameof(CheckTotalUniqueCompletionsRequirementMet)}] {nameof(vanityItemLayerDropDataBlockPersistentID)} has no value!");
                return;
            }

            VanityItemsLayerDropsDataBlock vilddb = VanityItemsLayerDropsDataBlock.GetBlock(vanityItemLayerDropDataBlockPersistentID.Value);

            if (vilddb == null)
            {
                Logger.Error($"[{nameof(CheckTotalUniqueCompletionsRequirementMet)}] {nameof(VanityItemsLayerDropsDataBlock)} with persistent ID {vanityItemLayerDropDataBlockPersistentID} could not be found!");
                return;
            }

            bool anyDropped = false;

            foreach (var layerDropData in vilddb.LayerDrops)
            {
                var layer = layerDropData.Layer.ToCustom();
                var count = layerDropData.Count;
                var isAll = layerDropData.IsAll;

                string key = $"{vilddb.name}:{layer}_{count}_{isAll}";

                if (LocalProgressionManager.Instance.CurrentLoadedLocalProgressionData.GetUniqueExpeditionLayersStateCount(layer) >= count)
                {
                    if (!AlreadyAcquiredLayerDrops.HasBeenClaimed(key))
                    {
                        Logger.Notice($"Dropping layer milestone reached rewards for \"{key}\" ...");
                        foreach (var group in layerDropData.Groups)
                        {
                            anyDropped |= Dropper.DropRandomFromGroup(group, LocalVanityItemPlayerData);
                        }

                        AlreadyAcquiredLayerDrops.Claim(key);
                    }
                }
            }

            if (anyDropped)
            {
                SaveAcquiredLayerDrops(AlreadyAcquiredLayerDrops);
            }
        }
        catch(Exception ex)
        {
            Logger.Error("Checking for unique layer drops completion rewards failed!");
            Logger.Exception(ex);
        }
    }

    public void CheckFirstTimeExpeditionCompletion(ExpeditionCompletionData data)
    {
        if (!data.WasFirstTimeCompletion) return;
        try
        {
            if (data.RundownId == 0)
            {
                Logger.Error($"[{nameof(CheckFirstTimeExpeditionCompletion)}] {nameof(ExpeditionCompletionData)}.{nameof(ExpeditionCompletionData.RundownId)} returned 0!");
                return;
            }

            RundownDataBlock rddb = RundownDataBlock.GetBlock(data.RundownId);

            char tierCharacter = data.ExpeditionId[0];
            int.TryParse(data.ExpeditionId[1].ToString(), out var expeditionIndex);

            expeditionIndex--;

            var tier = Utils.GetEnumFromName<eRundownTier>($"Tier{tierCharacter}");

            DropFirstTimeCompletionRewards(rddb.GetExpeditionData(tier, expeditionIndex));
        }
        catch(Exception ex)
        {
            Logger.Error("Something went wrong:");
            Logger.Exception(ex);
        }
    }

    public void DropFirstTimeCompletionRewards(GameData.ExpeditionInTierData expeditionData)
    {
        if(expeditionData.VanityItemsDropData.Groups.Count > 0)
        {
            Logger.Notice("Dropping first time completion rewards ...");
            foreach (var group in expeditionData.VanityItemsDropData.Groups)
            {
                Dropper.DropRandomFromGroup(group, LocalVanityItemPlayerData);
            }
        }
    }

    public object ProcessTransaction(DropServer.VanityItems.VanityItemServiceTransaction trans)
    {
        if(trans != null)
        {
            if(trans.AcknowledgeIds != null) AcknowledgeIds(trans.AcknowledgeIds);
            if(trans.TouchIds != null) TouchIds(trans.TouchIds);
        }

        return GetVanityItemPlayerData();
    }

    public object GetVanityItemPlayerData()
    {
        SaveToLocalFile(LocalVanityItemPlayerData);
        return LocalVanityItemPlayerData.ToBaseGame();
    }

    public void AcknowledgeIds(uint[] ids)
    {
        foreach(var id in ids)
        {
            LocalVanityItemPlayerData.SetFlag(id, LocalVanityItemStorage.VanityItemFlags.Acknowledged);
        }
    }

    public void TouchIds(uint[] ids)
    {
        foreach (var id in ids)
        {
            LocalVanityItemPlayerData.SetFlag(id, LocalVanityItemStorage.VanityItemFlags.Touched);
        }
    }

    public static void SaveToLocalFile(LocalVanityItemStorage data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        Instance.Logger.Msg(ConsoleColor.DarkRed, $"Saving VanityItems to disk at: {LocalFiles.VanityItemsPath}");
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(LocalFiles.VanityItemsPath, json);
    }

    public static LocalVanityItemStorage LoadFromLocalFile()
    {
        Instance.Logger.Msg(ConsoleColor.Green, $"Loading VanityItems from disk at: {LocalFiles.VanityItemsPath}");
        if (!File.Exists(LocalFiles.VanityItemsPath))
            return new LocalVanityItemStorage();
        var json = File.ReadAllText(LocalFiles.VanityItemsPath);

        return JsonConvert.DeserializeObject<LocalVanityItemStorage>(json);
    }

    public static void SaveAcquiredLayerDrops(LocalVanityAcquiredLayerDrops data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        Instance.Logger.Fail($"Saving LocalVanityAcquiredLayerDrops to disk at: {LocalFiles.VanityItemsLayerDropsPath}");
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(LocalFiles.VanityItemsLayerDropsPath, json);
    }

    public static LocalVanityAcquiredLayerDrops LoadAcquiredLayerDrops()
    {
        Instance.Logger.Success($"Loading LocalVanityAcquiredLayerDrops from disk at: {LocalFiles.VanityItemsLayerDropsPath}");
        if (!File.Exists(LocalFiles.VanityItemsLayerDropsPath))
            return new LocalVanityAcquiredLayerDrops();
        var json = File.ReadAllText(LocalFiles.VanityItemsLayerDropsPath);

        return JsonConvert.DeserializeObject<LocalVanityAcquiredLayerDrops>(json);
    }
}