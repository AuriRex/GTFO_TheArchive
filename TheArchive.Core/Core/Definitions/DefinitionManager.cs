using System;
using System.Collections.Generic;
using System.IO;
using TheArchive.Core.Definitions.Data;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Groups;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace TheArchive.Core.Definitions;

internal static class DefinitionManager
{
    public enum DefinitionCategory
    {
        Group
    }

    private static readonly Dictionary<string, Dictionary<DefinitionCategory, object>> _loadedDefinitions = new();
    private static readonly IArchiveLogger _logger = LoaderWrapper.CreateArSubLoggerInstance(nameof(DefinitionManager), ConsoleColor.DarkRed);

    public static void LoadModuleDefinitions(IArchiveModule module)
    {
        var asmName = module.GetType().Assembly.GetName().Name;
        var asmLocation = module.GetType().Assembly.Location;

        _loadedDefinitions.Add(asmName, new());

        if (LoadModuleDefinitionsFile(asmName, asmLocation, out var moduleDefinition)
            && ParseModuleGroupsDefinition(asmName, asmLocation, moduleDefinition, module))
        {
            _loadedDefinitions[asmName][DefinitionCategory.Group] = moduleDefinition;
        }
        else
        {
            GroupManager.GetOrCreateModuleGroup(module);
        }
    }

    private static bool LoadModuleDefinitionsFile(string asmName, string asmLocation, out ModuleGroupsDefinition moduleDefinition)
    {
        moduleDefinition = null;

        // Suggested file naming format: {scope}_{target}_{category}_Definition

        var moduleDefinitionsFileName = $"Module_{asmName}_{DefinitionCategory.Group}_Definition.json";

        try
        {
            var definitionsDir = Path.Combine(Path.GetDirectoryName(asmLocation), "Definitions");
            if (!Directory.Exists(definitionsDir))
                return false;

            var moduleGroupDefinitionFilePath = Path.Combine(definitionsDir, moduleDefinitionsFileName);

            if (!File.Exists(moduleGroupDefinitionFilePath))
                return false;

            moduleDefinition =
                JsonConvert.DeserializeObject<ModuleGroupsDefinition>(File.ReadAllText(moduleGroupDefinitionFilePath),
                    ArchiveMod.JsonSerializerSettings);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to read module definition file: \"{moduleDefinitionsFileName}\".");
            _logger.Exception(ex);
            return false;
        }

        return true;
    }

    private static bool ParseModuleGroupsDefinition(string asmName, string asmLocation, ModuleGroupsDefinition moduleDefinition, IArchiveModule module)
    {
        try
        {
            BuildGroup(module, moduleDefinition.ModuleGroup);

            foreach (var groupDefinition in moduleDefinition.TopLevelGroups)
            {
                BuildGroup(module, groupDefinition);
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to parse module definition for module: \"{asmName}\".");
            _logger.Exception(ex);
            return false;
        }
        return true;
    }

    private static void BuildGroup(IArchiveModule owner, GroupDefinitionBase groupDefinition, GroupBase parentGroup = null)
    {
        if (groupDefinition == null)
            return;

        GroupBase group;
        if (parentGroup == null)
        {
            group = groupDefinition.Type switch
            {
                GroupType.TopLevel => GroupManager.GetOrCreateTopLevelGroup(groupDefinition.Name),
                GroupType.Module => GroupManager.GetOrCreateModuleGroup(owner),
                _ => throw new ArgumentNullException(nameof(parentGroup), $"{groupDefinition.Type}Group must have parent group.")
            };
        }
        else
        {
            group = parentGroup.GetOrCreateSubGroup(groupDefinition.Name, groupDefinition.IsHidden);
        }

        group.AppendLocalization(groupDefinition.LocalizationData);

        switch (groupDefinition.Type)
        {
            case GroupType.Module:
                var moduleGroup = groupDefinition as ModuleGroupDefinition;
                if (moduleGroup.SubGroups != null && moduleGroup.SubGroups.Count > 0)
                {
                    foreach (var subGroup in moduleGroup.SubGroups)
                    {
                        BuildGroup(owner, subGroup, group);
                    }
                }
                break;
            case GroupType.Feature:
                var featureGroup = groupDefinition as FeatureGroupDefinition;
                if (featureGroup.SubGroups != null && featureGroup.SubGroups.Count > 0)
                {
                    foreach (var subGroup in featureGroup.SubGroups)
                    {
                        BuildGroup(owner, subGroup, group);
                    }
                }
                break;
            case GroupType.TopLevel:
            default:
                break;
        }
    }
}
