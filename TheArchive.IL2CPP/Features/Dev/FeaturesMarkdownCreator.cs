using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev
{
    [DoNotSaveToConfig]
    [DisallowInGameToggle]
    [HideInModSettings]
    internal class FeaturesMarkdownCreator : Feature
    {
        public override string Name => "Features Markdown Creator";

        public override FeatureGroup Group => FeatureGroups.Dev;

        public override string Description => "Used to automatically generate a markdown text containing all Features.\n\n(Copied to clipboard)";

        public new static IArchiveLogger FeatureLogger { get; set; }

        [FeatureConfig]
        public static ReadmeCreatorSettings Settings { get; set; }

        public class ReadmeCreatorSettings
        {
            public FButton CreateMarkdownButton { get; set; } = new FButton("Create Markdown (Clipboard)", "create_markdown");
        }

        public const string NEWLINE = "\n";

        public override void OnButtonPressed(ButtonSetting setting)
        {
            if(setting.ButtonID == "create_markdown")
            {
                CreateReadme();
            }
        }

        public static void CreateReadme(Assembly asmFilter = null)
        {
            var groups = new Dictionary<FeatureGroup, IEnumerable<Feature>>();

            var builder = new StringBuilder();

            foreach(var groupKvp in FeatureManager.Instance.GroupedFeatures)
            {
                var group = FeatureGroups.GetGroup(groupKvp.Key);

                if(group == null)
                {
                    FeatureLogger.Warning($"Attempted to resolve unknown group \"{groupKvp.Key}\".");
                    continue;
                }


                if(group == FeatureGroups.Dev)
                {
                    FeatureLogger.Info("Skipping Dev Group!");
                    continue;
                }

                if(group == FeatureGroups.LocalProgression)
                {
                    FeatureLogger.Info("Skipping LocalProgression");
                    continue;
                }

                var features = groupKvp.Value.Where(f => !f.IsHidden);

                if (features.Count() == 0)
                {
                    FeatureLogger.Info($"Skipping Group \"{group}\": All Features are hidden!");
                    continue;
                }

                groups.Add(group, features);
            }

            var orderedGroups = groups.OrderBy(kvp => StripTMPTagsRegex(kvp.Key.Name));

            foreach(var entry in orderedGroups)
            {
                var group = entry.Key;
                builder.Append($"  * {CreateQuickLink(group)}\n");
            }

            builder.Append(NEWLINE);

            foreach (var entry in orderedGroups)
            {
                var group = entry.Key;
                var features = entry.Value;

                if(asmFilter != null)
                {
                    features = features.Where(f => f.GetType().Assembly == asmFilter);
                }

                features = features.OrderBy(f => StripTMPTagsRegex(f.Name));

                builder.Append(NEWLINE);

                builder.Append(CreateGroupEntry(group));

                foreach(var feature in features)
                {
                    builder.Append(CreateFeatureEntry(feature));
                }
            }

            var result = builder.Replace("\\n", NEWLINE).ToString();

            GUIUtility.systemCopyBuffer = result;

            FeatureLogger.Notice($"Copied {result.Length} characters to clipboard!");
        }

        public static string CreateQuickLink(FeatureGroup group)
        {
            var name = StripTMPTagsRegex(group.Name);

            var link = $"#{name.ToLower().Replace(" ", "-").Replace("/", string.Empty)}";

            return $"[{name}]({link})";
        }

        public static string CreateGroupEntry(FeatureGroup group)
        {
            var builder = new StringBuilder();
            builder.Append($"## {StripTMPTagsRegex(group.Name)}");
            builder.Append(NEWLINE);
            builder.Append(NEWLINE);

            // Descriptions ?

            return builder.ToString();
        }

        public static string CreateFeatureEntry(Feature feature)
        {
            var builder = new StringBuilder();
            builder.Append($"### {Utils.StripTMPTagsRegex(feature.Name)}");

            if(feature.AppliesToRundowns != Utils.RundownFlags.None)
            {
                var tag = Utils.GetRundownTag(feature.AppliesToRundowns, true);

                builder.Append($" - `[{tag}]`");
            }

            builder.Append(NEWLINE);
            builder.Append(NEWLINE);

            if (!string.IsNullOrWhiteSpace(feature.Description))
            {
                builder.Append(Utils.StripTMPTagsRegex(feature.Description));
            }
            else
            {
                builder.Append(Tagged("description missing!"));
            }

            builder.Append(NEWLINE);
            builder.Append(NEWLINE);

            return builder.ToString();
        }

        private static string Tagged(string tag, bool todo = true)
        {
            return $"[ {(todo ? "TODO: " : string.Empty)}{tag} ]";
        }
    }
}
