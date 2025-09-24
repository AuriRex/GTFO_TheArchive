using BepInEx;
using System;
using System.IO;
using System.Reflection;
using TheArchive.Core.Localization.Data;
using TheArchive.Interfaces;

namespace TheArchive.Core.Localization.Services;

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

    public void Setup()
    {
        var location = ModuleType.Assembly.Location;

        if (string.IsNullOrWhiteSpace(location))
        {
            location = Path.Combine(Paths.ConfigPath, "Localization/");
        }
        
        var localizationFolderPath = Path.Combine(Path.GetDirectoryName(location)!, "Localization/");
        
        if (!Directory.Exists(localizationFolderPath))
            Directory.CreateDirectory(localizationFolderPath);

        var asmName = ModuleType.Assembly.GetName().Name;
        
        // Underscore at the beginning so it shows up on top if sorted by file name.
        var localizationFileName = $"Module_{asmName}_Localization.json";
        
        var filePath = Path.Combine(localizationFolderPath, localizationFileName);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(new ModuleLocalizationData(), ArchiveMod.JsonSerializerSettings));
        }
        
        _localizationData = JsonConvert.DeserializeObject<ModuleLocalizationData>(File.ReadAllText(filePath), ArchiveMod.JsonSerializerSettings);

        ArchiveLocalizationService.RegisterLocalizationService(this);
    }

    public override string GetById(uint id, string fallback = null)
    {
        if (!_localizationData.TryGetGenericText(id, CurrentLanguage, out var text))
        {
            if (!string.IsNullOrWhiteSpace(fallback))
                return fallback;
            return $"UNKNOWN ID: {id}";
        }
        
        return text;
    }

    public override string Get<T>(T value)
    {
        var type = typeof(T);

        if (type.IsEnum)
        {
            if (!_localizationData.EnumTexts.TryGetValue(type.FullName!, out var languages)
                || !languages.Entries.TryGetValue(CurrentLanguage, out var enumTexts))
            {
                return value.ToString();
            }

            bool isFlags = type.GetCustomAttribute<FlagsAttribute>() != null;

            if (isFlags)
            {
                return GetFlagsEnumText(type, value, enumTexts);
            }
            else
            {
                return GetSingleEnumText(value, enumTexts);
            }
        }

        return Get(type, value);
    }

    public override string Get(Type type, object value)
    {
        return value.ToString();
    }
}