using System.Collections.Generic;
using System.Linq;
using TheArchive.Core.Localization;

namespace TheArchive.Core.FeaturesAPI;

/// <summary>
/// Feature groupings
/// </summary>
public static class FeatureGroups
{
    #region Setup
    static FeatureGroups()
    {
        Accessibility.SetLanguage(Language.English, "Accessibility");
        Accessibility.SetLanguage(Language.Chinese, "辅助功能");

        ArchiveCore.SetLanguage(Language.English, "Archive Core");
        ArchiveCore.SetLanguage(Language.Chinese, "核心");

        Audio.SetLanguage(Language.English, "Audio");
        Audio.SetLanguage(Language.Chinese, "音频");

        Backport.SetLanguage(Language.English, "Backport");
        Backport.SetLanguage(Language.Chinese, "反向移植");

        Cosmetic.SetLanguage(Language.English, "Cosmetic");
        Cosmetic.SetLanguage(Language.Chinese, "美化");

        Dev.SetLanguage(Language.English, "Developer");
        Dev.SetLanguage(Language.Chinese, "开发者");

        Fixes.SetLanguage(Language.English, "Fixes");
        Fixes.SetLanguage(Language.Chinese, "修复");

        Hud.SetLanguage(Language.English, "HUD / UI");
        Hud.SetLanguage(Language.Chinese, "界面");

        LocalProgression.SetLanguage(Language.English, "Local Progression");
        LocalProgression.SetLanguage(Language.Chinese, "本地进度");

        Special.SetLanguage(Language.English, "Misc");
        Special.SetLanguage(Language.Chinese, "杂项");

        Presence.SetLanguage(Language.English, "Discord / Steam Presence");
        Presence.SetLanguage(Language.Chinese, "Discord / Steam 在线状态");

        Security.SetLanguage(Language.English, "Security / Anti Cheat");
        Security.SetLanguage(Language.Chinese, "安全 / 反作弊");

        QualityOfLife.SetLanguage(Language.English, "Quality of Life");
        QualityOfLife.SetLanguage(Language.Chinese, "生活质量");
    }
    #endregion


    private static FeatureGroup GetOrCreateArchiveGroup(string name, bool dev = false)
    {
        var group = ArchiveCoreGroups.FirstOrDefault(g => g.Name == name, null);

        if (group != null)
            group.IsNewlyCreated = false;

        group ??= new FeatureGroup(name, false, null, dev);

        ArchiveCoreGroups.Add(group);

        return group;
    }

    internal static FeatureGroup GetOrCreateModuleGroup(string name, Dictionary<Language, string> groupLanguages = null)
    {
        var group = ModuleGroups.FirstOrDefault(g => g.Name == name, null);

        if (group != null)
            group.IsNewlyCreated = false;

        group ??= new FeatureGroup(name, true, null);

        if (groupLanguages != null)
            group.SetLanguage(groupLanguages);

        return group;
    }

    internal static FeatureGroup GetGroup(string name) => AllGroups.FirstOrDefault(g => g.Name == name);
    internal static FeatureGroup GetModuleGroup(string name) => ModuleGroups.FirstOrDefault(g => g.Name == name);
    internal static HashSet<FeatureGroup> ArchiveCoreGroups { get; private set; } = new();
    internal static HashSet<FeatureGroup> ModuleGroups { get; private set; } = new();
    internal static HashSet<FeatureGroup> AllGroups { get; private set; } = new();

    #region Archive Groups
    internal static FeatureGroup ArchiveCore { get; private set; } = GetOrCreateArchiveGroup(ArchiveMod.ARCHIVE_CORE_FEATUREGROUP);
    /// <summary> A feature group for accessibility and ease-of-use features. </summary>
    public static FeatureGroup Accessibility { get; private set; } = GetOrCreateArchiveGroup("Accessibility");
    /// <summary> A feature group for audio related features. </summary>
    public static FeatureGroup Audio { get; private set; } = GetOrCreateArchiveGroup("Audio");
    /// <summary>
    /// A feature group for things related to backporting functionalities to older versions.<br/>
    /// <i>Mostly unused now.</i>
    /// </summary>
    public static FeatureGroup Backport { get; private set; } = GetOrCreateArchiveGroup("Backport");
    /// <summary> A feature group for cosmetic features. </summary>
    public static FeatureGroup Cosmetic { get; private set; } = GetOrCreateArchiveGroup("Cosmetic");
    /// <summary> A feature group for developer tools and similar. </summary>
    /// <remarks> This group is hidden by default! </remarks>
    public static FeatureGroup Dev { get; private set; } = GetOrCreateArchiveGroup("Developer", true);
    /// <summary> A feature group for features that fix things. </summary>
    public static FeatureGroup Fixes { get; private set; } = GetOrCreateArchiveGroup("Fixes");
    /// <summary> A feature group for HUD altering features. </summary>
    public static FeatureGroup Hud { get; private set; } = GetOrCreateArchiveGroup("HUD / UI");
    /// <summary>
    /// A feature group for features related to local progression functionality.<br/>
    /// <i>Mostly unused now.</i>
    /// </summary>
    public static FeatureGroup LocalProgression { get; private set; } = GetOrCreateArchiveGroup("Local Progression"); // , group => group.InlineSettings = true
    /// <summary> A feature group for any special or miscellaneous features that don't fit anywhere else. </summary>
    public static FeatureGroup Special { get; private set; } = GetOrCreateArchiveGroup("Misc");
    /// <summary> A feature group for steam/discord rich presence related features. </summary>
    public static FeatureGroup Presence { get; private set; } = GetOrCreateArchiveGroup("Discord / Steam Presence");
    /// <summary> A feature group for security related features. </summary>
    public static FeatureGroup Security { get; private set; } = GetOrCreateArchiveGroup("Security / Anti Cheat");
    /// <summary> A feature group for QoL features. </summary>
    public static FeatureGroup QualityOfLife { get; private set; } = GetOrCreateArchiveGroup("Quality of Life");
    #endregion
}

/// <summary>
/// A feature group.
/// </summary>
public class FeatureGroup
{
    /// <summary> The name of this feature group. </summary>
    public string Name { get; private set; } = string.Empty;
    internal string DisplayName => _languages.GetValueOrDefault(LocalizationCoreService.CurrentLanguage, Name);
    /// <summary> If this feature group has been newly created. </summary>
    public bool IsNewlyCreated { get; internal set; } = true;
    /// <summary> If this feature group is a default module group. </summary>
    public bool IsModuleGroup { get; internal set; }
    /// <summary> Should this feature group be hidden in mod settings? </summary>
    public bool IsHidden { get; private set; }
    /// <summary> The parent group if this is a subgroup. </summary>
    public FeatureGroup ParentGroup { get; }
    /// <summary> Set of subgroups. </summary>
    public HashSet<FeatureGroup> SubGroups { get; } = new();
    internal HashSet<Feature> Features { get; } = new();

    private readonly Dictionary<Language, string> _languages = new();

    /// <summary>
    /// Gets or creates a new subgroup inside of this feature group.
    /// </summary>
    /// <param name="name">The subgroup name.</param>
    /// <param name="isHidden">Should the group be hidden (only visible in dev mode)?</param>
    /// <returns>The already existing subgroup with the provided name or a newly created one if no existing one was found.</returns>
    public FeatureGroup GetOrCreateSubGroup(string name, bool isHidden = false)
    {
        var subGroup = SubGroups.FirstOrDefault(g => g.Name == name);

        if (subGroup != null)
            IsNewlyCreated = false;

        subGroup ??= new FeatureGroup(name, false, this, isHidden);

        return subGroup;
    }

    /// <summary>
    /// Set the groups language translation values using a translation dictionary.
    /// </summary>
    /// <param name="languages">Language dictionary</param>
    public void SetLanguage(Dictionary<Language, string> languages)
    {
        foreach (var lang in languages)
        {
            _languages[lang.Key] = lang.Value;
        }
    }

    /// <summary>
    /// Set a single languages translation value.
    /// </summary>
    /// <param name="language">The language to set.</param>
    /// <param name="text">Translated value.</param>
    public void SetLanguage(Language language, string text)
    {
        _languages[language] = text;
    }

    internal FeatureGroup(string name, bool moduleGroup = false, FeatureGroup parentGroup = null, bool isHidden = false)
    {
        Name = name;
        IsHidden = isHidden;
        if (moduleGroup)
        {
            IsModuleGroup = true;
            FeatureGroups.ModuleGroups.Add(this);
        }
        else if (parentGroup != null)
        {
            ParentGroup = parentGroup;
            ParentGroup.SubGroups.Add(this);
        }
        FeatureGroups.AllGroups.Add(this);
    }

    private FeatureGroup() { }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    /// Returns a string that represents the passed feature group.
    /// </summary>
    /// <param name="g">The feature group.</param>
    /// <returns>A string that represents this feature group.</returns>
    public static implicit operator string(FeatureGroup g) => g?.Name;
}