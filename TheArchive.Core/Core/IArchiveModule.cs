using TheArchive.Core.Bootstrap;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;

namespace TheArchive.Core;

/// <summary>
/// Defines an archive module and makes an assembly eligible for loading via the <see cref="ArchiveModuleChainloader"/>.
/// </summary>
public interface IArchiveModule
{
    /// <summary>
    /// This modules localization service.
    /// </summary>
    ILocalizationService LocalizationService { get; set; }
    
    /// <summary>
    /// A default logger instance.
    /// </summary>
    IArchiveLogger Logger { get; set; }

    /// <summary>
    /// Called once after the module has been loaded.
    /// </summary>
    void Init();
}