using BoosterImplants;
using GameData;
using Localization;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;
using TheArchive.Utilities;

namespace TheArchive.Features.Security;

[EnableFeatureByDefault]
public class AntiBoosterHack : Feature
{
    public override string Name => "Anti Booster Hack";

    public override string Description => "Prevents clients from using modified boosters.";

    public override FeatureGroup Group => FeatureGroups.Security;

    public new static IArchiveLogger FeatureLogger { get; set; }

    [FeatureConfig]
    public static AntiBoosterHackSettings Settings { get; set; }

    public class AntiBoosterHackSettings
    {
        [FSDisplayName("Punish Friends")]
        [FSDescription("If (Steam) Friends should be affected as well.")]
        public bool PunishFriends { get; set; } = false;

        [FSDisplayName("Punishment")]
        [FSDescription("What to do with griefers that are using modified boosters.")]
        public PunishmentMode Punishment { get; set; } = PunishmentMode.Kick;

        [Localized]
        public enum PunishmentMode
        {
            NoneAndLog,
            Kick,
            KickAndBan
        }
    }

    public override void Init()
    {
        if (ArchiveMod.IsPlayingModded)
        {
            RequestDisable("Playing Modded");
        }
    }

    public override void OnGameDataInitialized()
    {
        BoosterImplantTemplateManager.LoadTemplateData();
    }

    [ArchivePatch(typeof(BoosterImplantManager), nameof(BoosterImplantManager.OnSyncBoosterImplants))]
    private class BoosterImplantManager__OnSyncBoosterImplants__Patch
    {
        private static void Postfix(SNet_Player player, pBoosterImplantsWithOwner pBoosterImplantsWithOwner)
        {
            if (!SNet.IsMaster || pBoosterImplantsWithOwner == null || (player?.IsLocal ?? false) || !SNet.Replication.TryGetLastSender(out var sender))
                return;
            while (true)
            {
                if (pBoosterImplantsWithOwner.BasicImplant.BoosterEffectCount != 0)
                {
                    if (!BoosterImplantTemplateManager.TryGetBoosterImplantTemplate(pBoosterImplantsWithOwner.BasicImplant, BoosterImplantCategory.Muted))
                        break;
                }
                if (pBoosterImplantsWithOwner.AdvancedImplant.BoosterEffectCount != 0)
                {
                    if (!BoosterImplantTemplateManager.TryGetBoosterImplantTemplate(pBoosterImplantsWithOwner.AdvancedImplant, BoosterImplantCategory.Bold))
                        break;
                }
                if (pBoosterImplantsWithOwner.SpecializedImplant.BoosterEffectCount != 0)
                {
                    if (!BoosterImplantTemplateManager.TryGetBoosterImplantTemplate(pBoosterImplantsWithOwner.SpecializedImplant, BoosterImplantCategory.Aggressive))
                        break;
                }
                return;
            }
            PunishPlayer(sender);
        }
    }

    public static bool PunishPlayer(SNet_Player player)
    {
        if (player == null)
            return true;

        if (player.IsFriend() && !Settings.PunishFriends)
        {
            FeatureLogger.Notice($"Friend \"{player.NickName}\" \"{player.Lookup}\" is using modified boosters!");
            return false;
        }

        switch (Settings.Punishment)
        {
            case AntiBoosterHackSettings.PunishmentMode.KickAndBan:
                PlayerLobbyManagement.BanPlayer(player);
                goto default;
            case AntiBoosterHackSettings.PunishmentMode.Kick:
                PlayerLobbyManagement.KickPlayer(player);
                goto default;
            default:
            case AntiBoosterHackSettings.PunishmentMode.NoneAndLog:
                FeatureLogger.Notice($"Player \"{player.NickName}\" \"{player.Lookup}\" is using modified boosters! ({Settings.Punishment})");
                return true;
        }
    }

    public static class BoosterImplantTemplateManager
    {
        public static void LoadTemplateData()
        {
            OldBoosterImplantTemplateDataBlocks.Clear();
            OldBoosterImplantTemplateDataBlocks.AddRange(JsonConvert.DeserializeObject<List<BoosterImplantTemplateDataBlock>>(R5BoosterTemplatesJson, new JsonConverter[]
            {
                new LocalizedTextJsonConverter(), 
                new ListOfTConverter<uint>(), 
                new ListOfTConverter<BoosterImplantEffectInstance>(),
                new ListOfListOfTConverter<BoosterImplantEffectInstance>()
            }));
            BoosterImplantTemplates.Clear();
            var blocks = BoosterImplantTemplateDataBlock.GetAllBlocksForEditor();
            for (int i = 0; i < blocks.Count; i++)
            {
                BoosterImplantTemplates.Add(new(blocks[i]));
            }
            for (int i = 0; i < OldBoosterImplantTemplateDataBlocks.Count; i++)
            {
                BoosterImplantTemplates.Add(new(OldBoosterImplantTemplateDataBlocks[i]));
            }
        }

        public static bool TryGetBoosterImplantTemplate(pBoosterImplantData boosterImplant, BoosterImplantCategory category)
        {
            if (boosterImplant == null) return false;

            uint persistenID = boosterImplant.BoosterImplantID;
            var templates = BoosterImplantTemplates.FindAll(p => p.BoosterImplantID == persistenID && p.ImplantCategory == category);
            for (int k = 0; k < templates.Count; k++)
            {
                var template = templates[k];
                if (template == null || template.TemplateDataBlock == null)
                {
                    continue;
                }

                var conditionGroups = template.ConditionGroups;
                int conditionCount = boosterImplant.ConditionCount;
                bool ConditionMatch = false;
                var conditions = boosterImplant.Conditions.Take(conditionCount);
                for (int i = 0; i < conditionGroups.Count; i++)
                {
                    if (conditionCount != conditionGroups[i].Count)
                    {
                        continue;
                    }

                    bool flag1 = conditions.All(p => conditionGroups[i].Any(q => q == p));
                    bool flag2 = conditionGroups[i].All(p => conditions.Any(q => q == p));
                    if (flag1 && flag2)
                    {
                        ConditionMatch = true;
                        break;
                    }
                }
                if (!ConditionMatch) continue;

                int effectCount = boosterImplant.BoosterEffectCount;
                bool EffectMatch = false;
                var effectGroups = template.EffectGroups;
                var effects = boosterImplant.BoosterEffectDatas.Take(effectCount);
                for (int i = 0; i < effectGroups.Count; i++)
                {
                    if (effectGroups[i].Count != effectCount) continue;

                    for (int j = 0; j < effectGroups[i].Count; j++)
                    {
                        bool flag1 = effects.All(p => effectGroups[i].Any(q => q.BoosterImplantEffect == p.BoosterEffectID
                                                                               && p.EffectValue >= q.EffectMinValue && p.EffectValue <= q.EffectMaxValue));
                        bool flag2 = effectGroups[i].All(p => effects.Any(q => q.BoosterEffectID == p.BoosterImplantEffect
                                                                               && q.EffectValue >= p.EffectMinValue && q.EffectValue <= p.EffectMaxValue));
                        if (flag1 && flag2)
                        {
                            EffectMatch = true;
                            break;
                        }
                    }

                    if (EffectMatch) break;
                }
                if (!EffectMatch) continue;

                var UsageMatch = boosterImplant.UseCount <= (int)template.TemplateDataBlock.DurationRange.y && boosterImplant.UseCount >= 0;
                if (ConditionMatch && EffectMatch && UsageMatch)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<BoosterImplantTemplate> BoosterImplantTemplates { get; } = new();

        private static string _r5BoosterTemplatesJson;
        private static string R5BoosterTemplatesJson
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_r5BoosterTemplatesJson))
                    return _r5BoosterTemplatesJson;
                
                var bytes = Utils.LoadFromResource("TheArchive.Resources.RundownFiveBoosterTemplates.json");
                _r5BoosterTemplatesJson = Encoding.UTF8.GetString(bytes);
                return _r5BoosterTemplatesJson;
            }
        }
        private static List<BoosterImplantTemplateDataBlock> OldBoosterImplantTemplateDataBlocks = new();

        public class LocalizedTextJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(string.Empty);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return BoosterImplantTemplateDataBlock.UNKNOWN_BLOCK.PublicName;
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(LocalizedText);
            }
        }

        public class ListOfTConverter<T> : JsonConverter<Il2CppSystem.Collections.Generic.List<T>>
        {
            public override Il2CppSystem.Collections.Generic.List<T> ReadJson(JsonReader reader, Type objectType, Il2CppSystem.Collections.Generic.List<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var token = JToken.Load(reader);

                if (token.Type == JTokenType.Array)
                {
                    var list = token.ToObject<List<T>>(serializer);
                    return list.ToIL2CPPListIfNecessary();
                }

                return null;
            }

            public override void WriteJson(JsonWriter writer, Il2CppSystem.Collections.Generic.List<T> value, JsonSerializer serializer)
            {
                var token = JToken.FromObject(value);
                token.WriteTo(writer);
            }
        }

        public class ListOfListOfTConverter<T> : JsonConverter<Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<T>>>
        {
            public override Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<T>> ReadJson(JsonReader reader, Type objectType, Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<T>> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var token = JToken.Load(reader);
                var list = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<T>>();

                if (token.Type == JTokenType.Array)
                {
                    foreach (var innerArrayToken in token.Children())
                    {
                        if (innerArrayToken.Type == JTokenType.Array)
                        {
                            var innerList = new Il2CppSystem.Collections.Generic.List<T>();
                            foreach (var itemToken in innerArrayToken.Children())
                            {
                                var item = itemToken.ToObject<T>(serializer);
                                innerList.Add(item);
                            }
                            list.Add(innerList);
                        }
                    }
                }

                return list;
            }

            public override void WriteJson(JsonWriter writer, Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<T>> value, JsonSerializer serializer)
            {
                var token = JToken.FromObject(value);
                token.WriteTo(writer);
            }
        }

        public class BoosterImplantEffectTemplate
        {
            public BoosterImplantEffectTemplate(BoosterImplantEffectInstance effect)
            {
                EffectMaxValue = effect.MaxValue;
                EffectMinValue = effect.MinValue;
                BoosterImplantEffect = effect.BoosterImplantEffect;
            }

            public uint BoosterImplantEffect { get; set; }
            public float EffectMaxValue { get; set; }
            public float EffectMinValue { get; set; }
        }

        public class BoosterImplantTemplate
        {
            public BoosterImplantTemplate(BoosterImplantTemplateDataBlock block)
            {
                TemplateDataBlock = block;

                BoosterImplantID = block.persistentID;
                ImplantCategory = block.ImplantCategory;

                for (int i = 0; i < block.Effects.Count; i++)
                {
                    Effects.Add(new(block.Effects[i]));
                }
                for (int i = 0; i < block.RandomEffects.Count; i++)
                {
                    List<BoosterImplantEffectTemplate> randomEffects = new();
                    for (int j = 0; j < block.RandomEffects[i].Count; j++)
                    {
                        randomEffects.Add(new(block.RandomEffects[i][j]));
                    }
                    RandomEffects.Add(randomEffects);
                }

                Conditions.AddRange(block.Conditions.ToArray());
                RandomConditions.AddRange(block.RandomConditions.ToArray());

                EffectGroups = GenerateEffectGroups();
                ConditionGroups = GenerateConditionGroups();
            }

            private List<List<BoosterImplantEffectTemplate>> GenerateEffectGroups()
            {
                List<List<BoosterImplantEffectTemplate>> effectGroups = new();

                List<List<BoosterImplantEffectTemplate>> combinations = GetNElementCombinations(RandomEffects);
                for (int i = 0; i < combinations.Count; i++)
                {
                    List<BoosterImplantEffectTemplate> effectGroup = Effects.ToList();
                    effectGroup.AddRange(combinations[i]);
                    effectGroups.Add(effectGroup);
                }
                return effectGroups;
            }

            private List<List<uint>> GenerateConditionGroups()
            {
                List<List<uint>> conditions = new() { Conditions, RandomConditions };
                List<List<uint>> combinations = GetNElementCombinations(conditions);
                return combinations;
            }

            public uint BoosterImplantID { get; set; }
            public BoosterImplantCategory ImplantCategory { get; set; }
            [JsonIgnore]
            public List<BoosterImplantEffectTemplate> Effects { get; set; } = new();
            [JsonIgnore]
            public List<List<BoosterImplantEffectTemplate>> RandomEffects { get; set; } = new();
            [JsonIgnore]
            public List<uint> Conditions { get; set; } = new();
            [JsonIgnore]
            public List<uint> RandomConditions { get; set; } = new();

            [JsonIgnore]
            public BoosterImplantTemplateDataBlock TemplateDataBlock { get; private set; } = null;

            public List<List<BoosterImplantEffectTemplate>> EffectGroups { get; set; } = new();
            public List<List<uint>> ConditionGroups { get; set; } = new();
        }

        private static List<List<T>> GetNElementCombinations<T>(List<List<T>> lists)
        {
            List<List<T>> combinations = new List<List<T>>();

            GetNElementCombinationsHelper(lists, new List<T>(), 0, combinations);

            return combinations;
        }

        private static void GetNElementCombinationsHelper<T>(List<List<T>> lists, List<T> currentCombination, int currentIndex, List<List<T>> combinations)
        {
            if (currentIndex == lists.Count)
            {
                combinations.Add(new List<T>(currentCombination));
                return;
            }

            List<T> currentList = lists[currentIndex];

            if (currentList.Count == 0)
            {
                GetNElementCombinationsHelper(lists, currentCombination, currentIndex + 1, combinations);
                return;
            }

            foreach (T item in currentList)
            {
                currentCombination.Add(item);
                GetNElementCombinationsHelper(lists, currentCombination, currentIndex + 1, combinations);
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }
    }
}