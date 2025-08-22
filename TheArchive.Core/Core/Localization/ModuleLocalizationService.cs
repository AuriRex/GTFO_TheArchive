using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Interfaces;

namespace TheArchive.Core.Localization;

internal class ModuleLocalizationService : BaseLocalizationService
{
    private readonly IArchiveModule _archiveModule;
    private ModuleLocalizationData _localizationData;

    public Type ModuleType { get; }
    
    public ModuleLocalizationService(IArchiveModule archiveModule, Type moduleType, IArchiveLogger logger) : base(logger, moduleType.FullName)
    {
        _archiveModule = archiveModule;
        ModuleType = moduleType;
    }

    internal void Setup()
    {
        var location = ModuleType.Assembly.Location;

        if (string.IsNullOrWhiteSpace(location))
        {
            location = Path.Combine(Paths.ConfigPath, "Localization/");
        }
        
        var localizationFolderPath = Path.Combine(Path.GetDirectoryName(location)!, "Localization/");
        
        if (!Directory.Exists(localizationFolderPath))
            Directory.CreateDirectory(localizationFolderPath);

        // Underscore at the beginning so it shows up on top if sorted by file name.
        var localizationFileName = $"_{ModuleType.Name}_ModuleLocalization.json";
        
        var filePath = Path.Combine(localizationFolderPath, localizationFileName);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(new ModuleLocalizationData(), ArchiveMod.JsonSerializerSettings));
        }
        
        _localizationData = JsonConvert.DeserializeObject<ModuleLocalizationData>(File.ReadAllText(filePath), ArchiveMod.JsonSerializerSettings);
        
        var moduleGroup = FeatureGroups.GetOrCreateModuleGroup($"{ModuleType.FullName}.ModuleGroup");
        
        moduleGroup.SetLanguage(_localizationData.ModuleGroup);
        
        LocalizationCoreService.RegisterLocalizationService(this);
    }
    
    public override string Get(uint id)
    {
        if (!_localizationData.TryGetGenericText(id, CurrentLanguage, out var text))
        {
            return $"UNKNOWN ID: {id}";
        }
        
        return text;
    }

    public override string Get<T>(T value)
    {
        var type = typeof(T);
        
        if (type.IsEnum)
        {
            if (!_localizationData.EnumTexts.TryGetValue(type.FullName!, out var languages) || !languages.Entries.TryGetValue(CurrentLanguage, out var enumTexts) || enumTexts.Any(p => string.IsNullOrWhiteSpace(p.Value)))
            {
                return value.ToString();
            }
            List<string> result = new();
            foreach (var v in Enum.GetValues(type))
            {
                if (!value.HasFlag((T)v))
                    continue;
                if (!enumTexts.TryGetValue(v.ToString()!, out var tvalue))
                {
                    return value.ToString();
                }
                result.Add(tvalue);
            }
            return string.Join(", ", result);
        }
        
        return Get(type, value);
    }

    public override string Get(Type type, object value)
    {
        return value.ToString();
    }
}