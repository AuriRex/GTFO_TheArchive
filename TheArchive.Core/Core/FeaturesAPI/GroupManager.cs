using System;
using System.Collections.Generic;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Core.Localization.Datas;

namespace TheArchive.Core.FeaturesAPI;

public static class GroupManager
{
    /// <summary>
    /// Gets an existing or creates a new top-level feature group.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <returns>The existing or newly created group.</returns>
    public static TopLevelGroup GetOrCreateTopLevelGroup(string name) =>
        GetOrCreateTopLevelGroupInternal(name, dev: false, null);

    private static TopLevelGroup GetOrCreateTopLevelGroupInternal(string name, bool dev = false, GroupLocalizationData localizationData = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"Parameter '{nameof(name)}' can not be null or white space");
        }

        if (TopLevelGroups.TryGetValue(name, out var group))
            group.IsNewlyCreated = false;

        group ??= new TopLevelGroup(name, dev);

        if (localizationData != null)
            group.AppendLocalization(localizationData);
        
        TopLevelGroups[name] = group;

        return group;
    }

    /// <summary>
    /// Gets an existing or creates a new module/add-on feature group.
    /// </summary>
    /// <param name="module">The module that own this module group.</param>
    /// <param name="localizationData">Optional group localization. (Usually provided via localization files!)</param>
    /// <returns>The existing or newly created group.</returns>
    internal static ModuleGroup GetOrCreateModuleGroup(IArchiveModule module, GroupLocalizationData localizationData = null)
    {
        var asmName = module.GetType().Assembly.GetName().Name;
        var moduleGroupId = $"{asmName}.ModuleGroup";
        if (ModuleGroups.TryGetValue(moduleGroupId, out var group))
            group.IsNewlyCreated = false;

        group ??= new ModuleGroup(module);

        if (localizationData != null)
            group.AppendLocalization(localizationData);

        return group;
    }

    internal static bool TryGetGroup(string identifier, out GroupBase group)
    {
        if (!AllGroups.TryGetValue(identifier, out group))
            return false;
        return true;
    }

    internal static ModuleGroup GetModuleGroup(string asmName)
    {
        if (!ModuleGroups.TryGetValue($"{asmName}.ModuleGroup", out var group))
            return null;
        return group;
    }

    internal static Dictionary<string, TopLevelGroup> TopLevelGroups { get; private set; } = new();
    internal static Dictionary<string, ModuleGroup> ModuleGroups { get; private set; } = new();
    internal static Dictionary<string, GroupBase> AllGroups { get; private set; } = new();

    #region Top-Level Groups
    /// <summary> A feature group for accessibility and ease-of-use features. </summary>
    public static TopLevelGroup Accessibility { get; private set; } = GetOrCreateTopLevelGroupInternal("Accessibility");
    /// <summary> A feature group for audio related features. </summary>
    public static TopLevelGroup Audio { get; private set; } = GetOrCreateTopLevelGroupInternal("Audio");
    /// <summary> A feature group for cosmetic features. </summary>
    public static TopLevelGroup Cosmetic { get; private set; } = GetOrCreateTopLevelGroupInternal("Cosmetic");
    /// <summary> A feature group for developer tools and similar. </summary>
    /// <remarks> This group is hidden by default! </remarks>
    public static TopLevelGroup Dev { get; private set; } = GetOrCreateTopLevelGroupInternal("Developer", true);
    /// <summary> A feature group for features that fix things. </summary>
    public static TopLevelGroup Fixes { get; private set; } = GetOrCreateTopLevelGroupInternal("Fixes");
    /// <summary> A feature group for HUD altering features. </summary>
    public static TopLevelGroup Hud { get; private set; } = GetOrCreateTopLevelGroupInternal("HUD");
    /// <summary>
    /// A feature group for features related to local progression functionality.<br/>
    /// <i>Mostly unused now.</i>
    /// </summary>
    internal static TopLevelGroup LocalProgression { get; private set; } = GetOrCreateTopLevelGroupInternal("LocalProgression"); // , group => group.InlineSettings = true
    /// <summary> A feature group for any special or miscellaneous features that don't fit anywhere else. </summary>
    public static TopLevelGroup Special { get; private set; } = GetOrCreateTopLevelGroupInternal("Misc");
    /// <summary> A feature group for steam/discord rich presence related features. </summary>
    public static TopLevelGroup Presence { get; private set; } = GetOrCreateTopLevelGroupInternal("Presence");
    /// <summary> A feature group for security related features. </summary>
    public static TopLevelGroup Security { get; private set; } = GetOrCreateTopLevelGroupInternal("Security");
    /// <summary> A feature group for QoL features. </summary>
    public static TopLevelGroup QualityOfLife { get; private set; } = GetOrCreateTopLevelGroupInternal("QoL");
    #endregion
}