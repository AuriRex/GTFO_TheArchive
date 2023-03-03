using TheArchive.Utilities;

namespace TheArchive.Models.Progression
{
    public struct ExpeditionCompletionData
    {
        public bool Success => RawSessionData.ExpeditionSurvived;
        public string RundownIdString => RawSessionData.RundownId;
        public uint RundownId { get; internal set; }
        public string ExpeditionId => RawSessionData.ExpeditionId;
        public string SessionId => RawSessionData.SessionId;
        public int ArtifactsCollected => RawSessionData.ArtifactsCollected;
        public bool WasPrisonerEfficiencyClear => RawSessionData.PrisonerEfficiencyCompleted;
        public bool WasFirstTimeCompletion { get; internal set; }
        public ExpeditionSession RawSessionData { get; internal set; }
        /// <summary>
        /// Artifact Heat before this session
        /// </summary>
        public float PreArtifactHeat { get; internal set; }
        /// <summary>
        /// Artifact Heat after session completion
        /// </summary>
        public float NewArtifactHeat { get; internal set; }
    }
}
