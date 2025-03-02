namespace TheArchive.Core.Models;

public enum PresenceGameState
{
    Startup,
    NoLobby,
    InLobby,
    Dropping,
    LevelGenerationFinished,
    InLevel,
    ExpeditionFailed,
    ExpeditionSuccess
}