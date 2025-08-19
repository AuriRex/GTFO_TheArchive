using TheArchive.Core.Bootstrap;

namespace TheArchive.Core;

/// <summary>
/// Defines an archive module and makes an assembly eligible for loading via the <see cref="ArchiveModuleChainloader"/>.
/// </summary>
public interface IArchiveModule
{
    /// <summary>
    /// The modules default feature group.
    /// </summary>
    string ModuleGroup { get; }

    /// <summary>
    /// Called once after the module has been loaded.
    /// </summary>
    void Init();
}