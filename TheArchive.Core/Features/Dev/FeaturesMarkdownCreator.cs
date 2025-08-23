using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Components;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Utilities;
using UnityEngine;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev;

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
        [FSDisplayName("Use Localized Texts")]
        public FButton UseLocalizedTextsButton { get; set; } = new FButton("False", "use_localized_texts");
        
        [FSDisplayName("Switch Assembly")]
        public FButton NextAssemblyButton { get; set; } = new FButton("Next ASM", "next_asm");
        
        [FSDisplayName("Selected ASM:")]
        public FLabel SelectedAssemblyLabel { get; set; } = new FLabel("///");
        
        [FSSpacer]
        [FSDisplayName("Create Markdown")]
        public FButton CreateMarkdownButton { get; set; } = new FButton("Create Markdown (Clipboard)", "create_markdown");
    }

    public const char NEWLINE = '\n';

    private static Assembly _selectedAssembly;
    private static bool _useLocalizedTexts;

    public override void OnDatablocksReady()
    {
        DiscoverAssemblies();
    }

    public override void OnButtonPressed(ButtonSetting setting)
    {
        switch (setting.ButtonID)
        {
            case "use_localized_texts":
                ToggleUseLocalizedTexts();
                break;
            case "next_asm":
                NextAssembly();
                break;
            case "create_markdown":
                CreateReadme(_selectedAssembly);
                break;
        }
    }

    private void ToggleUseLocalizedTexts()
    {
        _useLocalizedTexts = !_useLocalizedTexts;
        
        Settings?.UseLocalizedTextsButton?.SecondaryText?.SetText($"[ {(_useLocalizedTexts ? "<color=green>" : "<color=red>")}{_useLocalizedTexts}</color> ]");
    }

    private static int _index;
    private static Assembly[] _loadedModules;
    private static void DiscoverAssemblies()
    {
        _loadedModules = ArchiveMod.Modules.Select(m => m.GetType().Assembly).Distinct().ToArray();
        
        SelectAssembly(_loadedModules[0]);
    }
    
    public static void NextAssembly()
    {
        _index++;
        
        if (_index >= _loadedModules.Length)
        {
            _index = 0;
        }
        
        var asm = _loadedModules[_index];
        
        SelectAssembly(asm);
    }

    public static void SelectAssembly(Assembly asm)
    {
        _selectedAssembly = asm;
        Settings?.SelectedAssemblyLabel?.PrimaryText?.SetText($"Current ASM: <color=orange>{asm?.GetName().Name ?? "None"}</color>");
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

            if(group.IsHidden)
            {
                FeatureLogger.Info($"Skipping hidden group \"{groupKvp.Key}\".");
                continue;
            }

            if(group == FeatureGroups.LocalProgression)
            {
                FeatureLogger.Info("Skipping LocalProgression");
                continue;
            }

            var features = groupKvp.Value.Where(f => !f.IsHidden).ToArray();

            if(asmFilter != null)
            {
                features = features.Where(f => f.GetType().Assembly == asmFilter).ToArray();
            }
            
            if (!features.Any())
            {
                FeatureLogger.Info($"Skipping Group \"{group}\": All Features are hidden!");
                continue;
            }

            if (features.Length > 0)
            {
                groups.Add(group, features);
            }
        }

        var orderedGroups = groups.OrderBy(kvp => StripTMPTagsRegex(kvp.Key.Identifier)).ToArray();

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

            features = features.OrderBy(f => StripTMPTagsRegex(f.Name));

            builder.Append(NEWLINE);

            builder.Append(CreateGroupEntry(group));

            foreach(var feature in features)
            {
                builder.Append(CreateFeatureEntry(feature));
            }
        }

        var result = builder.Replace("\\n", NEWLINE.ToString()).ToString();

        GUIUtility.systemCopyBuffer = result;

        FeatureLogger.Notice($"Copied {result.Length} characters to clipboard!");
    }

    public static string CreateQuickLink(FeatureGroup group)
    {
        var name = StripTMPTagsRegex(_useLocalizedTexts ? group.DisplayName : group.Identifier);

        var link = $"#{name.ToLower().Replace(" ", "-").Replace("/", string.Empty)}";

        return $"[{name}]({link})";
    }

    public static string CreateGroupEntry(FeatureGroup group)
    {
        var builder = new StringBuilder();
        builder.Append($"## {StripTMPTagsRegex(_useLocalizedTexts ? group.DisplayName : group.Identifier)}");
        builder.Append(NEWLINE);
        builder.Append(NEWLINE);

        // Descriptions ?

        return builder.ToString();
    }

    public static string CreateFeatureEntry(Feature feature)
    {
        var builder = new StringBuilder();
        builder.Append($"### {Utils.StripTMPTagsRegex(_useLocalizedTexts ? feature.FeatureInternal.DisplayName : feature.Name)}");

        if(feature.AppliesToRundowns != Utils.RundownFlags.None)
        {
            var tag = Utils.GetRundownTag(feature.AppliesToRundowns, true);

            builder.Append($" - `[{tag}]`");
        }

        builder.Append(NEWLINE);
        builder.Append(NEWLINE);

        if (!string.IsNullOrWhiteSpace(_useLocalizedTexts ? feature.FeatureInternal.DisplayDescription : feature.Description))
        {
            builder.Append(Utils.StripTMPTagsRegex(_useLocalizedTexts ? feature.FeatureInternal.DisplayDescription : feature.Description));
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