namespace TheArchive.Core;

public interface IArchiveModule
{
    string ModuleGroup { get; }

    void Init();
}