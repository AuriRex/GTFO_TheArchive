namespace TheArchive.Interfaces;

public interface IInjectLogger
{
    public IArchiveLogger Logger { get; set; }
}