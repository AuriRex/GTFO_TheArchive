namespace TheArchive.Core.Models;

/// <summary>
/// Rich presence system states.
/// </summary>
public enum PresenceGameState
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Startup,
    NoLobby,
    InLobby,
    Dropping,
    LevelGenerationFinished,
    InLevel,
    ExpeditionFailed,
    ExpeditionSuccess
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}